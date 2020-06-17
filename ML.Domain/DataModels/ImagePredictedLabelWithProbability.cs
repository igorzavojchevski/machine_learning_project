using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.DataModels
{
    public class ImagePredictedLabelWithProbability
    {
        public string ImageId { get; set; }
        public string ImageFileSystemId { get; set; }

        public string PredictedLabel { get; set; }
        public float MaxProbability { get; set; }

        public long PredictionExecutionTime { get; set; }

        public Dictionary<string, float> TopProbabilities { get; set; }
    }
}
