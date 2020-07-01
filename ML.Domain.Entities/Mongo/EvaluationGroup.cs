using ML.Domain.Entities.Enums;
using ML.Domain.Entities.Mongo.Base;
using MongoDB.Bson;
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
        public ObjectId EvaluationStreamId { get; set; }
        public string EvaluationStreamName { get; set; }
    }
}
