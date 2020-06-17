using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Core.TensorFlowInception
{
    public struct TensorFlowModelSettings
    {
        // Input tensor name.
        public const string inputTensorName = "input";

        // Output tensor name.
        public const string outputTensorName = "softmax0";
        public const string outputTensorName1 = "softmax1";
        public const string outputTensorName2 = "softmax2";

    }
}
