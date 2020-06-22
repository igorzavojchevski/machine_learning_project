using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using ML.Infrastructure.DependecyResolution;

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
                    config.LogName = "ClassificationService Service";
                    config.SourceName = "ClassificationService Service Source";
                });

                services.RegisterServices(Configuration);

            }).UseWindowsService();
        }

        
    }
}
