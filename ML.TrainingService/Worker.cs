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
        private readonly ITrainingService _trainService;

        public Worker(ILogger<Worker> logger, ITrainingService trainService)
        {
            _logger = logger;
            _trainService = trainService;
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
                _trainService.DoBeforeTrainingStart();

                //Training for Logo Custom
                _trainService.Train();

                //Do after training finished activities- (set flag for start etc.)
                _trainService.DoAfterTrainingFinished();

                Thread.Sleep(TimeSpan.FromMinutes(10)); //make scheduled execution
            }
        }
    }
}
