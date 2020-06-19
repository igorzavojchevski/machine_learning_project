using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Domain.DataModels.TrainingModels;
using ML.Domain.Entities.Mongo;
using ML.Utils.Extensions.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.BL.Concrete
{
    public class LogoLabelScoringService : ILogoLabelScoringService
    {

        //private static string[] labels = null;
        //private static float? MaxProbabilityThreshold = null;
        public IConfiguration Configuration { get; }
        private readonly ILogger<LogoLabelScoringService> _logger;
        private readonly PredictionEnginePool<InMemoryImageData, ImagePrediction> _predictionEnginePool;
        private readonly IAdvertisementService _advertisementService;

        public LogoLabelScoringService(
            PredictionEnginePool<InMemoryImageData, ImagePrediction> predictionEnginePool,
            IConfiguration configuration,
            ILogger<LogoLabelScoringService> logger,
            IAdvertisementService advertisementService)
        {
            Configuration = configuration;
            _logger = logger;
            _predictionEnginePool = predictionEnginePool;
            _advertisementService = advertisementService;
            //MaxProbabilityThreshold = Configuration.GetValue<float?>("MLModel:MaxProbabilityThreshold"); // do this as system settings
        }

        public void Score(string imagesToCheckPath)
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

        private void DoLabelScoring(Guid GroupGuid, InMemoryImageData image)
        {
            ImagePrediction prediction = _predictionEnginePool.Predict(image);
            //ImagePredictedLabelWithProbability prediction = _tensorFlowInceptionLabelScoringService.DoLabelScoring(image);
            SaveImageScoringInfo(prediction, GroupGuid);
        }

        private List<List<InMemoryImageData>> ChunkImagesInGroups(IEnumerable<InMemoryImageData> Images)
        {
            int chunks = Configuration.GetValue<int>("MLModel:MaxChunksToProcessAtOnce"); // do this as system settings
            return Images.Select((value, i) => new { Index = i, Value = value }).GroupBy(t => t.Index / (chunks != 0 ? chunks : Images.Count())).Select(t => t.Select(v => v.Value).ToList()).ToList();
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

        private Dictionary<string, float> GetTopLabels(string[] labels, float[] probs)
        {
            Dictionary<string, float> topLabels = new Dictionary<string, float>();

            IEnumerable<float> probabilities = probs.OrderByDescending(t => t).Take(Configuration.GetValue<int>("MLLogoModel:MaxLabels"));

            for (int i = 0; i < probabilities.Count(); i++)
            {
                if (labels.Length == i) break;
                string test = labels[probs.AsSpan().IndexOf(probs[i])];
                if (topLabels.ContainsKey(test)) continue;
                topLabels.Add(test, probs[i]);
            }
            return topLabels;
        }
    }
}
