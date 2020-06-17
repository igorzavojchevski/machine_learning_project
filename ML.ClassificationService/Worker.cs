using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;

namespace ML.ClassificationService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IFrameExporterService _frameExporterService;
        private readonly ILabelScoringService _labelScoringService;

        public Worker(ILogger<Worker> logger, IFrameExporterService frameExporterService, ILabelScoringService labelScoringService)
        {
            _logger = logger;
            _frameExporterService = frameExporterService;
            _labelScoringService = labelScoringService;
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

            string imagesPath = @"C:\Users\igor.zavojchevski\Desktop\Master\TestMaterial\Frames\Panda"; //this should be output of frameexporterservice

            _labelScoringService.Score(imagesPath);

            return base.StartAsync(cancellationToken);
        }
    }
}
