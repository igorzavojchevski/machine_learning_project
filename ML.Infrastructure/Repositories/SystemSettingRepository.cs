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


            
            first_time_executed = true;
        }
    }
}
