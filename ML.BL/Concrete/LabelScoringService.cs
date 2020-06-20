using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public class LabelScoringService : ScoringService, ILabelScoringService
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger<LabelScoringService> _logger;
        private readonly IAdvertisementService _advertisementService;
        private readonly ITensorFlowInceptionLabelScoringService _tensorFlowInceptionLabelScoringService;

        public LabelScoringService(
            IConfiguration configuration,
            ILogger<LabelScoringService> logger,
            IAdvertisementService advertisementService,
            ITensorFlowInceptionLabelScoringService tensorFlowInceptionLabelScoringService)
            : base(configuration, logger)
        {
            Configuration = configuration;
            _logger = logger;
            _advertisementService = advertisementService;
            _tensorFlowInceptionLabelScoringService = tensorFlowInceptionLabelScoringService;
        }

        public override void Score(string imagesToCheckPath)
        {
            base.Score(imagesToCheckPath);
        }

        public override void DoLabelScoring(Guid GroupGuid, InMemoryImageData image)
        {
            ImagePredictedLabelWithProbability prediction = _tensorFlowInceptionLabelScoringService.DoLabelScoring(image);
            SaveImageScoringInfo(prediction, GroupGuid);
        }

        //public void Score(string imagesToCheckPath)
        //{
        //    //FileInfo[] Images = GetListOfImages(imagesToCheckPath);
        //    IEnumerable<InMemoryImageData> Images = BaseExtensions.LoadInMemoryImagesFromDirectory(imagesToCheckPath, false);

        //    if (Images == null || Images.Count() == 0) { _logger.LogDebug("Score - No Images provided"); return; }

        //    Guid GroupGuid = Guid.NewGuid();

        //    //List<List<FileInfo>> chunkedList = ChunkImagesInGroups(Images);
        //    List<List<InMemoryImageData>> chunkedList = ChunkImagesInGroups(Images);
        //    foreach (List<InMemoryImageData> chunk in chunkedList)
        //    {
        //        Task.Factory.StartNew(() =>
        //        Parallel.ForEach<InMemoryImageData>(chunk, image =>
        //        {
        //            DoLabelScoring(GroupGuid, image);
        //        }));
        //    }
        //}

        //private void DoLabelScoring(Guid GroupGuid, InMemoryImageData image)
        //{
        //    ImagePredictedLabelWithProbability prediction = _tensorFlowInceptionLabelScoringService.DoLabelScoring(image);
        //    SaveImageScoringInfo(prediction, GroupGuid);
        //}

        //private List<List<FileInfo>> ChunkImagesInGroups(FileInfo[] Images)
        //{
        //    int chunks = Configuration.GetValue<int>("MLModel:MaxChunksToProcessAtOnce"); // do this as system settings
        //    return Images.Select((value, i) => new { Index = i, Value = value }).GroupBy(t => t.Index / (chunks != 0 ? chunks : Images.Length)).Select(t => t.Select(v => v.Value).ToList()).ToList();
        //}

        //private List<List<InMemoryImageData>> ChunkImagesInGroups(IEnumerable<InMemoryImageData> Images)
        //{
        //    int chunks = Configuration.GetValue<int>("MLModel:MaxChunksToProcessAtOnce"); // do this as system settings
        //    return Images.Select((value, i) => new { Index = i, Value = value }).GroupBy(t => t.Index / (chunks != 0 ? chunks : Images.Count())).Select(t => t.Select(v => v.Value).ToList()).ToList();
        //}

        //private FileInfo[] GetListOfImages(string imagesToCheckPath)
        //{
        //    DirectoryInfo di = new DirectoryInfo(imagesToCheckPath);
        //    FileInfo[] Images = di.GetFiles();
        //    return Images;
        //}

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
