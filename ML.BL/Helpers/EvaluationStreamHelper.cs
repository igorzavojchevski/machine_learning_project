using ML.Domain.Entities.Mongo;
using ML.Domain.ReturnModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Helpers
{
    public static class EvaluationStreamHelper
    {
        public static EvaluationStreamModel ToEvaluationStreamModel(this EvaluationStream entity)
        {
            if (entity == null) return new EvaluationStreamModel();

            return new EvaluationStreamModel()
            {
                Id = entity.Id.ToString(),
                Name = entity.Name,
                Stream = entity.Stream,
                Code = entity.Code,
                IsActive = entity.IsActive
            };
        }
    }
}
