using Microsoft.Extensions.Logging;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Utils.Extensions.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ML.BL
{
    public abstract class ScoringService : IScoringService
    {
        private readonly ILogger<ScoringService> _logger;
        private readonly ISystemSettingService _systemSettingService;

        public ScoringService(
            ILogger<ScoringService> logger,
            ISystemSettingService systemSettingService)
        {
            _logger = logger;
            _systemSettingService = systemSettingService;
        }

        public virtual void Score(string imagesToCheckPath)
        {
            IEnumerable<InMemoryImageData> Images = BaseExtensions.LoadInMemoryImagesFromDirectory(imagesToCheckPath, false);

            if (Images == null || Images.Count() == 0) { _logger.LogDebug("ScoringService - Score - No Images provided"); return; }

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
            int chunks = _systemSettingService.MaxChunksToProcessAtOnce;
            return Images.Select((value, i) => new { Index = i, Value = value }).GroupBy(t => t.Index / (chunks != 0 ? chunks : Images.Count())).Select(t => t.Select(v => v.Value).ToList()).ToList();
        }
    }
}
