using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Domain.DataModels.CustomLogoTrainingModel;
using ML.Domain.DataModels.Models;
using ML.Domain.Entities.Enums;
using ML.Domain.Entities.Mongo;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ML.BL.Concrete
{
    public class CommercialScoringService : ScoringService, ICommercialScoringService
    {
        private readonly ILogger<ScoringService> _logger;
        private readonly PredictionEnginePool<InMemoryImageData, ImagePrediction> _predictionEnginePool;
        private readonly ICommercialService _commercialService;
        private readonly ISystemSettingService _systemSettingService;
        private readonly ILabelClassService _labelClassService;

        public CommercialScoringService(
            ILogger<ScoringService> logger,
            PredictionEnginePool<InMemoryImageData, ImagePrediction> predictionEnginePool,
            ICommercialService commercialService,
            ISystemSettingService systemSettingService,
            IEvaluationGroupService evaluationGroupService,
            ILabelClassService labelClassService)
            : base(logger, systemSettingService, evaluationGroupService)
        {
            _logger = logger;
            _predictionEnginePool = predictionEnginePool;
            _commercialService = commercialService;
            _systemSettingService = systemSettingService;
            _labelClassService = labelClassService;
        }

        public override void Score()
        {
            _logger.LogInformation("CommercialScoringService - Score started");

            base.Score();

            _logger.LogInformation("CommercialScoringService - Score finished");
        }

        public override void DoLabelScoring(Guid GroupGuid, InMemoryImageData image, ObjectId evaluationStreamID)
        {
            ImagePrediction prediction = PredictImage(image);
            SaveImageScoringInfo(image, prediction, GroupGuid, ClassifiedBy.ClassificationService, evaluationStreamID);
        }

        public ImagePrediction PredictImage(InMemoryImageData image)
        {
            ImagePrediction prediction = _predictionEnginePool.Predict(image);
            return prediction;
        }

        public void SaveImageScoringInfo(InMemoryImageData image, ImagePrediction prediction, Guid GroupGuid, ClassifiedBy classifiedBy, ObjectId? evaluationStreamID = null)
        {
            Commercial ad = new Commercial
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
                EvaluationStreamId = evaluationStreamID,
                ClassifiedBy = classifiedBy,
                ModifiedBy = "CommercialScoringService",
                ModifiedOn = DateTime.UtcNow
            };

            _commercialService.InsertOne(ad);
        }

        public override void GroupByLabel(Guid GroupGuid)
        {
            List<Commercial> cmrsByGuid = _commercialService.GetAll(t => t.GroupGuid == GroupGuid).ToList();
            if (cmrsByGuid == null || cmrsByGuid.Count == 0) { _logger.LogInformation("CommercialScoringService - GroupByLabel - cmrsByGuid contains no elements"); return; }

            List<(string KeyLabel, int CountLabel)> cmrsGroups = cmrsByGuid.OrderByDescending(t => t.MaxProbability).GroupBy(g => g.PredictedLabel).Select(a => (a.Key, a.Count())).ToList();

            string label = string.Empty;

            if (((float)cmrsGroups.Max(t => t.CountLabel) / cmrsByGuid.Count) >= _systemSettingService.ClassGroupThreshold)
                label = cmrsGroups.Select(t => t.KeyLabel).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(label) || label.ToLower() == "none") label = $"New_Item_{GroupGuid}";

            label = label.Contains("_") ? label : $"{label}_{GroupGuid}";

            string destOutputPath = CheckLabelClassOutputDirectory(label, GroupGuid);
            //string destOutputPath = Path.Combine(_systemSettingService.CUSTOMLOGOMODEL_TrainedImagesFolderPath, label)
            if(!Directory.Exists(destOutputPath)) Directory.CreateDirectory(destOutputPath);

            string sourcePath = cmrsByGuid.Select(t => t.OriginalImageDirPath).FirstOrDefault();
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

                    UpdateCommercial(cmrsByGuid, destOutputPath, destFile, fileName, label);
                }
            }
        }

        private void UpdateCommercial(List<Commercial> cmrsByGuid, string destOutputPath, string destFile, string fileName, string label)
        {
            var commercialItem = cmrsByGuid.First(t => t.ImageId == fileName);
            commercialItem.ImageFilePath = destFile;
            commercialItem.ImageDirPath = destOutputPath;
            commercialItem.PredictedLabel = label;
            _commercialService.Update(commercialItem);
        }

        private string CheckLabelClassOutputDirectory(string label, Guid GroupGuid)
        {
            LabelClass lastOldLabelClass = 
                _labelClassService
                .GetAll()
                .Where(t => t.ClassName == label)
                .OrderByDescending(t => t.TrainingVersion)
                .OrderByDescending(t => t.Version)
                .FirstOrDefault();

            if (lastOldLabelClass == null) 
            {
                string newPath = Path.Combine(_systemSettingService.CUSTOMLOGOMODEL_TrainedImagesFolderPath, label);
                int lastTrainingVersion = _labelClassService.GetAll().Any() ? _labelClassService.GetAll().Max(t => t.TrainingVersion) : 0;

                LabelClass newLabelClass = new LabelClass()
                {
                    ClassName = label,
                    CategoryType = "Default", //make this enum in future
                    ImagesGroupGuid = GroupGuid,
                    DirectoryPath = newPath,
                    TrainingVersion = lastTrainingVersion,
                    Version = 1,
                    IsChanged = false,
                    ModifiedBy = "CommercialScoringService - evaluation/classification process",
                    ModifiedOn = DateTime.UtcNow
                };

                _labelClassService.InsertOne(newLabelClass);

                newLabelClass.FirstVersionId = newLabelClass.Id;
                newLabelClass.ModifiedOn = DateTime.UtcNow;

                _labelClassService.Update(newLabelClass);

                return newPath; 
            }
            
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
    }
}
