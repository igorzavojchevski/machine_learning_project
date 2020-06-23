using ML.Domain.Entities.Enums;
using ML.Domain.Entities.Mongo.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.Entities.Mongo
{
    public class EvaluationGroup : MongoBaseEntity
    {
        public string EvaluationGroupDirPath { get; set; }
        public Guid EvaluationGroupGuid { get; set; }
        public TrainingStatus Status { get; set; }
    }
}
