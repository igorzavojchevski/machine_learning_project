﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using ML.BL;
using ML.BL.Concrete;
using ML.BL.Helpers;
using ML.BL.Interfaces;
using ML.BL.Mongo.Concrete;
using ML.BL.Mongo.Interfaces;
using ML.Core;
using ML.Core.TensorFlowInception;
using ML.Domain.DataModels.TFLabelScoringModel;
using ML.Domain.DataModels.CustomLogoTrainingModel;
using ML.ImageClassification.Train.Concrete;
using ML.ImageClassification.Train.Interfaces;
using ML.Infrastructure.DataContext;
using ML.Infrastructure.Interfaces;
using ML.Infrastructure.Repositories;
using ML.Utils;
using ML.Utils.Extensions.Base;
using ML.Domain.DataModels;

namespace ML.Infrastructure.DependecyResolution
{
    public static class ServiceResolution
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration Configuration, bool skipFuncServices = false)
        {
            services.AddSingleton<IMongoDbContext, MongoDbContext>();

            services.AddRepositories();
            services.AddServices();

            if (skipFuncServices) return services;

            services.AddFunctionalService();

            services.InstanceMLEngine();

            return services;
        }

        //Repositories
        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<ISystemSettingRepository, SystemSettingRepository>();
            services.AddTransient<ICommercialRepository, CommercialRepository>();
            services.AddTransient<ICommercialBlockRepository, CommercialBlockRepository>();
            services.AddTransient<ILabelClassRepository, LabelClassRepository>();
            services.AddTransient<IEvaluationGroupRepository, EvaluationGroupRepository>();
            services.AddTransient<IEvaluationStreamRepository, EvaluationStreamRepository>();

            return services;
        }

        //Services
        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<ISystemSettingService, SystemSettingService>();
            services.AddTransient<ICommercialService, CommercialService>();
            services.AddTransient<BL.Mongo.Interfaces.ICommercialBlockService, BL.Mongo.Concrete.CommercialBlockService>();
            services.AddTransient<ILabelClassService, LabelClassService>();
            services.AddTransient<IEvaluationGroupService, EvaluationGroupService>();
            services.AddTransient<IEvaluationStreamService, EvaluationStreamService>();
            services.AddTransient<IArchivingService, ArchivingService>();

            services.AddTransient<IFrameExporterService, FrameExporterService>();

            ServiceProviderHelper.ServiceProvider = services.BuildServiceProvider();

            return services;
        }



        private static IServiceCollection AddFunctionalService(this IServiceCollection services)
        {
            services.AddTransient<ITrainingService, TrainingService>();

            services.AddTransient<IScoringServiceFactory, ScoringServiceFactory>();
            services.AddTransient<ICommercialScoringService, CommercialScoringService>();
            services.AddTransient<ILabelScoringService, LabelScoringService>();
            services.AddHttpClient<BL.Interfaces.ICommercialBlockService, BL.Concrete.CommercialBlockService>();
            services.AddTransient<BL.Interfaces.ICommercialBlockService, BL.Concrete.CommercialBlockService>();

            return services;
        }

        //MLEngine
        private static IServiceCollection InstanceMLEngine(this IServiceCollection services)
        {
            services.InstanceInceptionTensorFlowModel();
            services.InstanceLogoClassificationModel();

            return services;
        }

        private static IServiceCollection InstanceLogoClassificationModel(this IServiceCollection services)
        {
            services.AddPredictionEnginePool<InMemoryImageData, ImagePrediction>()
                .FromFile(filePath: ServiceHelper.SystemSettingService.CUSTOMLOGOMODEL_ModelFilePath, watchForChanges: true);
            return services;
        }

        #region TensorFlowInceptionModel
        //PrepareTensorFlowInceptionModel
        private static IServiceCollection InstanceInceptionTensorFlowModel(this IServiceCollection services)
        {
            // Register the PredictionEnginePool as a service in the IoC container for DI.
            services.AddPredictionEnginePool<ImageInputData, ImageLabelPredictions>();
            services
                .AddOptions<PredictionEnginePoolOptions<ImageInputData, ImageLabelPredictions>>()
                .Configure(options => { options.ModelLoader = new InMemoryModelLoader(GetTensorFlowInceptionMLModel()); });
            return services;
        }
        //PrepareTensorFlowInceptionModel
        private static ITransformer GetTensorFlowInceptionMLModel()
        {
            //Configure the ML.NET model for the pre-trained Inception TensorFlow model.
            string _tensorFlowModelFilePath = BaseExtensions.GetPath(ServiceHelper.SystemSettingService.TF_ModelFilePath);
            TensorFlowInceptionModelConfigurator tensorFlowInceptionModelConfigurator = new TensorFlowInceptionModelConfigurator(_tensorFlowModelFilePath);
            ITransformer _InceptionMLNetModel = tensorFlowInceptionModelConfigurator.Model;
            return _InceptionMLNetModel;
        }
        #endregion
    }
}
