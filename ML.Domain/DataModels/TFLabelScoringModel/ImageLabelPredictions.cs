using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.DataModels.TFLabelScoringModel
{
    public class ImageLabelPredictions
    {
        //TODO: Change to fixed output column name for TensorFlow model
        [ColumnName("softmax0")]
        public float[] PredictedLabels;
    }
}
