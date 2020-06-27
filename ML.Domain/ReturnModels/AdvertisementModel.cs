using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.ReturnModels
{
    public class AdvertisementModel
    {
        public string Id { get; set; }
        public string ImageId { get; set; }
        public string ImageFilePath { get; set; }
        public string ImageDirPath { get; set; }
        public string OriginalImageFilePath { get; set; }
        public string OriginalImageDirPath { get; set; }
        public string PredictedLabel { get; set; }
        public float MaxProbability { get; set; }
        public long PredictionExecutionTime { get; set; }
        public Guid GroupGuid { get; set; }
        public bool IsMerged { get; set; }
        public string MergeParentDirPath { get; set; }
        public Dictionary<string, float> TopProbabilities { get; set; }
    }
}
