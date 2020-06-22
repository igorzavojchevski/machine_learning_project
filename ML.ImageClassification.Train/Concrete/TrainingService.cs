using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Domain.DataModels.CustomLogoTrainingModel;
using ML.Domain.Entities.Mongo;
using ML.ImageClassification.Train.Interfaces;
using ML.Utils.Extensions.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ML.ImageClassification.Train.Concrete
{
    public class TrainingService : ITrainingService
    {
        private readonly ILogger<TrainingService> _logger;
        private readonly MLContext _mlContext;
        private readonly ISystemSettingService _systemSettingService;

        public TrainingService(ILogger<TrainingService> logger, ISystemSettingService systemSettingService)
        {
            _logger = logger;
            _mlContext = new MLContext(seed: 1);
            _systemSettingService = systemSettingService;
        }

        public void DoBeforeTrainingStart()
        {
            //Set IsTrainingStarted configuration to TRUE
            string key = "IsTrainingServiceStarted";
            SystemSetting setting = _systemSettingService.GetAll().Where(t => t.SettingKey == key).FirstOrDefault();

            if (setting == null)
            {
                _systemSettingService.InsertOne(new SystemSetting() { SettingKey = key, SettingValue = "true", ModifiedOn = DateTime.UtcNow, ModifiedBy = "TrainingService" });
                return;
            }

            setting.SettingValue = "true";
            _systemSettingService.Update(setting);

            //Check if training file is used by another process
            bool isFileLocked = true;
            while (isFileLocked)
            {
                bool isLocked = BaseExtensions.IsFileLocked(_systemSettingService.CUSTOMLOGOMODEL_OutputFilePath);
                if (!isLocked) isFileLocked = false;
                else
                {
                    _logger.LogInformation("TrainService - DoBeforeTrainingStart - File isLocked - TRUE");
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            }
        }

        public void DoAfterTrainingFinished()
        {
            //Set IsTrainingStarted configuration to FALSE
            string key = "IsTrainingServiceStarted";
            SystemSetting setting = _systemSettingService.GetAll().Where(t => t.SettingKey == key).FirstOrDefault();
            setting.SettingValue = "false";
            _systemSettingService.Update(setting);
        }

        public void Train()
        {
            try
            {
                _logger.LogInformation("TrainService - Train started");

                string modelOutputFilePath = _systemSettingService.CUSTOMLOGOMODEL_OutputFilePath;
                string imagesToReTrainFolderPath = _systemSettingService.CUSTOMLOGOMODEL_TrainedImagesFolderPath;
                string imagesToTestAfterTrainingFolderPath = _systemSettingService.CUSTOMLOGOMODEL_ImagesToTestAfterTrainingFolderPath;


                //
                _logger.LogInformation("TrainService - Train - 2.Load the initial full image-set started");
                // 2. Load the initial full image-set into an IDataView and shuffle so it will be better balanced
                IEnumerable<ImageData> images = BaseExtensions.LoadImagesFromDirectory(folder: imagesToReTrainFolderPath, useFolderNameAsLabel: true).Select(x => new ImageData(x.imagePath, x.label));
                IDataView fullImagesDataset = _mlContext.Data.LoadFromEnumerable(images);
                IDataView shuffledFullImageFilePathsDataset = _mlContext.Data.ShuffleRows(fullImagesDataset);

                _logger.LogInformation("TrainService - Train - 2.Load the initial full image-set finished");
                //


                //
                _logger.LogInformation("TrainService - Train - 3.Load Images with in-memory type started");
                // 3. Load Images with in-memory type within the IDataView and Transform Labels to Keys (Categorical)
                IDataView shuffledFullImagesDataset = _mlContext.Transforms.Conversion.
                        MapValueToKey(outputColumnName: "LabelAsKey", inputColumnName: "Label", keyOrdinality: Microsoft.ML.Transforms.ValueToKeyMappingEstimator.KeyOrdinality.ByValue)
                    .Append(_mlContext.Transforms.LoadRawImageBytes(
                                                    outputColumnName: "Image",
                                                    imageFolder: imagesToReTrainFolderPath,
                                                    inputColumnName: "ImagePath"))
                    .Fit(shuffledFullImageFilePathsDataset)
                    .Transform(shuffledFullImageFilePathsDataset);

                _logger.LogInformation("TrainService - Train - 3.Load Images with in-memory type finished");
                //


                //
                _logger.LogInformation("TrainService - Train - 4.Split the data 80:20 into train and test sets started");
                // 4. Split the data 80:20 into train and test sets, train and evaluate.
                DataOperationsCatalog.TrainTestData trainTestData = _mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.2);
                IDataView trainDataView = trainTestData.TrainSet;
                IDataView testDataView = trainTestData.TestSet;

                _logger.LogInformation("TrainService - Train - 4.Split the data 80:20 into train and test sets finished");
                //


                //
                _logger.LogInformation("TrainService - Train - 5. Define the model's training pipeline started");
                // 5. Define the model's training pipeline using DNN default values
                EstimatorChain<Microsoft.ML.Transforms.KeyToValueMappingTransformer> pipeline =
                    _mlContext.MulticlassClassification.Trainers
                        .ImageClassification(featureColumnName: "Image",
                                                labelColumnName: "LabelAsKey",
                                                validationSet: testDataView)
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabel",
                                                                          inputColumnName: "PredictedLabel"));

                _logger.LogInformation("TrainService - Train - 5. Define the model's training pipeline finished");
                //


                //
                _logger.LogInformation("TrainService - Train - 6.Train/create the ML model started");
                // 6. Train/create the ML model
                _logger.LogInformation("TrainService - *** Training the image classification model with DNN Transfer Learning on top of the selected pre-trained model/architecture ***");

                // Measuring training time
                Stopwatch watch = Stopwatch.StartNew();

                //Train
                ITransformer trainedModel = pipeline.Fit(trainDataView);

                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;
                _logger.LogInformation($"Training with transfer learning took: {elapsedMs / 1000} seconds");

                _logger.LogInformation("TrainService - Train - 6.Train/create the ML model finished");
                //


                //
                _logger.LogInformation("TrainService - Train - 7. Get the quality metrics started");
                // 7. Get the quality metrics (accuracy, etc.)
                EvaluateModel(_mlContext, testDataView, trainedModel);

                _logger.LogInformation("TrainService - Train - 7. Get the quality metrics finished");
                //


                //
                _logger.LogInformation("TrainService - Train - 8.Save the model to assets/outputs");
                // 8. Save the model to assets/outputs (You get ML.NET .zip model file and TensorFlow .pb model file)
                _mlContext.Model.Save(trainedModel, trainDataView.Schema, modelOutputFilePath);
                _logger.LogInformation($"Model saved to: {modelOutputFilePath}");
                //


                // 9. Try a single prediction simulating an end-user app
                TrySinglePrediction(imagesToTestAfterTrainingFolderPath, _mlContext, trainedModel);

                _logger.LogInformation("Finished Training");
            }
            catch (Exception ex)
            {
                _logger.LogError("TrainService - exception", ex);
                _logger.LogError(ex, "TrainService - exception");
            }
        }

        private void EvaluateModel(MLContext mlContext, IDataView testDataset, ITransformer trainedModel)
        {
            _logger.LogInformation("Making predictions in bulk for evaluating model's quality...");

            // Measuring time
            Stopwatch watch = Stopwatch.StartNew();

            IDataView predictionsDataView = trainedModel.Transform(testDataset);

            MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName: "LabelAsKey", predictedLabelColumnName: "PredictedLabel");
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
            PredictionEngine<InMemoryImageData, ImagePrediction> predictionEngine =
                mlContext.Model.CreatePredictionEngine<InMemoryImageData, ImagePrediction>(trainedModel);

            IEnumerable<InMemoryImageData> testImages = BaseExtensions.LoadInMemoryImagesFromDirectory(
                imagesFolderPathForPredictions, false);

            InMemoryImageData imageToPredict = testImages.FirstOrDefault();
            if (imageToPredict == null)
            {
                _logger.LogInformation("TrySinglePrediction - imageToPredict == null - no image to predict");
                return;
            }

            ImagePrediction prediction = predictionEngine.Predict(imageToPredict);

            _logger.LogInformation(
                $"Image Filepath : [{imageToPredict.ImageFilePath}], " +
                $"Image Filename : [{imageToPredict.ImageFileName}], " +
                $"Scores : [{string.Join(",", prediction.Score)}], " +
                $"Predicted Label : {prediction.PredictedLabel}");
        }
    }
}
