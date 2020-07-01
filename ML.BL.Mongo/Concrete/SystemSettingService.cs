using ML.BL.Mongo.Concrete.Base;
using ML.BL.Mongo.Interfaces;
using ML.Domain.Entities.Mongo;
using ML.Infrastructure.Interfaces;
using System.Linq;

namespace ML.BL.Mongo.Concrete
{
    public class SystemSettingService : MongoBaseService<SystemSetting, ISystemSettingRepository>, ISystemSettingService
    {
        private readonly ISystemSettingRepository _systemSettingRepository;
        public override ISystemSettingRepository Repository
        {
            get
            {
                return _systemSettingRepository;
            }
        }

        public SystemSettingService(ISystemSettingRepository systemSettingRepository)
        {
            _systemSettingRepository = systemSettingRepository;
        }

        #region Get Settings Logic
        public bool GetSettingValue(string settingKey)
        {

            var setting = _systemSettingRepository.GetAll().FirstOrDefault(x => x.SettingKey == settingKey);
            if (setting == null) return false;

            return (bool.TryParse(setting.SettingValue, out bool val)) ? val : false;
        }

        public string GetSettingValueByKey(string settingKey)
        {
            var setting = _systemSettingRepository.GetAll().FirstOrDefault(x => x.SettingKey == settingKey);
            if (setting == null) return string.Empty;

            return setting.SettingValue;
        }
        #endregion


        #region Settings
        public int MaxChunksToProcessAtOnce
        {
            get
            {
                int chunks = int.TryParse(GetSettingValueByKey("MaxChunksToProcessAtOnce"), out chunks) ? chunks : 3;
                return chunks;
            }
        }

        public bool IsTrainingServiceStarted
        {
            get
            {
                return GetSettingValue("IsTrainingServiceStarted");
            }
        }

        public float ClassGroupThreshold
        {
            get
            {
                float threshold = float.TryParse(GetSettingValueByKey("ClassGroupThreshold"), out threshold) ? threshold : 0.7f;
                return threshold;
            }
        }

        public string FFMPEG_ExecutablePath
        {
            get
            {
                return GetSettingValueByKey("FFMPEG_ExecutablePath");
                //@"C:\ffmpeg\bin\ffmpeg.exe"
            }
        }

        public string Archive_Path_For_Trained_Images
        {
            get
            {
                return GetSettingValueByKey("Archive_Path_For_Trained_Images");
            }
        }

        public string Archive_LastStartDate
        {
            get
            {
                return GetSettingValueByKey("Archive_LastStartDate");
            }
        }

        public int Archive_NextStartPeriod_Minutes
        {
            get
            {
                int minutes = int.TryParse(GetSettingValueByKey("Archive_NextStartPeriod_Minutes"), out minutes) ? minutes : 4320;
                return minutes;
            }
        }




        public float TF_LabelScoring_MaxProbabilityThreshold
        {
            get
            {
                float threshold = float.TryParse(GetSettingValueByKey("TF_LabelScoring_MaxProbabilityThreshold"), out threshold) ? threshold : 0.7f;
                return threshold;
            }
        }

        public int TF_LabelScoring_MaxLabels
        {
            get
            {
                int maxlabels = int.TryParse(GetSettingValueByKey("TF_LabelScoring_MaxLabels"), out maxlabels) ? maxlabels : 5;
                return maxlabels;
            }
        }

        public string TF_LabelsFilePath
        {
            get
            {
                return GetSettingValueByKey("TF_LabelsFilePath");
                //"LabelsFilePath": "C:\\Users\\igor.zavojchevski\\Desktop\\Master\\ML\\assets\\MLModels\\TensorFlowModel\\imagenet_comp_graph_label_strings.txt"

            }
        }


        public string TF_ModelFilePath
        {
            get
            {
                return GetSettingValueByKey("TF_TensorFlowModelFilePath");
                //"TensorFlowModelFilePath": "C:\\Users\\igor.zavojchevski\\Desktop\\Master\\ML\\assets\\MLModels\\TensorFlowModel\\tensorflow_inception_graph.pb",
            }
        }


        //Two different settings to overwrite one, due to training process which can be paralelly done with score evaluation
        //After finishing, file should be transfered from one to another location
        public string CUSTOMLOGOMODEL_ModelFilePath
        {
            get
            {
                return GetSettingValueByKey("CUSTOMLOGOMODEL_ModelFilePath");
                //"MLModelFilePath": "C:\\Users\\igor.zavojchevski\\Desktop\\Master\\ML\\assets\\Training\\outputs\\imageClassifier.zip"
            }
        }

        public string CUSTOMLOGOMODEL_OutputFilePath
        {
            get
            {
                return GetSettingValueByKey("CUSTOMLOGOMODEL_OutputFilePath");
                //@"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\outputs\imageClassifier.zip"
            }
        }

        public string CUSTOMLOGOMODEL_TrainedImagesFolderPath
        {
            get
            {
                return GetSettingValueByKey("CUSTOMLOGOMODEL_TrainedImagesFolderPath");
                //@"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\inputs\images_to_train";
            }
        }

        public string CUSTOMLOGOMODEL_ImagesToTestAfterTrainingFolderPath
        {
            get
            {
                return GetSettingValueByKey("CUSTOMLOGOMODEL_ImagesToTestAfterTrainingFolderPath");
                //@"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\inputs\images_for_prediction";
            }
        }

        public string CUSTOMLOGOMODEL_ExportedFromService_ImagesToEvaluateFolderPath
        {
            get
            {
                return GetSettingValueByKey("CUSTOMLOGOMODEL_ExportedFromService_ImagesToEvaluateFolderPath");
                //@"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\inputs\images_for_prediction";
            }
        }

        
        #endregion
    }
}
