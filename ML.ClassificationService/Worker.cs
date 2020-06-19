using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.ImageClassification.Train.Interfaces;

namespace ML.ClassificationService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IFrameExporterService _frameExporterService;
        private readonly ILabelScoringService _labelScoringService;
        private readonly ITrainService _trainService;
        private readonly ILogoLabelScoringService _logoLabelScoringService;

        public Worker(
            ILogger<Worker> logger, 
            IFrameExporterService frameExporterService, 
            ILabelScoringService labelScoringService, 
            ITrainService trainService,
            ILogoLabelScoringService logoLabelScoringService
            )
        {
            _logger = logger;
            _frameExporterService = frameExporterService;
            _labelScoringService = labelScoringService;
            _trainService = trainService;
            _logoLabelScoringService = logoLabelScoringService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
                await Task.CompletedTask;
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service Starting");

            //TODO - REWORK THIS
            //_frameExporterService.Export();

            //LabelScoring on TensorFlowInception
            //string imagesPath = @"C:\Users\igor.zavojchevski\Desktop\Master\TestMaterial\Frames\Panda"; //this should be output of frameexporterservice
            //_labelScoringService.Score(imagesPath);

            //Training for Logo Custom
            //_trainService.Train();

            //Evaluation over Custom Logo
            string imagesFolderPathForPredictions = @"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\inputs\images_for_prediction";
            _logoLabelScoringService.Score(imagesFolderPathForPredictions);

            return base.StartAsync(cancellationToken);
        }
    }
}
