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
        public bool GetValue(string settingKey)
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
                int chunks = int.TryParse(GetSettingValueByKey("MaxChunksToProcessAtOnce"), out chunks) ? chunks : 5;
                return chunks;
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


        public string CUSTOMLOGOMODEL_ModelFilePath
        {
            get
            {
                return GetSettingValueByKey("CUSTOMLOGOMODEL_ModelFilePath");
                //"MLModelFilePath": "C:\\Users\\igor.zavojchevski\\Desktop\\Master\\ML\\assets\\Training\\outputs\\imageClassifier.zip"
            }
        }
        
        #endregion
    }
}
