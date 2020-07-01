using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using ML.Infrastructure.DependecyResolution;
using NLog.Web;

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
            .ConfigureLogging(loggingBuilder =>
            {
                //options.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Information);
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            })
            .ConfigureServices((hostContext, services) =>
            {
                IConfiguration Configuration = hostContext.Configuration;

                services.AddHostedService<Worker>()
                .Configure<EventLogSettings>(config =>
                {
                    config.LogName = "ClassificationService Service";
                    config.SourceName = "ClassificationService Service Source";
                });

                services.RegisterServices(Configuration);

            })
            .UseNLog()
            .UseWindowsService();
        }

        
    }
}
