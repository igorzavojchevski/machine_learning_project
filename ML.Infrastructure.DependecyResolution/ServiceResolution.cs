using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using ML.Core;
using ML.Domain.DataModels;
using ML.Infrastructure.DataContext;
using System;

namespace ML.Infrastructure.DependecyResolution
{
    public static class ServiceResolution
    {
        public static IServiceCollection AddInternalServices(this IServiceCollection services)
        {
            services.AddSingleton<IMongoDbContext, MongoDbContext>();

            return services;
        }

        public static IServiceCollection AddExternalService(this IServiceCollection services, ITransformer _mlnetModel)
        {
            // Register the PredictionEnginePool as a service in the IoC container for DI.
            services.AddPredictionEnginePool<ImageInputData, ImageLabelPredictions>();
            services
                .AddOptions<PredictionEnginePoolOptions<ImageInputData, ImageLabelPredictions>>()
                .Configure(options => { options.ModelLoader = new InMemoryModelLoader(_mlnetModel); });


            //TO-DO REWORK THIS PART FOR MULTIPLE MODELS

            return services;
        }
    }
}
