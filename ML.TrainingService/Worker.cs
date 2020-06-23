using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ML.ImageClassification.Train.Interfaces;

namespace ML.TrainingService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITrainingService _trainingService;

        public Worker(ILogger<Worker> logger, ITrainingService trainingService)
        {
            _logger = logger;
            _trainingService = trainingService;
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

            return base.StartAsync(cancellationToken);
        }

        public void Training()
        {
            while (true)
            {
                //Do before training start activities - (set flag for start etc.)
                _trainingService.DoBeforeTrainingStart();

                //Training for Logo Custom
                _trainingService.Train();

                //Do after training finished activities- (set flag for start etc.)
                _trainingService.DoAfterTrainingFinished();

                Thread.Sleep(TimeSpan.FromMinutes(10)); //make scheduled execution
            }
        }
    }
}
