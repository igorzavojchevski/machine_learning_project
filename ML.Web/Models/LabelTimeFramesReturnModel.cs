using ML.Domain.Entities.Enums;
using ML.Domain.ReturnModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ML.Web.Models
{
    public class LabelTimeFramesReturnModel
    {
        public DateTime DateTimeKey { get; set; }
        public List<LabelTimeFrameGroup> LabelTimeFrameGroups { get; set; }
    }

    public class LabelTimeFrameGroup
    {
        public string ClassName { get; set; }
        public Guid GroupGuid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
                                
        public string ClassifiedBy { get; set; }
        public string EvaluationStreamName { get; set; }
    }
}
