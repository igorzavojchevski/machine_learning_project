using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ML.Domain.Entities.Mongo.Base
{
    public abstract class MongoBaseEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}
