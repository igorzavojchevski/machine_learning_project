using ML.BL.Mongo.Interfaces.Base;
using ML.Domain.Entities.Mongo;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Mongo.Interfaces
{
    public interface ISystemSettingService : IMongoBaseService<SystemSetting>
    {
        int MaxChunksToProcessAtOnce { get; }
        
        float TF_LabelScoring_MaxProbabilityThreshold { get; }
        int TF_LabelScoring_MaxLabels { get; }
        string TF_LabelsFilePath { get; }
        string TF_ModelFilePath { get; }

        string CUSTOMLOGOMODEL_ModelFilePath { get; }
    }
}
