using ML.Domain.Entities.Mongo;
using ML.Infrastructure.DataContext;
using ML.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ML.Infrastructure.Repositories
{
    public class SystemSettingRepository : MongoBaseRepository<SystemSetting>, ISystemSettingRepository
    {
        private static bool first_time_executed = false;
        public SystemSettingRepository(IMongoDbContext mongoDbContext) : base(mongoDbContext)
        {
            if (!first_time_executed) FirstInsert();
        }

        public void FirstInsert()
        {
            if (!GetAll().Any(t => t.SettingKey == "FFMPEG_ExecutablePath"))
                InsertOne(new SystemSetting { SettingKey = "FFMPEG_ExecutablePath", SettingValue = @"C:\ffmpeg\bin\ffmpeg.exe", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            if (!GetAll().Any(t => t.SettingKey == "Archive_Path_For_Trained_Images"))
                InsertOne(new SystemSetting { SettingKey = "Archive_Path_For_Trained_Images", SettingValue = @"E:\ML_Archive", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            if (!GetAll().Any(t => t.SettingKey == "Archive_LastStartDate"))
                InsertOne(new SystemSetting { SettingKey = "Archive_LastStartDate", SettingValue = @"2020-06-20 00:00:00", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            if (!GetAll().Any(t => t.SettingKey == "Archive_NextStartPeriod_Minutes"))
                InsertOne(new SystemSetting { SettingKey = "Archive_NextStartPeriod_Minutes", SettingValue = @"4320", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            if (!GetAll().Any(t => t.SettingKey == "ClassGroupThreshold"))
                InsertOne(new SystemSetting { SettingKey = "ClassGroupThreshold", SettingValue = @"0.9", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            if (!GetAll().Any(t => t.SettingKey == "MaxChunksToProcessAtOnce"))
                InsertOne(new SystemSetting { SettingKey = "MaxChunksToProcessAtOnce", SettingValue = @"3", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            if (!GetAll().Any(t => t.SettingKey == "ExportService_ExportPeriod_Seconds"))
                InsertOne(new SystemSetting { SettingKey = "ExportService_ExportPeriod_Seconds", SettingValue = @"60", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });


            if (!GetAll().Any(t => t.SettingKey == "TF_LabelsFilePath"))
                InsertOne(new SystemSetting { SettingKey = "TF_LabelsFilePath", SettingValue = "C:\\Users\\igor.zavojchevski\\Desktop\\Master\\ML\\assets\\MLModels\\TensorFlowModel\\imagenet_comp_graph_label_strings.txt", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });
            
            if (!GetAll().Any(t => t.SettingKey == "TF_TensorFlowModelFilePath"))
                InsertOne(new SystemSetting { SettingKey = "TF_TensorFlowModelFilePath", SettingValue = "C:\\Users\\igor.zavojchevski\\Desktop\\Master\\ML\\assets\\MLModels\\TensorFlowModel\\tensorflow_inception_graph.pb", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            
            if (!GetAll().Any(t => t.SettingKey == "CUSTOMLOGOMODEL_ModelFilePath"))
                InsertOne(new SystemSetting { SettingKey = "CUSTOMLOGOMODEL_ModelFilePath", SettingValue = "C:\\Users\\igor.zavojchevski\\Desktop\\Master\\ML\\assets\\Training\\outputs\\imageClassifier.zip", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });
            
            if (!GetAll().Any(t => t.SettingKey == "CUSTOMLOGOMODEL_OutputFilePath"))
                InsertOne(new SystemSetting { SettingKey = "CUSTOMLOGOMODEL_OutputFilePath", SettingValue = @"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\outputs\imageClassifier.zip", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });
            
            if (!GetAll().Any(t => t.SettingKey == "CUSTOMLOGOMODEL_TrainedImagesFolderPath"))
                InsertOne(new SystemSetting { SettingKey = "CUSTOMLOGOMODEL_TrainedImagesFolderPath", SettingValue = @"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\inputs\images_to_train", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            if (!GetAll().Any(t => t.SettingKey == "CUSTOMLOGOMODEL_ImagesToTestAfterTrainingFolderPath"))
                InsertOne(new SystemSetting { SettingKey = "CUSTOMLOGOMODEL_ImagesToTestAfterTrainingFolderPath", SettingValue = @"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\inputs\images_for_prediction", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            if (!GetAll().Any(t => t.SettingKey == "CUSTOMLOGOMODEL_ExportedFromService_ImagesToEvaluateFolderPath"))
                InsertOne(new SystemSetting { SettingKey = "CUSTOMLOGOMODEL_ExportedFromService_ImagesToEvaluateFolderPath", SettingValue = @"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\inputs\images_for_prediction", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            if (!GetAll().Any(t => t.SettingKey == "Inellipse_Ad_Pointer_BaseURL"))
                InsertOne(new SystemSetting { SettingKey = "Inellipse_Ad_Pointer_BaseURL", SettingValue = "https://development.ad-pointer.com/mgmt-api", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            if (!GetAll().Any(t => t.SettingKey == "Inellipse_Ad_Pointer_Endpoint"))
                InsertOne(new SystemSetting { SettingKey = "Inellipse_Ad_Pointer_Endpoint", SettingValue = "/v1/adevent/create", ModifiedBy = "SystemSettingRepository", ModifiedOn = DateTime.UtcNow });

            first_time_executed = true;
        }
    }
}
