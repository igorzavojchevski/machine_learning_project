using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.DataModels.Models
{
    public class ProbabilityGroup
    {
        public string PredictedLabel { get; set; }
        public float Probability { get; set; }
    }
}
