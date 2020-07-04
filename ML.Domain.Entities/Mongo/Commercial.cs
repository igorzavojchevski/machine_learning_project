using ML.Domain.Entities.Enums;
using ML.Domain.Entities.Mongo.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.Entities.Mongo
{
    public class Commercial : MongoBaseEntity
    {
        public string ImageId { get; set; }
        public string ImageFilePath { get; set; }
        public string ImageDirPath { get; set; }

        public string OriginalImageFilePath { get; set; }
        public string OriginalImageDirPath { get; set; }

        public string PredictedLabel { get; set; }
        
        [BsonRepresentation(BsonType.Double, AllowTruncation = true)]
        public float MaxProbability { get; set; }
        public long PredictionExecutionTime { get; set; }
        public Guid GroupGuid { get; set; }
        public DateTime ImageDateTime { get; set; }

        public ObjectId? EvaluationStreamId { get; set; }

        public ClassifiedBy ClassifiedBy { get; set; }

        public bool IsTrained { get; set; }
        public string TrainedForClassName { get; set; }


        // not needed at this moment
        public bool IsMerged { get; set; }
        public string MergeParentDirPath { get; set; }
        public Dictionary<string, float> TopProbabilities { get; set; }
    }
}
