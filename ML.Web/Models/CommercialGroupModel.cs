using ML.Domain.ReturnModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ML.Web.Models
{
    public class CommercialGroupModel
    {
        public CommercialGroupModel()
        {
            Group = new List<CommercialImagesGroupModel>();
        }

        public List<CommercialImagesGroupModel> Group { get; set; }
        public long Count { get; set; }
    }
}
