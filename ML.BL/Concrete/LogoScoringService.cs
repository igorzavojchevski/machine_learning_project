using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Domain.DataModels.CustomLogoTrainingModel;
using ML.Domain.Entities.Mongo;
using System;
using System.Linq;

namespace ML.BL.Concrete
{
    public class LogoScoringService : ScoringService, ILogoScoringService
    {
        private readonly ILogger<ScoringService> _logger;
        private readonly PredictionEnginePool<InMemoryImageData, ImagePrediction> _predictionEnginePool;
        private readonly IAdvertisementService _advertisementService;

        public LogoScoringService(
            ILogger<ScoringService> logger, 
            PredictionEnginePool<InMemoryImageData, ImagePrediction> predictionEnginePool,
            IAdvertisementService advertisementService,
            ISystemSettingService systemSettingService)
            :base(logger, systemSettingService)
        {
            _logger = logger;
            _predictionEnginePool = predictionEnginePool;
            _advertisementService = advertisementService;
       }

        public override void Score(string imagesToCheckPath)
        {
            _logger.LogInformation("LogoScoringService - Score started");

            base.Score(imagesToCheckPath);

            _logger.LogInformation("LogoScoringService - Score finished");
        }

        public override void DoLabelScoring(Guid GroupGuid, InMemoryImageData image)
        {
            ImagePrediction prediction = _predictionEnginePool.Predict(image);
            SaveImageScoringInfo(prediction, GroupGuid);
        }

        private void SaveImageScoringInfo(ImagePrediction prediction, Guid GroupGuid)
        {
            Advertisement ad = new Advertisement
            {
                GroupGuid = GroupGuid,
                PredictedLabel = prediction.PredictedLabel,
                MaxProbability = prediction.Score.Max(),
                ModifiedBy = "SaveImageScoringInfoService",
                ModifiedOn = DateTime.UtcNow
            };

            _advertisementService.InsertOne(ad);
        }
    }
}
