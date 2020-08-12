using ML.Domain.Entities.Mongo.Base;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.Entities.Mongo
{
    public class CommercialBlock : MongoBaseEntity
    {
        public string Label { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> CommercialIDs { get; set; }
        public string VideoURL { get; set; }
        public ObjectId? EvaluationStreamID { get; set; }
        public string EvaluationStreamName { get; set; }
    }
}
