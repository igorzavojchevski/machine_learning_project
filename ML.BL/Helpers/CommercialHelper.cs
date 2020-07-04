using ML.Domain.Entities.Mongo;
using ML.Domain.ReturnModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Helpers
{
    public static class CommercialHelper
    {
        public static CommercialModel ToCommercialModel(this Commercial entity)
        {
            if (entity == null) return new CommercialModel();

            return new CommercialModel()
            {
                Id = entity.Id.ToString(),
                PredictedLabel = entity.PredictedLabel,
                ImageId = entity.ImageId,
                ImageFilePath = entity.ImageFilePath,
                ImageDirPath = entity.ImageDirPath,
                OriginalImageFilePath = entity.OriginalImageFilePath,
                OriginalImageDirPath = entity.OriginalImageDirPath,
                GroupGuid = entity.GroupGuid,
                MaxProbability = entity.MaxProbability,
                ImageDateTime = entity.ImageDateTime,
                IsTrained = entity.IsTrained,
                TrainedForClassName = entity.TrainedForClassName,

                IsMerged = entity.IsMerged,
                MergeParentDirPath = entity.MergeParentDirPath,
                PredictionExecutionTime = entity.PredictionExecutionTime,
                TopProbabilities = entity.TopProbabilities
            };
        }
    }
}
