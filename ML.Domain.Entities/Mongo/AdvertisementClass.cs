using ML.Domain.Entities.Mongo.Base;
using System;

namespace ML.Domain.Entities.Mongo
{
    public class AdvertisementClass : MongoBaseEntity
    {
        public string ClassName { get; set; } //Reklama1, Reklama2, Skopsko
        public string CategoryType { get; set; } //Default, Banking, Beverage, Restaurants...
        public string DirectoryPath { get; set; }
        public Guid ImagesGroupGuid { get; set; }
    }
}
