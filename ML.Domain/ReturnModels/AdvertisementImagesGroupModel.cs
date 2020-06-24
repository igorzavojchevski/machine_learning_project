using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.ReturnModels
{
    public class AdvertisementImagesGroupModel
    {
        public string PredictedLabel { get; set; }
        public List<AdvertisementModel> Advertisements { get; set; }
    }
}
