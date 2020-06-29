using ML.Domain.DataModels;
using ML.Domain.DataModels.CustomLogoTrainingModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Interfaces
{
    public interface IAdvertisementScoringService
    {
        void Score();
        ImagePrediction PredictImage(InMemoryImageData image);
        void SaveImageScoringInfo(InMemoryImageData image, ImagePrediction prediction, Guid GroupGuid, bool isCustom = false);
    }
}
