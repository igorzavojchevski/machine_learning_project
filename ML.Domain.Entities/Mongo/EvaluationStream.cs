using ML.Domain.Entities.Mongo.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.Entities.Mongo
{
    public class EvaluationStream : MongoBaseEntity
    {
        public string Name { get; set; }
        public string Stream { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
    }
}
