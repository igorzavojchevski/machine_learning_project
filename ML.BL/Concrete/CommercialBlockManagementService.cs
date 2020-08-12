using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.Entities.Mongo;
using ML.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ML.BL.Concrete
{
    public class CommercialBlockManagementService : ICommercialBlockManagementService
    {
        private readonly ILogger<CommercialBlockManagementService> _logger;
        private readonly ICommercialBlockService _commercialBlockService;
        private readonly ICommercialService _commercialService;
        private readonly IEvaluationStreamService _evaluationStreamService;

        public CommercialBlockManagementService(ILogger<CommercialBlockManagementService> logger, ICommercialBlockService commercialBlockService, ICommercialService commercialService, IEvaluationStreamService evaluationStreamService)
        {
            _logger = logger;
            _commercialBlockService = commercialBlockService;
            _commercialService = commercialService;
            _evaluationStreamService = evaluationStreamService;
        }

        public void MakeCommercialBlocks()
        {
            try
            {
                _logger.LogInformation("MakeCommercialBlocks - Start");

                DateTime? lastCommercialBlockDateChecked = _commercialBlockService.GetAll().Any() ? _commercialBlockService.GetAll().Max(t => t.EndDate) : (DateTime?)null;

                if (!lastCommercialBlockDateChecked.HasValue) lastCommercialBlockDateChecked = DateTime.UtcNow;

                lastCommercialBlockDateChecked -= TimeSpan.FromMinutes(1);

                _logger.LogInformation("MakeCommercialBlocks - lastCommercialBlockDateChecked=" + lastCommercialBlockDateChecked.ToString());

                List<Commercial> commercials = _commercialService.GetAll().Where(t => t.ImageDateTime <= lastCommercialBlockDateChecked && !t.PredictedLabel.ToLower().StartsWith("new_item")).ToList();
                if (commercials == null || !commercials.Any()) return;

                List<EvaluationStream> evaluationStreams = _evaluationStreamService.GetAll().ToList() ?? new List<EvaluationStream>(); 

                List<CommercialBlock> groupedCommercials = commercials
                    .OrderBy(t => t.ImageDateTime).GroupWhile((preceeding, next) => preceeding.PredictedLabel == next.PredictedLabel)
                    .Select(t =>
                    new CommercialBlock()
                    {
                        Label = t.Select(tt => tt.PredictedLabel).FirstOrDefault(),
                        CommercialIDs = t.Select(tt => tt.Id.ToString()).ToList(),
                        StartDate = t.Min(tt => tt.ImageDateTime),
                        EndDate = t.Max(tt => tt.ImageDateTime),
                        ModifiedOn = DateTime.UtcNow,
                        ModifiedBy = "CommercialBlockService",
                        EvaluationStreamID = t.Select(tt => tt.EvaluationStreamId).FirstOrDefault(),
                        EvaluationStreamName = evaluationStreams.Where(es => es.Id == t.Select(tt => tt.EvaluationStreamId).FirstOrDefault()).Select(t => t.Name).FirstOrDefault()
                    })
                    .ToList();

                _commercialBlockService.InsertMany(groupedCommercials);

                _logger.LogInformation("MakeCommercialBlocks - End");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MakeCommercialBlocks - Error");
            }
        }


        public void MakeCommercialVideosForBlocks()
        {

        }
    }
}
