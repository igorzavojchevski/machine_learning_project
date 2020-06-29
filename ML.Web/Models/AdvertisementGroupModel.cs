using ML.Domain.ReturnModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ML.Web.Models
{
    public class AdvertisementGroupModel
    {
        public AdvertisementGroupModel()
        {
            Group = new List<AdvertisementImagesGroupModel>();
        }

        public List<AdvertisementImagesGroupModel> Group { get; set; }
        public long Count { get; set; }
    }
}
