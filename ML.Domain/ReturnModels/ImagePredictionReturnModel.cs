using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.ReturnModels
{
    public class ImagePredictionReturnModel
    {
        public float MaxScore { get; set; }

        public string PredictedLabel { get; set; }
    }
}
