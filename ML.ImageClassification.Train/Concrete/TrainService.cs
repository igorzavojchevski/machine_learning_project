using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using ML.Domain.DataModels.TrainingModels;
using ML.ImageClassification.Train.Interfaces;
using ML.Utils.Extensions.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Microsoft.ML.Transforms.ValueToKeyMappingEstimator;

namespace ML.ImageClassification.Train.Concrete
{
    public class TrainService : ITrainService
    {
        private readonly ILogger<TrainService> _logger;
        private readonly MLContext _mlContext;

        public TrainService(ILogger<TrainService> logger)
        {
            _logger = logger;
            _mlContext = new MLContext(seed: 1);
        }

        public void Train()
        {
            //string outputMlNetModelFilePath = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");
            //string imagesFolderPathForPredictions = Path.Combine(assetsPath, "inputs", "images-for-predictions", "FlowersForPredictions");
            string outputMlNetModelFilePath = @"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\outputs\imageClassifier.zip"; //Path.Combine(assetsPath, "outputs", "imageClassifier.zip");
            string fullImagesetFolderPath = @"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\inputs\images_to_train";
            string imagesFolderPathForPredictions = @"C:\Users\igor.zavojchevski\Desktop\Master\ML\assets\Training\inputs\images_for_prediction";
            // 2. Load the initial full image-set into an IDataView and shuffle so it'll be better balanced
            IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: fullImagesetFolderPath, useFolderNameAsLabel: true);
            IDataView fullImagesDataset = _mlContext.Data.LoadFromEnumerable(images);
            IDataView shuffledFullImageFilePathsDataset = _mlContext.Data.ShuffleRows(fullImagesDataset);

            // 3. Load Images with in-memory type within the IDataView and Transform Labels to Keys (Categorical)
            IDataView shuffledFullImagesDataset = _mlContext.Transforms.Conversion.
                    MapValueToKey(outputColumnName: "LabelAsKey", inputColumnName: "Label", keyOrdinality: KeyOrdinality.ByValue)
                .Append(_mlContext.Transforms.LoadRawImageBytes(
                                                outputColumnName: "Image",
                                                imageFolder: fullImagesetFolderPath,
                                                inputColumnName: "ImagePath"))
                .Fit(shuffledFullImageFilePathsDataset)
                .Transform(shuffledFullImageFilePathsDataset);

            // 4. Split the data 80:20 into train and test sets, train and evaluate.
            var trainTestData = _mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.2);
            IDataView trainDataView = trainTestData.TrainSet;
            IDataView testDataView = trainTestData.TestSet;

            // 5. Define the model's training pipeline using DNN default values
            //
            var pipeline = _mlContext.MulticlassClassification.Trainers
                    .ImageClassification(featureColumnName: "Image",
                                            labelColumnName: "LabelAsKey",
                                            validationSet: testDataView)
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabel",
                                                                      inputColumnName: "PredictedLabel"));

            // 6. Train/create the ML model
            _logger.LogInformation("*** Training the image classification model with DNN Transfer Learning on top of the selected pre-trained model/architecture ***");

            // Measuring training time
            var watch = Stopwatch.StartNew();

            //Train
            ITransformer trainedModel = pipeline.Fit(trainDataView);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            _logger.LogInformation($"Training with transfer learning took: {elapsedMs / 1000} seconds");

            // 7. Get the quality metrics (accuracy, etc.)
            EvaluateModel(_mlContext, testDataView, trainedModel);

            // 8. Save the model to assets/outputs (You get ML.NET .zip model file and TensorFlow .pb model file)
            _mlContext.Model.Save(trainedModel, trainDataView.Schema, outputMlNetModelFilePath);
            _logger.LogInformation($"Model saved to: {outputMlNetModelFilePath}");

            // 9. Try a single prediction simulating an end-user app
            TrySinglePrediction(imagesFolderPathForPredictions, _mlContext, trainedModel);

            _logger.LogInformation("Finished Training");
        }

        public IEnumerable<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameAsLabel = true)
            => BaseExtensions.LoadImagesFromDirectory(folder, useFolderNameAsLabel)
            .Select(x => new ImageData(x.imagePath, x.label));

        private void EvaluateModel(MLContext mlContext, IDataView testDataset, ITransformer trainedModel)
        {
            _logger.LogInformation("Making predictions in bulk for evaluating model's quality...");

            // Measuring time
            var watch = Stopwatch.StartNew();

            var predictionsDataView = trainedModel.Transform(testDataset);

            var metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName: "LabelAsKey", predictedLabelColumnName: "PredictedLabel");
            PrintMultiClassClassificationMetrics("TensorFlow DNN Transfer Learning", metrics);

            watch.Stop();
            var elapsed2Ms = watch.ElapsedMilliseconds;

            _logger.LogInformation($"Predicting and Evaluation took: {elapsed2Ms / 1000} seconds");
        }

        public void PrintMultiClassClassificationMetrics(string name, MulticlassClassificationMetrics metrics)
        {
            _logger.LogInformation($"************************************************************");
            _logger.LogInformation($"*    Metrics for {name} multi-class classification model   ");
            _logger.LogInformation($"*-----------------------------------------------------------");
            _logger.LogInformation($"    AccuracyMacro = {metrics.MacroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            _logger.LogInformation($"    AccuracyMicro = {metrics.MicroAccuracy:0.####}, a value between 0 and 1, the closer to 1, the better");
            _logger.LogInformation($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better");

            int i = 0;
            foreach (var classLogLoss in metrics.PerClassLogLoss)
            {
                i++;
                _logger.LogInformation($"    LogLoss for class {i} = {classLogLoss:0.####}, the closer to 0, the better");
            }
            _logger.LogInformation($"************************************************************");
        }

        private void TrySinglePrediction(string imagesFolderPathForPredictions, MLContext mlContext, ITransformer trainedModel)
        {
            // Create prediction function to try one prediction
            var predictionEngine = mlContext.Model
                .CreatePredictionEngine<InMemoryImageData, ImagePrediction>(trainedModel);

            var testImages = BaseExtensions.LoadInMemoryImagesFromDirectory(
                imagesFolderPathForPredictions, false);

            var imageToPredict = testImages.First();

            var prediction = predictionEngine.Predict(imageToPredict);

            _logger.LogInformation(
                $"Image Filename : [{imageToPredict.ImageFileName}], " +
                $"Scores : [{string.Join(",", prediction.Score)}], " +
                $"Predicted Label : {prediction.PredictedLabel}");
        }
    }
}
