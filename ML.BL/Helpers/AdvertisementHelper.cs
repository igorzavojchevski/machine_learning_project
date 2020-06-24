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
                PredictedLabel = entity.PredictedLabel,
                ImageId = entity.ImageId,
                ImageFilePath = entity.ImageFilePath,
                ImageDirPath = entity.ImageDirPath,
                GroupGuid = entity.GroupGuid,
                MaxProbability = entity.MaxProbability,

                IsMerged = entity.IsMerged,
                MergeParentDirPath = entity.MergeParentDirPath,
                PredictionExecutionTime = entity.PredictionExecutionTime,
                TopProbabilities = entity.TopProbabilities
            };
        }
    }
}
