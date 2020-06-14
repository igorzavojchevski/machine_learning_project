using ML.Domain.Entities.Mongo.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.Entities.Mongo
{
    public class Advertisement : MongoBaseEntity
    {
        public string ImageId { get; set; }
        public string PredictedLabel { get; set; }
        public float MaxProbability { get; set; }
        public long PredictionExecutionTime { get; set; }
        public Dictionary<string, float> TopProbabilities { get; set; }
        public Guid GroupGuid { get; set; }
    }
}
