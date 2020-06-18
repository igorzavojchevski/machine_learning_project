using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Domain.Entities.Mongo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.BL.Concrete
{
    public class LabelScoringService : ILabelScoringService
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<LabelScoringService> _logger;
        private readonly IAdvertisementService _advertisementService;
        private readonly ITensorFlowInceptionLabelScoringService _tensorFlowInceptionLabelScoringService;

        public LabelScoringService(
            IConfiguration configuration,
            ILogger<LabelScoringService> logger,
            IAdvertisementService advertisementService,
            ITensorFlowInceptionLabelScoringService tensorFlowInceptionLabelScoringService)
        {
            Configuration = configuration;
            _logger = logger;
            _advertisementService = advertisementService;
            _tensorFlowInceptionLabelScoringService = tensorFlowInceptionLabelScoringService;
        }

        public void Score(string imagesToCheckPath)
        {
            FileInfo[] Images = GetListOfImages(imagesToCheckPath);

            if (Images == null || Images.Length == 0) { _logger.LogDebug("Score - No Images provided"); return; }

            Guid GroupGuid = Guid.NewGuid();

            List<List<FileInfo>> chunkedList = ChunkImagesInGroups(Images);
            foreach (List<FileInfo> chunk in chunkedList)
            {
                Task.Factory.StartNew(() =>
                Parallel.ForEach<FileInfo>(chunk, image =>
                {
                    DoLabelScoring(GroupGuid, image);
                }));
            }
        }

        private void DoLabelScoring(Guid GroupGuid, FileInfo image)
        {
            ImagePredictedLabelWithProbability prediction = _tensorFlowInceptionLabelScoringService.DoLabelScoring(image);
            SaveImageScoringInfo(prediction, GroupGuid);
        }

        private List<List<FileInfo>> ChunkImagesInGroups(FileInfo[] Images)
        {
            int chunks = Configuration.GetValue<int>("MLModel:MaxChunksToProcessAtOnce"); // do this as system settings
            return Images.Select((value, i) => new { Index = i, Value = value }).GroupBy(t => t.Index / (chunks != 0 ? chunks : Images.Length)).Select(t => t.Select(v => v.Value).ToList()).ToList();
        }

        private FileInfo[] GetListOfImages(string imagesToCheckPath)
        {
            DirectoryInfo di = new DirectoryInfo(imagesToCheckPath);
            FileInfo[] Images = di.GetFiles();
            return Images;
        }

        private void SaveImageScoringInfo(ImagePredictedLabelWithProbability prediction, Guid GroupGuid)
        {
            Advertisement ad = new Advertisement
            {
                GroupGuid = GroupGuid,
                PredictedLabel = prediction.PredictedLabel,
                MaxProbability = prediction.MaxProbability,
                TopProbabilities = prediction.TopProbabilities,
                PredictionExecutionTime = prediction.PredictionExecutionTime,
                ImageId = prediction.ImageId,
                ModifiedBy = "SaveImageScoringInfoService",
                ModifiedOn = DateTime.UtcNow
            };

            _advertisementService.InsertOne(ad);
        }
    }
}
