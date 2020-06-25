using ML.Domain.Entities.Mongo.Base;
using MongoDB.Bson;
using System;

namespace ML.Domain.Entities.Mongo
{
    public class LabelClass : MongoBaseEntity
    {
        public string ClassName { get; set; } //Reklama1, Reklama2, Skopsko
        public string CategoryType { get; set; } //Default, Banking, Beverage, Restaurants...
        public string DirectoryPath { get; set; }
        public Guid ImagesGroupGuid { get; set; }
        public int Version { get; set; } //ClassVersion - class can be different after training (due to name change)
        public int TrainingVersion { get; set; } //Training version - after each training, this is incremented, does not depend on label name change
        public ObjectId FirstVersionId { get; set; }
        public bool IsChanged { get; set; }
    }
}
