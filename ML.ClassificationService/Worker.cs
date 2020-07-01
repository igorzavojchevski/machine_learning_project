using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ML.BL;
using ML.BL.Helpers;
using ML.BL.Interfaces;
using ML.BL.Mongo.Concrete;
using ML.BL.Mongo.Interfaces;
using ML.ImageClassification.Train.Interfaces;

namespace ML.ClassificationService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IFrameExporterService _frameExporterService;
        private readonly ILabelScoringService _labelScoringService;
        private readonly IScoringServiceFactory _ScoringServiceFactory;

        public Worker(
            ILogger<Worker> logger,
            IFrameExporterService frameExporterService,
            ILabelScoringService labelScoringService,
            IScoringServiceFactory ScoringServiceFactory
            )
        {
            _logger = logger;
            _frameExporterService = frameExporterService;
            _labelScoringService = labelScoringService;
            _ScoringServiceFactory = ScoringServiceFactory;
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
            _logger.LogInformation("Classification Service Starting");

            Thread classificationService = new Thread(Score);
            classificationService.Start();

            return base.StartAsync(cancellationToken);
        }

        public void Score()
        {
            while (true)
            {
                try
                {

                    if (ServiceHelper.SystemSettingService.IsTrainingServiceStarted)
                    {
                        Thread.Sleep(TimeSpan.FromMinutes(5));
                        continue;
                    }

                    //Used for testing only
                    //LabelScoring on TensorFlowInception
                    //string imagesPath = @"C:\Users\igor.zavojchevski\Desktop\Master\TestMaterial\Frames\Panda"; //this should be output of frameexporterservice
                    //_labelScoringService.Score(imagesPath);

                    //Evaluation over Custom Logo
                    IScoringService sync = _ScoringServiceFactory.Create(ScoringServiceType.LogoScoring);
                    sync.Score();
                    //ServiceHelper.SystemSettingService.CUSTOMLOGOMODEL_ExportedFromService_ImagesToEvaluateFolderPath

                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ClassificationService - Score exception");
                }
            }
        }
    }
}
