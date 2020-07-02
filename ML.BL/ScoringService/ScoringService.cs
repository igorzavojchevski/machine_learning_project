using Microsoft.Extensions.Logging;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Domain.Entities.Enums;
using ML.Domain.Entities.Mongo;
using ML.Utils.Extensions.Base;
using MongoDB.Bson;
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
        private readonly IEvaluationGroupService _evaluationGroupService;

        public ScoringService(
            ILogger<ScoringService> logger,
            ISystemSettingService systemSettingService,
            IEvaluationGroupService evaluationGroupService)
        {
            _logger = logger;
            _systemSettingService = systemSettingService;
            _evaluationGroupService = evaluationGroupService;
        }

        public virtual void Score()
        {
            EvaluationGroup evaluationGroup = _evaluationGroupService.GetAll().Where(t => t.Status == TrainingStatus.New).OrderBy(t => t.ModifiedOn).FirstOrDefault();
            if (evaluationGroup == null) { _logger.LogInformation("ScoringService - Score - No trainingGroup with NEW status"); return; }

            if(string.IsNullOrWhiteSpace(evaluationGroup.DirPath)) { _logger.LogInformation("ScoringService - Score - Invalid EvaluationGroupDirPath"); return; }

            evaluationGroup.Status = TrainingStatus.Processing;
            evaluationGroup.ModifiedOn = DateTime.UtcNow;
            _evaluationGroupService.Update(evaluationGroup);

            IEnumerable<InMemoryImageData> Images = BaseExtensions.LoadInMemoryImagesFromDirectory(evaluationGroup.DirPath, false);

            if (Images == null || Images.Count() == 0) { _logger.LogInformation("ScoringService - Score - No Images provided"); return; }

            Guid GroupGuid = evaluationGroup.EvaluationGroupGuid; //Guid.NewGuid();

            List<Task<ParallelLoopResult>> listOfTasks = new List<Task<ParallelLoopResult>>();

            List<List<InMemoryImageData>> chunkedList = ChunkImagesInGroups(Images);
            foreach (List<InMemoryImageData> chunk in chunkedList)
            {
                var task = Task.Factory.StartNew(() =>
                Parallel.ForEach<InMemoryImageData>(chunk, image =>
                {
                    DoLabelScoring(GroupGuid, image, evaluationGroup.EvaluationStreamId);
                }));
                listOfTasks.Add(task);
            }

            Task.WaitAll(listOfTasks.ToArray());

            GroupByLabel(GroupGuid);

            evaluationGroup.Status = TrainingStatus.Processed;
            evaluationGroup.ModifiedOn = DateTime.UtcNow;
            _evaluationGroupService.Update(evaluationGroup);
        }

        public virtual void DoLabelScoring(Guid GroupGuid, InMemoryImageData image, ObjectId evaluationStreamID)
        {
            throw new NotImplementedException();
        }

        public virtual void GroupByLabel(Guid GroupGuid)
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
