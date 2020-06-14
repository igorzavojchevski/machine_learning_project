using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using ML.Domain.DataModels;
using ML.Utils.Extensions;
using ML.Utils.Extensions.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ML.BL.Concrete
{
    //TODO - Rework this
    public class LabelScoringService
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<LabelScoringService> _logger;
        private readonly string _labelsFilePath;
        private readonly PredictionEnginePool<ImageInputData, ImageLabelPredictions> _predictionEnginePool;

        public LabelScoringService(
            PredictionEnginePool<ImageInputData, ImageLabelPredictions> predictionEnginePool,
            IConfiguration configuration, ILogger<LabelScoringService> logger)
        {
            _logger = logger;
            Configuration = configuration;
            _labelsFilePath = BaseExtensions.GetPath(Configuration["MLModel:LabelsFilePath"], Configuration.GetValue<bool>("MLModel:IsAbsolute"));
            _predictionEnginePool = predictionEnginePool;
        }

        public void Score()
        {
            FileInfo[] Images = GetListOfImages();

            for (int i = 0; i < Images.Length; i++)
            {
                ImagePredictedLabelWithProbability prediction = DoLabelScoring(Images[i]);

            }
        }

        private ImagePredictedLabelWithProbability DoLabelScoring(FileInfo fileInfo)
        {
            if (fileInfo.Length == 0)
            {
                _logger.LogDebug("fileInfo.Length == 0");
                return new ImagePredictedLabelWithProbability();
            }

            MemoryStream imageMemoryStream = new MemoryStream();
            using (FileStream fileStream = File.OpenRead(fileInfo.FullName))
            {
                imageMemoryStream = new MemoryStream();
                imageMemoryStream.SetLength(fileStream.Length);
                fileStream.Read(imageMemoryStream.GetBuffer(), 0, (int)fileStream.Length);
            }

            // Check that image is valid.
            byte[] imageData = imageMemoryStream.ToArray();
            if (!imageData.IsValidImage()) 
            { 
                _logger.LogDebug("Image type not supported"); 
                return new ImagePredictedLabelWithProbability(); 
            };

            //Memory stream to Image (System.Drawing).
            Image image = Image.FromStream(imageMemoryStream);

            // Convert to Bitmap.
            Bitmap bitmapImage = (Bitmap)image;

            _logger.LogInformation("Start processing image...");


            // Measure execution time.
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            // Set the specific image data into the ImageInputData type used in the DataView.
            ImageInputData imageInputData = new ImageInputData { Image = bitmapImage };

            // Predict code for provided image.
            ImageLabelPredictions imageLabelPredictions = _predictionEnginePool.Predict(imageInputData);

            // Stop measuring time.
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            _logger.LogInformation($"Image processed in {elapsedMs} miliseconds");



            // Predict the image's label (The one with highest probability).
            ImagePredictedLabelWithProbability imageLabelsPrediction = FindLabelsWithProbability(imageLabelPredictions, imageInputData);

            return imageLabelsPrediction;
        }

        private ImagePredictedLabelWithProbability FindLabelsWithProbability(ImageLabelPredictions imageLabelPredictions, ImageInputData imageInputData)
        {
            // Read TF model's labels (.txt with labels) to classify the image across those labels.
            var labels = ReadLabels(_labelsFilePath);

            float[] probabilities = imageLabelPredictions.PredictedLabels;

            // Set a single label as predicted or even none if probabilities are lower than 70%.
            var imageBestLabelPrediction = new ImagePredictedLabelWithProbability()
            {
                ImageId = imageInputData.GetHashCode().ToString(), //This ID is not really needed, it could come from the application itself, etc.
            };

            (imageBestLabelPrediction.PredictedLabel, imageBestLabelPrediction.MaxProbability) = GetBestLabel(labels, probabilities);

            //test take 5
            imageBestLabelPrediction.AllProbabilities = GetAllLabels(labels, probabilities).OrderBy(t => t.Value).TakeLast(5).ToDictionary(pair => pair.Key, pair => pair.Value);

            return imageBestLabelPrediction;
        }
        private (string, float) GetBestLabel(string[] labels, float[] probs)
        {
            var max = probs.Max();
            var index = probs.AsSpan().IndexOf(max);

            if (max > 0.7)
                return (labels[index], max);

            return ("None", max);
        }

        //Used for testing only - not needed
        private Dictionary<string, float> GetAllLabels(string[] labels, float[] probs)
        {
            Dictionary<string, float> d = new Dictionary<string, float>();

            for (int i = 0; i < probs.Length; i++)
            {
                if (labels.Length == i) break;
                var test = labels[probs.AsSpan().IndexOf(probs[i])];
                if (d.ContainsKey(test)) continue;
                d.Add(test, probs[i]);
            }
            return d;
        }

        //Read these in memory
        private string[] ReadLabels(string labelsLocation)
        {
            return System.IO.File.ReadAllLines(labelsLocation);
        }

        private FileInfo[] GetListOfImages()
        {
            //TODO - Rework this part for dynamically getting and get only supported file types
            DirectoryInfo di = new DirectoryInfo(@"C:\Users\igor.zavojchevski\Desktop\Master\TestMaterial\Frames\Ski");
            FileInfo[] Images = di.GetFiles();
            return Images;
        }
    }
}
