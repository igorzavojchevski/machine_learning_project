using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.ReturnModels
{
    public class CommercialImagesGroupModel
    {
        public string ID { get; set; }
        public string PredictedLabel { get; set; }
        public List<CommercialModel> Commercials { get; set; }
    }
}
