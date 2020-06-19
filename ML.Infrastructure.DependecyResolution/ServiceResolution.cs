using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using ML.BL.Concrete;
using ML.BL.Interfaces;
using ML.BL.Mongo.Concrete;
using ML.BL.Mongo.Interfaces;
using ML.Core;
using ML.Core.TensorFlowInception;
using ML.Domain.DataModels;
using ML.Domain.DataModels.TrainingModels;
using ML.ImageClassification.Train.Concrete;
using ML.ImageClassification.Train.Interfaces;
using ML.Infrastructure.DataContext;
using ML.Infrastructure.Interfaces;
using ML.Infrastructure.Repositories;
using ML.Utils.Extensions.Base;

namespace ML.Infrastructure.DependecyResolution
{
    public static class ServiceResolution
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddSingleton<IMongoDbContext, MongoDbContext>();

            services.AddRepositories();
            services.AddServices();

            services.InstanceMLEngine(Configuration);

            return services;
        }

        //Repositories
        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IAdvertisementRepository, AdvertisementRepository>();
            return services;
        }

        //Services
        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IFrameExporterService, FrameExporterService>();
            services.AddTransient<IAdvertisementService, AdvertisementService>();
            services.AddTransient<ITensorFlowInceptionLabelScoringService, TensorFlowInceptionLabelScoringService>();
            services.AddTransient<ILabelScoringService, LabelScoringService>();
            services.AddTransient<ITrainService, TrainService>();
            services.AddTransient<ILogoLabelScoringService, LogoLabelScoringService>();

            return services;
        }

        //MLEngine
        private static IServiceCollection InstanceMLEngine(this IServiceCollection services, IConfiguration Configuration)
        {
            services.InstanceInceptionTensorFlowModel(Configuration);
            services.InstanceLogoClassificationModel(Configuration);

            return services;
        }

        private static IServiceCollection InstanceLogoClassificationModel(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddPredictionEnginePool<InMemoryImageData, ImagePrediction>().FromFile(Configuration["MLLogoModel:MLModelFilePath"]);
            return services;
        }

        #region TensorFlowInceptionModel
        //PrepareTensorFlowInceptionModel
        private static IServiceCollection InstanceInceptionTensorFlowModel(this IServiceCollection services, IConfiguration Configuration)
        {
            // Register the PredictionEnginePool as a service in the IoC container for DI.
            services.AddPredictionEnginePool<ImageInputData, ImageLabelPredictions>();
            services
                .AddOptions<PredictionEnginePoolOptions<ImageInputData, ImageLabelPredictions>>()
                .Configure(options => { options.ModelLoader = new InMemoryModelLoader(GetTensorFlowInceptionMLModel(Configuration)); });
            return services;
        }
        //PrepareTensorFlowInceptionModel
        private static ITransformer GetTensorFlowInceptionMLModel(IConfiguration Configuration)
        {
            //Configure the ML.NET model for the pre-trained Inception TensorFlow model.
            string _tensorFlowModelFilePath = BaseExtensions.GetPath(
                Configuration["MLModel:TensorFlowModelFilePath"],
                Configuration.GetValue<bool>("MLModel:IsAbsolute"));
            TensorFlowInceptionModelConfigurator tensorFlowInceptionModelConfigurator = new TensorFlowInceptionModelConfigurator(_tensorFlowModelFilePath);
            ITransformer _InceptionMLNetModel = tensorFlowInceptionModelConfigurator.Model;
            return _InceptionMLNetModel;
        }
        #endregion
    }
}
