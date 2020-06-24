using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.DataModels;
using ML.Domain.ReturnModels;
using ML.Utils.Extensions;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ML.BL.Helpers;
using ML.Domain.Entities.Mongo;
using System.Collections.Generic;
using ML.Domain.DataModels.CustomLogoTrainingModel;

namespace ML.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageClassificationController : ControllerBase
    {
        #region Props
        private readonly ILogger<ImageClassificationController> _logger;
        private readonly ILabelScoringService _labelScoringService;
        private readonly IAdvertisementService _advertisementService;
        private readonly IAdvertisementScoringService _advertisementScoringService;
        #endregion

        #region ctor
        public ImageClassificationController(
            ILogger<ImageClassificationController> logger,
            ILabelScoringService labelScoringService,
            IAdvertisementService advertisementService,
            IAdvertisementScoringService advertisementScoringService)
        {
            _logger = logger;
            _labelScoringService = labelScoringService;
            _advertisementService = advertisementService;
            _advertisementScoringService = advertisementScoringService;
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

            ////Memory stream to Image (System.Drawing).
            //Image image = Image.FromStream(imageMemoryStream);

            //// Convert to Bitmap.
            //Bitmap bitmapImage = (Bitmap)image;

            _logger.LogInformation("Start processing image...");

            // Measure execution time.
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            // Set the specific image data into the ImageInputData type used in the DataView.
            //ImageInputData imageInputData = new ImageInputData { Image = bitmapImage };

            InMemoryImageData imageInputData = new InMemoryImageData(imageData, null, null, null, null);

            // Predict code for provided image.
            ImagePredictedLabelWithProbability imageLabelPredictions = _labelScoringService.CheckImageForLabelScoring(imageInputData);

            // Stop measuring time.
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            _logger.LogInformation($"Image processed in {elapsedMs} miliseconds");


            //// Predict the image's label (The one with highest probability).
            //ImagePredictedLabelWithProbability imageLabelsPrediction = FindLabelsWithProbability(imageLabelPredictions, imageInputData);

            return Ok(imageLabelPredictions);
        }



        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Route("classifyimagecustom")]
        public async Task<IActionResult> ClassifyImageCustom(IFormFile imageFile)
        {
            if (imageFile.Length == 0) return BadRequest();

            //Image to stream.
            var imageMemoryStream = new MemoryStream();
            await imageFile.CopyToAsync(imageMemoryStream);

            // Check that image is valid.
            byte[] imageData = imageMemoryStream.ToArray();
            if (!imageData.IsValidImage()) return StatusCode(StatusCodes.Status415UnsupportedMediaType);

            _logger.LogInformation("Start processing image...");

            // Measure execution time.
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            InMemoryImageData imageInputData = new InMemoryImageData(imageData, null, imageFile.FileName, null, null);

            // Predict code for provided image.
            ImagePrediction imagePrediction = _advertisementScoringService.CheckImageAndDoLabelScoring(imageInputData);
            ImagePredictionReturnModel returnModel = new ImagePredictionReturnModel() { MaxScore = imagePrediction.Score.Max(), PredictedLabel = imagePrediction.PredictedLabel };

            // Stop measuring time.
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            _logger.LogInformation($"Image processed in {elapsedMs} miliseconds");

            return Ok(returnModel);
        }


        [Route("GetAllImages")]
        public IActionResult GetAllImages()
        {
            var labels = _advertisementService.GetAll().Select(t => t.PredictedLabel).Distinct().ToList();

            List<AdvertisementImagesGroupModel> groupList = new List<AdvertisementImagesGroupModel>();

            foreach(var item in labels)
            {
                var list = _advertisementService.GetAll().Where(t => t.PredictedLabel == item).ToList();
                groupList.Add(new AdvertisementImagesGroupModel() { PredictedLabel = item, Advertisements = list.Select(t => t.ToAdvertisementModel()).ToList() });
            }

            return Ok(groupList);

        }
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
