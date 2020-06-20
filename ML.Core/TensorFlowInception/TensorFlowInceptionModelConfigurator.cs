using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using ML.Domain.DataModels.TFLabelScoringModel;

namespace ML.Core.TensorFlowInception
{
    public class TensorFlowInceptionModelConfigurator
    {
        private readonly MLContext _mlContext;

        public ITransformer Model { get; }

        public TensorFlowInceptionModelConfigurator(string tensorFlowModelFilePath)
        {
            _mlContext = new MLContext();
            // Model creation and pipeline definition for images needs to run just once, so calling it from the constructor.
            Model = SetupMLNetModel(tensorFlowModelFilePath);
        }

        private ITransformer SetupMLNetModel(string tensorFlowModelFilePath)
        {
            EstimatorChain<TensorFlowTransformer> pipeline =
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
            return _mlContext.Data.LoadFromEnumerable<ImageInputData>(list);
        }
    }
}
