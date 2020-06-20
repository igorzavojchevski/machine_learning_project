using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels.TrainingModels;
using ML.Domain.Entities.Mongo;
using ML.Utils.Extensions.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.BL
{
    public abstract class ScoringService : IScoringService
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger<ScoringService> _logger;
        //private readonly PredictionEnginePool<InMemoryImageData, ImagePrediction> _predictionEnginePool;

        public ScoringService(
            //PredictionEnginePool<InMemoryImageData, ImagePrediction> predictionEnginePool,
            IConfiguration configuration,
            ILogger<ScoringService> logger)
        {
            Configuration = configuration;
            _logger = logger;
            //_predictionEnginePool = predictionEnginePool;
            //MaxProbabilityThreshold = Configuration.GetValue<float?>("MLModel:MaxProbabilityThreshold"); // do this as system settings
        }

        public virtual void Score(string imagesToCheckPath)
        {
            IEnumerable<InMemoryImageData> Images = BaseExtensions.LoadInMemoryImagesFromDirectory(imagesToCheckPath, false);

            if (Images == null || Images.Count() == 0) { _logger.LogDebug("Score - No Images provided"); return; }

            Guid GroupGuid = Guid.NewGuid();

            List<List<InMemoryImageData>> chunkedList = ChunkImagesInGroups(Images);
            foreach (List<InMemoryImageData> chunk in chunkedList)
            {
                Task.Factory.StartNew(() =>
                Parallel.ForEach<InMemoryImageData>(chunk, image =>
                {
                    DoLabelScoring(GroupGuid, image);
                }));
            }
        }

        public virtual void DoLabelScoring(Guid GroupGuid, InMemoryImageData image)
        {
            throw new NotImplementedException();
        }

        private List<List<InMemoryImageData>> ChunkImagesInGroups(IEnumerable<InMemoryImageData> Images)
        {
            int chunks = Configuration.GetValue<int>("MLModel:MaxChunksToProcessAtOnce"); // do this as system settings
            return Images.Select((value, i) => new { Index = i, Value = value }).GroupBy(t => t.Index / (chunks != 0 ? chunks : Images.Count())).Select(t => t.Select(v => v.Value).ToList()).ToList();
        }

        ////Do instancing by type here
        //private void DoLabelScoring(Guid GroupGuid, InMemoryImageData image)
        //{
        //    ImagePrediction prediction = _predictionEnginePool.Predict(image);
        //    //ImagePredictedLabelWithProbability prediction = _tensorFlowInceptionLabelScoringService.DoLabelScoring(image);
        //    SaveImageScoringInfo(prediction, GroupGuid);
        //}

        //private void SaveImageScoringInfo(ImagePrediction prediction, Guid GroupGuid)
        //{
        //    Advertisement ad = new Advertisement
        //    {
        //        GroupGuid = GroupGuid,
        //        PredictedLabel = prediction.PredictedLabel,
        //        MaxProbability = prediction.Score.Max(),
        //        ModifiedBy = "SaveImageScoringInfoService",
        //        ModifiedOn = DateTime.UtcNow
        //    };

        //    _advertisementService.InsertOne(ad);
        //}
    }
}
