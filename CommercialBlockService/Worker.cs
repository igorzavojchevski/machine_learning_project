using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;

namespace CommercialBlockService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ICommercialBlockManagementService _commercialBlockManagementService;

        public Worker(ILogger<Worker> logger, ICommercialBlockManagementService commercialBlockManagementService)
        {
            _logger = logger;
            _commercialBlockManagementService = commercialBlockManagementService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CommercialBlockService Starting");

            Thread commercialBlockService = new Thread(CommercialBlockService);
            commercialBlockService.Start();

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        public void CommercialBlockService()
        {
            while (true)
            {
                _commercialBlockManagementService.MakeCommercialBlocks();
                _commercialBlockManagementService.MakeCommercialVideosForBlocks();
                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }
    }
}
