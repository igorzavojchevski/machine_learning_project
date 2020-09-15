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
        bool IsTrainingServiceStarted { get; }
        float ClassGroupThreshold { get; }
        string FFMPEG_ExecutablePath { get; }
        string Archive_Path_For_Trained_Images { get; }
        string Archive_LastStartDate { get; }
        int Archive_NextStartPeriod_Minutes { get; }
        int ExportService_ExportPeriod_Seconds { get; }


        float TF_LabelScoring_MaxProbabilityThreshold { get; }
        int TF_LabelScoring_MaxLabels { get; }
        string TF_LabelsFilePath { get; }
        string TF_ModelFilePath { get; }


        string CUSTOMLOGOMODEL_ModelFilePath { get; }
        string CUSTOMLOGOMODEL_OutputFilePath { get; }
        string CUSTOMLOGOMODEL_TrainedImagesFolderPath { get; }
        string CUSTOMLOGOMODEL_ImagesToTestAfterTrainingFolderPath { get; }
        string CUSTOMLOGOMODEL_ExportedFromService_ImagesToEvaluateFolderPath { get; }

        string Inellipse_Ad_Pointer_BaseURL { get; }
        string Inellipse_Ad_Pointer_Endpoint { get; }

    }
}
