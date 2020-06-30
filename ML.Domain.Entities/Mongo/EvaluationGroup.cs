using ML.Domain.Entities.Enums;
using ML.Domain.Entities.Mongo.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.Entities.Mongo
{
    public class EvaluationGroup : MongoBaseEntity
    {
        public string DirPath { get; set; }
        public string ZipDirPath { get; set; }
        public Guid EvaluationGroupGuid { get; set; }
        public Guid ParentGroupGuid { get; set; }
        public TrainingStatus Status { get; set; }
    }
}
