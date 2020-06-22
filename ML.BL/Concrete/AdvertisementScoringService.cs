﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Domain.DataModels.CustomLogoTrainingModel;
using ML.Domain.DataModels.Models;
using ML.Domain.Entities.Mongo;
using System;
using System.Collections.Generic;
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

        public AdvertisementScoringService(
            ILogger<ScoringService> logger,
            PredictionEnginePool<InMemoryImageData, ImagePrediction> predictionEnginePool,
            IAdvertisementService advertisementService,
            ISystemSettingService systemSettingService)
            : base(logger, systemSettingService)
        {
            _logger = logger;
            _predictionEnginePool = predictionEnginePool;
            _advertisementService = advertisementService;
            _systemSettingService = systemSettingService;
        }

        public override void Score(string imagesToCheckPath)
        {
            _logger.LogInformation("AdvertisementScoringService - Score started");

            base.Score(imagesToCheckPath);

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

            if (string.IsNullOrWhiteSpace(label) || label.ToLower() == "none") label = "New_Item_";

            label = label.Contains("_") ? label : $"{label}_{GroupGuid}";

            string destOutputPath = Path.Combine(_systemSettingService.CUSTOMLOGOMODEL_TrainedImagesFolderPath, label);
            Directory.CreateDirectory(destOutputPath);

            string sourcePath = advByGuid.Select(t => t.ImageDirPath).FirstOrDefault();
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
                }
            }
        }

        private void SaveImageScoringInfo(InMemoryImageData image, ImagePrediction prediction, Guid GroupGuid)
        {
            Advertisement ad = new Advertisement
            {
                GroupGuid = GroupGuid,
                ImageId = image.ImageFileName,
                ImageFilePath = image.ImageFilePath,
                ImageDirPath = image.ImageDirPath,
                PredictedLabel = prediction.PredictedLabel,
                MaxProbability = prediction.Score.Max(),
                ModifiedBy = "AdvertisementScoringService",
                ModifiedOn = DateTime.UtcNow
            };

            _advertisementService.InsertOne(ad);
        }
    }
}