using ML.Domain.Entities.Mongo;
using ML.Domain.ReturnModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Helpers
{
    public static class AdvertisementHelper
    {
        public static AdvertisementModel ToAdvertisementModel(this Advertisement entity)
        {
            if (entity == null) return new AdvertisementModel();

            return new AdvertisementModel()
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

                IsMerged = entity.IsMerged,
                MergeParentDirPath = entity.MergeParentDirPath,
                PredictionExecutionTime = entity.PredictionExecutionTime,
                TopProbabilities = entity.TopProbabilities
            };
        }
    }
}
