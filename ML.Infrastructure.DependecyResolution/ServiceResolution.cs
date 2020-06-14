using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using ML.BL.Concrete;
using ML.BL.Interfaces;
using ML.BL.Mongo.Concrete;
using ML.BL.Mongo.Interfaces;
using ML.Core;
using ML.Domain.DataModels;
using ML.Infrastructure.DataContext;
using ML.Infrastructure.Interfaces;
using ML.Infrastructure.Repositories;
using System;

namespace ML.Infrastructure.DependecyResolution
{
    public static class ServiceResolution
    {
        //TODO - Rework this for separate logic for repositories/services/functional services
        public static IServiceCollection AddInternalServices(this IServiceCollection services)
        {
            services.AddSingleton<IMongoDbContext, MongoDbContext>();

            services.AddTransient<IFrameExporterService, FrameExporterService>();
            services.AddTransient<IAdvertisementRepository, AdvertisementRepository>();
            services.AddTransient<IAdvertisementService, AdvertisementService>();
            
            services.AddTransient<ILabelScoringService, LabelScoringService>();
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
