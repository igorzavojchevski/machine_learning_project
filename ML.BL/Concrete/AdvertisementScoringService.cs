using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Domain.DataModels.CustomLogoTrainingModel;
using ML.Domain.DataModels.Models;
using ML.Domain.Entities.Mongo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ML.BL.Concrete
{
    public class AdvertisementScoringService : ScoringService, IAdvertisementScoringService
    {
        private readonly ILogger<ScoringService> _logger;
        private readonly PredictionEnginePool<InMemoryImageData, ImagePrediction> _predictionEnginePool;
        private readonly IAdvertisementService _advertisementService;
        private readonly ISystemSettingService _systemSettingService;
        private readonly ILabelClassService _labelClassService;

        public AdvertisementScoringService(
            ILogger<ScoringService> logger,
            PredictionEnginePool<InMemoryImageData, ImagePrediction> predictionEnginePool,
            IAdvertisementService advertisementService,
            ISystemSettingService systemSettingService,
            IEvaluationGroupService evaluationGroupService,
            ILabelClassService labelClassService)
            : base(logger, systemSettingService, evaluationGroupService)
        {
            _logger = logger;
            _predictionEnginePool = predictionEnginePool;
            _advertisementService = advertisementService;
            _systemSettingService = systemSettingService;
            _labelClassService = labelClassService;
        }

        public ImagePrediction CheckImageAndDoLabelScoring(InMemoryImageData image)
        {
            ImagePrediction prediction = _predictionEnginePool.Predict(image);

            MemoryStream ms = new MemoryStream(image.Image);
            Image i = Image.FromStream(ms);

            string destOutputPath = Path.Combine(_systemSettingService.CUSTOMLOGOMODEL_TrainedImagesFolderPath, prediction.PredictedLabel);
            Directory.CreateDirectory(destOutputPath);

            string imageFilePath = Path.Combine(destOutputPath, image.ImageFileName);

            i.Save(imageFilePath);

            InMemoryImageData newImage =
                new InMemoryImageData(image.Image, prediction.PredictedLabel, image.ImageFileName, imageFilePath, destOutputPath, image.ImageDateTime);

            //do separate logic for saving
            SaveImageScoringInfo(newImage, prediction, Guid.NewGuid(), true);

            return prediction;
        }

        public override void Score()
        {
            _logger.LogInformation("AdvertisementScoringService - Score started");

            base.Score();

            _logger.LogInformation("AdvertisementScoringService - Score finished");
        }

        public override void DoLabelScoring(Guid GroupGuid, InMemoryImageData image)
        {
            ImagePrediction prediction = _predictionEnginePool.Predict(image);
            SaveImageScoringInfo(image, prediction, GroupGuid);
        }

        public override void GroupByLabel(Guid GroupGuid)
        {
            List<Advertisement> advByGuid = _advertisementService.GetAll(t => t.GroupGuid == GroupGuid).ToList();
            if (advByGuid == null || advByGuid.Count == 0) { _logger.LogInformation("AdvertisementScoringService - GroupByLabel - advByGuid contains no elements"); return; }

            List<(string KeyLabel, int CountLabel)> advGroups = advByGuid.OrderByDescending(t => t.MaxProbability).GroupBy(g => g.PredictedLabel).Select(a => (a.Key, a.Count())).ToList();

            string label = string.Empty;

            if (((float)advGroups.Max(t => t.CountLabel) / advByGuid.Count) >= _systemSettingService.ClassGroupThreshold)
                label = advGroups.Select(t => t.KeyLabel).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(label) || label.ToLower() == "none") label = $"New_Item_{GroupGuid}";

            label = label.Contains("_") ? label : $"{label}_{GroupGuid}";

            string destOutputPath = CheckLabelClassOutputDirectory(label);
            //string destOutputPath = Path.Combine(_systemSettingService.CUSTOMLOGOMODEL_TrainedImagesFolderPath, label)
            Directory.CreateDirectory(destOutputPath);

            string sourcePath = advByGuid.Select(t => t.OriginalImageDirPath).FirstOrDefault();
            if (System.IO.Directory.Exists(sourcePath))
            {
                string[] files = System.IO.Directory.GetFiles(sourcePath);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    string fileName = System.IO.Path.GetFileName(s);
                    string destFile = System.IO.Path.Combine(destOutputPath, fileName);
                    System.IO.File.Copy(s, destFile, true);

                    UpdateAdvertisement(advByGuid, destOutputPath, destFile, fileName);
                }
            }
        }

        private void UpdateAdvertisement(List<Advertisement> advByGuid, string destOutputPath, string destFile, string fileName)
        {
            var advertisementItem = advByGuid.First(t => t.ImageId == fileName);
            advertisementItem.ImageFilePath = destFile;
            advertisementItem.ImageDirPath = destOutputPath;
            _advertisementService.Update(advertisementItem);
        }

        private string CheckLabelClassOutputDirectory(string label)
        {
            LabelClass lastOldLabelClass = 
                _labelClassService
                .GetAll()
                .Where(t => t.ClassName == label)
                .OrderByDescending(t => t.TrainingVersion)
                .OrderByDescending(t => t.Version)
                .FirstOrDefault();

            if (lastOldLabelClass == null) return Path.Combine(_systemSettingService.CUSTOMLOGOMODEL_TrainedImagesFolderPath, label);
            
            if (!lastOldLabelClass.IsChanged) return lastOldLabelClass.DirectoryPath;

            LabelClass labelClassAfterEdit = 
                _labelClassService
                .GetAll()
                .Where(t => t.FirstVersionId == lastOldLabelClass.FirstVersionId)
                .OrderByDescending(t => t.TrainingVersion)
                .OrderByDescending(t => t.Version)
                .FirstOrDefault();

            return labelClassAfterEdit.DirectoryPath;
        }

        //private void InsertAdvertisementClass(string destOutputPath, string label, Guid GroupGuid)
        //{
        //    if (!_labelClassService.GetAll().Any(t => t.ClassName == label))
        //        _labelClassService.InsertOne(new LabelClass() 
        //        {
        //            ClassName = label,
        //            CategoryType = "Default", //make this enum in future
        //            ImagesGroupGuid = GroupGuid,
        //            DirectoryPath = destOutputPath,
        //            ModifiedBy = "GroupByLabel - InsertAdvertisementClass",
        //            ModifiedOn = DateTime.UtcNow
        //        });
        //}

        private void SaveImageScoringInfo(InMemoryImageData image, ImagePrediction prediction, Guid GroupGuid, bool isCustom = false)
        {
            Advertisement ad = new Advertisement
            {
                GroupGuid = GroupGuid,
                ImageId = image.ImageFileName,
                OriginalImageFilePath = image.ImageFilePath,
                OriginalImageDirPath = image.ImageDirPath,
                ImageFilePath = image.ImageFilePath,
                ImageDirPath = image.ImageDirPath,
                ImageDateTime = image.ImageDateTime,
                PredictedLabel = prediction.PredictedLabel,
                MaxProbability = prediction.Score.Max(),
                IsCustom = isCustom,
                ModifiedBy = "AdvertisementScoringService",
                ModifiedOn = DateTime.UtcNow
            };

            _advertisementService.InsertOne(ad);
        }
    }
}
