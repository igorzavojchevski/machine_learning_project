using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.DataModels
{
    public class ImagePredictedLabelWithProbability
    {
        public string ImageId { get; set; }

        public string PredictedLabel { get; set; }
        public float MaxProbability { get; set; }

        public long PredictionExecutionTime { get; set; }

        //public string AllProbabilities { get; set; }
        public Dictionary<string, float> AllProbabilities { get; set; }
    }
}
