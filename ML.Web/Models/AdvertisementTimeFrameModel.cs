using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ML.Web.Models
{
    public class AdvertisementTimeFrameModel
    {
        public string Id { get; set; }
        public string ClassName { get; set; }
        public Guid GroupGuid { get; set; }
        public DateTime ImageDateTime { get; set; }
        public bool IsCustom { get; set; }
    }
}
