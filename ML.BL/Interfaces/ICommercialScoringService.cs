using ML.Domain.DataModels;
using ML.Domain.DataModels.CustomLogoTrainingModel;
using ML.Domain.Entities.Enums;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Interfaces
{
    public interface ICommercialScoringService
    {
        void Score();
        ImagePrediction PredictImage(InMemoryImageData image);
        void SaveImageScoringInfo(InMemoryImageData image, ImagePrediction prediction, Guid GroupGuid, ClassifiedBy classifiedBy, ObjectId? evaluationStreamID = null);
    }
}
