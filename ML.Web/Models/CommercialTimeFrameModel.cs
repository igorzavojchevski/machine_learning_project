using ML.Domain.Entities.Enums;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ML.Web.Models
{
    public class CommercialTimeFrameModel
    {
        public string Id { get; set; }
        public string ClassName { get; set; }
        public Guid GroupGuid { get; set; }
        public DateTime ImageDateTime { get; set; }
        public ClassifiedBy ClassifiedBy { get; set; }
        public ObjectId? EvaluationStreamId { get; set; }
        public string EvaluationStreamName { get; set; }
    }
}
