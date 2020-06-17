using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.ML;
using ML.Core;
using ML.Infrastructure.DependecyResolution;
using ML.Utils.Extensions.Base;

namespace ML.ClassificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .ConfigureLogging(
            options => options.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Information))
            .ConfigureServices((hostContext, services) =>
            {
                IConfiguration Configuration = hostContext.Configuration;

                services.AddHostedService<Worker>()
                .Configure<EventLogSettings>(config =>
                {
                    config.LogName = "Sample Service";
                    config.SourceName = "Sample Service Source";
                });

                services.RegisterServices(Configuration);

            }).UseWindowsService();
        }

        
    }
}
