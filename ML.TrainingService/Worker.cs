using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.ImageClassification.Train.Interfaces;

namespace ML.TrainingService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITrainingService _trainingService;
        private readonly IArchivingService _archivingService;

        public Worker(ILogger<Worker> logger, ITrainingService trainingService, IArchivingService archivingService)
        {
            _logger = logger;
            _trainingService = trainingService;
            _archivingService = archivingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
                await Task.CompletedTask;
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Training Service Starting");

            Thread training = new Thread(Training);
            training.Start();

            Thread archiving = new Thread(Archiving);
            archiving.Start();

            return base.StartAsync(cancellationToken);
        }

        public void Archiving()
        {
            while (true)
            {
                _archivingService.ArchiveImages();
                Thread.Sleep(TimeSpan.FromMinutes(60));
            }
        }

        public void Training()
        {
            while (true)
            {
                TrainingProcess();
                Thread.Sleep(TimeSpan.FromMinutes(30)); //make scheduled execution
            }
        }

        public void TrainingProcess()
        {
            try
            {
                //Do before training start activities - (set flag for start etc.)
                _trainingService.DoBeforeTrainingStart();

                //Do cleanup for not used items
                _trainingService.DoCleanup();

                //Training for Logo Custom
                _trainingService.Train();

                //Do after training finished activities- (set flag for start etc.)
                _trainingService.DoAfterTrainingFinished();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "TrainingService exception");
            }
        }
    }
}
