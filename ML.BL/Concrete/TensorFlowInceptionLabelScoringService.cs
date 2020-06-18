using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Utils.Extensions;
using ML.Utils.Extensions.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ML.BL.Concrete
{
    //TODO - Rework this
    public class TensorFlowInceptionLabelScoringService : ITensorFlowInceptionLabelScoringService
    {
        private static string[] labels = null;
        private static float? MaxProbabilityThreshold = null;
        public IConfiguration Configuration { get; }
        private readonly ILogger<TensorFlowInceptionLabelScoringService> _logger;
        private readonly PredictionEnginePool<ImageInputData, ImageLabelPredictions> _predictionEnginePool;

        public TensorFlowInceptionLabelScoringService(
            PredictionEnginePool<ImageInputData, ImageLabelPredictions> predictionEnginePool,
            IConfiguration configuration,
            ILogger<TensorFlowInceptionLabelScoringService> logger)
        {
            Configuration = configuration;
            _logger = logger;
            _predictionEnginePool = predictionEnginePool;
            MaxProbabilityThreshold = Configuration.GetValue<float?>("MLModel:MaxProbabilityThreshold"); // do this as system settings
        }

        public ImagePredictedLabelWithProbability DoLabelScoring(FileInfo fileInfo)
        {
            if (fileInfo.Length == 0)
            {
                _logger.LogDebug("fileInfo.Length == 0");
                return new ImagePredictedLabelWithProbability();
            }

            string imageName = fileInfo.Name;
            string imageFullPath = fileInfo.FullName;
            MemoryStream imageMemoryStream = new MemoryStream();
            using (FileStream fileStream = File.OpenRead(imageFullPath))
            {
                imageMemoryStream = new MemoryStream();
                imageMemoryStream.SetLength(fileStream.Length);
                fileStream.Read(imageMemoryStream.GetBuffer(), 0, (int)fileStream.Length);
            }

            // Check that image is valid.
            byte[] imageData = imageMemoryStream.ToArray();
            if (!imageData.IsValidImage())
            {
                _logger.LogDebug("DoLabelScoring - Image type not supported {0}", imageFullPath);
                return new ImagePredictedLabelWithProbability();
            };

            //Memory stream to Image (System.Drawing).
            Image image = Image.FromStream(imageMemoryStream);

            // Convert to Bitmap.
            Bitmap bitmapImage = (Bitmap)image;

            _logger.LogInformation("DoLabelScoring - Start processing image... {0}", imageName);

            // Measure execution time.
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            // Set the specific image data into the ImageInputData type used in the DataView.
            ImageInputData imageInputData = new ImageInputData { Image = bitmapImage };

            // Predict code for provided image.
            ImageLabelPredictions imageLabelPredictions = _predictionEnginePool.Predict(imageInputData);

            // Stop measuring time.
            watch.Stop();

            _logger.LogInformation("Image {0} processed in {1} miliseconds", imageName, watch.ElapsedMilliseconds);

            // Predict the image's label (The one with highest probability).
            ImagePredictedLabelWithProbability imageLabelsPrediction = FindLabelsWithProbability(imageLabelPredictions, imageInputData, imageName);

            return imageLabelsPrediction;
        }

        private ImagePredictedLabelWithProbability FindLabelsWithProbability(ImageLabelPredictions imageLabelPredictions, ImageInputData imageInputData, string imageName)
        {
            ImagePredictedLabelWithProbability imageBestLabelPrediction = new ImagePredictedLabelWithProbability();

            string _labelsFilePath = BaseExtensions.GetPath(Configuration["MLModel:LabelsFilePath"], Configuration.GetValue<bool>("MLModel:IsAbsolute"));
            if (string.IsNullOrWhiteSpace(_labelsFilePath)) return imageBestLabelPrediction;

            // Read TF model's labels (.txt with labels) to classify the image across those labels.
            labels = (labels == null || labels.Length == 0) ? ReadLabels(_labelsFilePath) : labels;

            float[] probabilities = imageLabelPredictions.PredictedLabels;

            // Set a single label as predicted or even none if probabilities are lower than MaxProbabilityThreshold (default 70%).
            imageBestLabelPrediction = new ImagePredictedLabelWithProbability()
            {
                ImageId = imageInputData.GetHashCode().ToString(), //This ID is not really needed, it could come from the application itself, etc.
                ImageFileSystemId = imageName
            };

            (imageBestLabelPrediction.PredictedLabel, imageBestLabelPrediction.MaxProbability) = GetBestLabel(labels, probabilities);

            imageBestLabelPrediction.TopProbabilities = GetTopLabels(labels, probabilities);

            return imageBestLabelPrediction;
        }

        private (string, float) GetBestLabel(string[] labels, float[] probs)
        {
            float max = probs.Max();

            if (max <= MaxProbabilityThreshold) return ("None", max);

            int index = probs.AsSpan().IndexOf(max);
            return (labels[index], max);
        }

        private Dictionary<string, float> GetTopLabels(string[] labels, float[] probs)
        {
            Dictionary<string, float> topLabels = new Dictionary<string, float>();

            IEnumerable<float> probabilities = probs.OrderByDescending(t => t).Take(Configuration.GetValue<int>("MLModel:MaxLabels"));

            for (int i = 0; i < probabilities.Count(); i++)
            {
                if (labels.Length == i) break;
                string test = labels[probs.AsSpan().IndexOf(probs[i])];
                if (topLabels.ContainsKey(test)) continue;
                topLabels.Add(test, probs[i]);
            }
            return topLabels;
        }

        private string[] ReadLabels(string labelsLocation)
        {
            return labels = File.ReadAllLines(labelsLocation);
        }
    }
}
