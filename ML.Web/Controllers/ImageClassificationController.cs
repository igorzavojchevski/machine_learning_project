using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Domain.DataModels.TFLabelScoringModel;
using ML.Utils.Extensions;
using ML.Utils.Extensions.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ML.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageClassificationController : ControllerBase
    {
        #region Props
        public IConfiguration Configuration { get; }
        private readonly PredictionEnginePool<ImageInputData, ImageLabelPredictions> _predictionEnginePool;
        private readonly ILogger<ImageClassificationController> _logger;
        private readonly string _labelsFilePath;
        private readonly ISystemSettingService _systemSettingService;
        #endregion

        #region ctor
        public ImageClassificationController(PredictionEnginePool<ImageInputData, ImageLabelPredictions> predictionEnginePool,
                                             IConfiguration configuration,
                                             ILogger<ImageClassificationController> logger,
                                             ISystemSettingService systemSettingService)
        {
            _logger = logger;
            Configuration = configuration;
            _systemSettingService = systemSettingService;
            _labelsFilePath = BaseExtensions.GetPath(_systemSettingService.TF_LabelsFilePath);

            // Get the ML Model Engine injected, for scoring.
            _predictionEnginePool = predictionEnginePool;
        }
        #endregion


        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Route("classifyimage")]
        public async Task<IActionResult> ClassifyImage(IFormFile imageFile)
        {
            if (imageFile.Length == 0) return BadRequest();

            //Image to stream.
            var imageMemoryStream = new MemoryStream();
            await imageFile.CopyToAsync(imageMemoryStream);

            // Check that image is valid.
            byte[] imageData = imageMemoryStream.ToArray();
            if (!imageData.IsValidImage()) return StatusCode(StatusCodes.Status415UnsupportedMediaType);

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

            return Ok(imageLabelsPrediction);
        }



        #region Private Methods
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
            imageBestLabelPrediction.TopProbabilities = GetAllLabels(labels, probabilities).OrderBy(t => t.Value).TakeLast(5).ToDictionary(pair => pair.Key, pair => pair.Value);

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
        #endregion

        #region Test Methods
        ////Test Method
        //// GET api/ImageClassification
        //[HttpGet]
        //public ActionResult<IEnumerable<string>> Get()
        //{
        //    return new string[] { "ACK Heart beat 1", "ACK Heart beat 2" };
        //}
        #endregion
    }
}
