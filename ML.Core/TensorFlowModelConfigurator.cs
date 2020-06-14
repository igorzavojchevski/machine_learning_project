using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using System.Collections.Generic;
using System.Linq;
using ML.Domain.DataModels;

namespace ML.Core
{
    public class TensorFlowModelConfigurator
    {
        private readonly MLContext _mlContext;

        public ITransformer Model { get; }

        public TensorFlowModelConfigurator(string tensorFlowModelFilePath)
        {
            _mlContext = new MLContext();

            // Model creation and pipeline definition for images needs to run just once, so calling it from the constructor.
            Model = SetupMlnetModel(tensorFlowModelFilePath);
        }

        public struct ImageSettings
        {
            public const int imageHeight = 227;
            public const int imageWidth = 227;
            public const float mean = 117;         //offsetImage
            public const bool channelsLast = true; //interleavePixelColors
        }

        // For checking tensor names, you can open the TF model .pb file with tools like Netron: https://github.com/lutzroeder/netron
        public struct TensorFlowModelSettings
        {
            // Input tensor name.
            public const string inputTensorName = "input";

            // Output tensor name.
            public const string outputTensorName = "softmax0";
            public const string outputTensorName1 = "softmax1";
            public const string outputTensorName2 = "softmax2";

        }

        private ITransformer SetupMlnetModel(string tensorFlowModelFilePath)
        {
            var pipeline =
                _mlContext
                .Transforms
                .ResizeImages(outputColumnName: TensorFlowModelSettings.inputTensorName, imageWidth: ImageSettings.imageWidth, imageHeight: ImageSettings.imageHeight, inputColumnName: nameof(ImageInputData.Image), ImageResizingEstimator.ResizingKind.Fill)
                .Append(_mlContext.Transforms.ExtractPixels(outputColumnName: TensorFlowModelSettings.inputTensorName, interleavePixelColors: ImageSettings.channelsLast, offsetImage: ImageSettings.mean))
                .Append(_mlContext.Model.LoadTensorFlowModel(tensorFlowModelFilePath)
                .ScoreTensorFlowModel(outputColumnNames: new[] { TensorFlowModelSettings.outputTensorName, TensorFlowModelSettings.outputTensorName1, TensorFlowModelSettings.outputTensorName2 },
                                    inputColumnNames: new[] { TensorFlowModelSettings.inputTensorName }, addBatchDimensionInput: true));


            //Pipeline
            ITransformer mlModel = pipeline.Fit(CreateEmptyDataView());

            return mlModel;
        }
        private IDataView CreateEmptyDataView()
        {
            // Create empty DataView ot Images. We just need the schema to call fit().
            var list = new List<ImageInputData>();
            list.Add(new ImageInputData());
            //Test: Might not need to create the Bitmap.. = null; ?
            IEnumerable<ImageInputData> enumerableData = list;
            var dv = _mlContext.Data.LoadFromEnumerable<ImageInputData>(list);
            return dv;
        }
    }
}
