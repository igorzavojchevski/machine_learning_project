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
using MongoDB.Bson;
using System;
using ML.Web.Models;

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
        private readonly ILabelClassService _labelClassService;
        private readonly ISystemSettingService _systemSettingService;
        #endregion

        #region ctor
        public ImageClassificationController(
            ILogger<ImageClassificationController> logger,
            ILabelScoringService labelScoringService,
            IAdvertisementService advertisementService,
            IAdvertisementScoringService advertisementScoringService,
            ILabelClassService labelClassService,
            ISystemSettingService systemSettingService)
        {
            _logger = logger;
            _labelScoringService = labelScoringService;
            _advertisementService = advertisementService;
            _advertisementScoringService = advertisementScoringService;
            _labelClassService = labelClassService;
            _systemSettingService = systemSettingService;
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
        [Route("ClassifyImageCustom")]
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
            var lastTrainingVersion = _labelClassService.GetAll().Max(t => t.TrainingVersion);

            var lastTrainingVersionLabels = _labelClassService
                .GetAll()
                .Where(t => t.TrainingVersion == lastTrainingVersion)
                .ToList();

            var labelClasses = lastTrainingVersionLabels
                .GroupBy(r => r.FirstVersionId)
                .Select(g => g.OrderByDescending(r => r.Version).First())
                .Select(t => new LabelClass { Id = t.Id, ClassName = t.ClassName, FirstVersionId = t.FirstVersionId })
                .ToList();

            List<AdvertisementImagesGroupModel> groupList = new List<AdvertisementImagesGroupModel>();

            foreach (var item in labelClasses)
            {
                string id = item.Id.ToString();
                List<string> classNamesFromAllVersions = _labelClassService.GetAll().Where(t => t.FirstVersionId == item.FirstVersionId).Select(t => t.ClassName).ToList();
                List<Advertisement> list = _advertisementService.GetAll().Where(t => classNamesFromAllVersions.Contains(t.PredictedLabel)).ToList();
                groupList.Add(new AdvertisementImagesGroupModel() { ID = id, PredictedLabel = item.ClassName, Advertisements = list.Select(t => t.ToAdvertisementModel()).ToList() });
            }

            return Ok(groupList);
        }


        [Route("GetAllAvailableLabels")]
        public IActionResult GetAllAvailableLabels()
        {
            var lastTrainingVersion = _labelClassService.GetAll().Max(t => t.TrainingVersion);

            var lastTrainingVersionLabels = _labelClassService
                .GetAll()
                .Where(t => t.TrainingVersion == lastTrainingVersion)
                .ToList();

            var labelClasses = lastTrainingVersionLabels
                .GroupBy(r => r.FirstVersionId)
                .Select(g => g.OrderByDescending(r => r.Version).First())
                .Select(t => new LabelClassReturnModel { Id = t.Id.ToString(), ClassName = t.ClassName })
                .ToList();

            return Ok(labelClasses);
        }


        [HttpPost]
        [Route("EditLabelClassName")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult EditLabelClassName(LabelItem labelItem)
        {
            if (labelItem == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(labelItem.Name)) return BadRequest();

            ObjectId.TryParse(labelItem.Id, out ObjectId labelClassID);

            LabelClass existingLabelClass = _labelClassService.GetAll().Where(t => t.Id == labelClassID).FirstOrDefault();
            if (existingLabelClass == null) return NotFound();

            if (labelItem.Name == existingLabelClass.ClassName) return Ok();

            LabelClass newLabelClass = new LabelClass()
            {
                ClassName = labelItem.Name,
                CategoryType = "Default", //make this enum in future
                ImagesGroupGuid = existingLabelClass.ImagesGroupGuid,
                DirectoryPath = Path.Combine(Directory.GetParent(existingLabelClass.DirectoryPath).FullName, labelItem.Name),
                TrainingVersion = existingLabelClass.TrainingVersion,
                Version = existingLabelClass.Version + 1,
                FirstVersionId = existingLabelClass.FirstVersionId,
                IsChanged = false,
                ModifiedBy = "EditLabelClassName - by admin",
                ModifiedOn = DateTime.UtcNow
            };

            _labelClassService.InsertOne(newLabelClass);

            existingLabelClass.IsChanged = true;
            existingLabelClass.ModifiedOn = DateTime.UtcNow;
            _labelClassService.Update(existingLabelClass);

            Directory.Move(existingLabelClass.DirectoryPath, newLabelClass.DirectoryPath);                
            //Directory.CreateDirectory(newLabelClass.DirectoryPath);

            //IEnumerable<FileInfo> files = Directory.GetFiles(existingLabelClass.DirectoryPath).Select(f => new FileInfo(f));
            //foreach (var file in files)
            //{
            //    System.IO.File.Move(file.FullName, Path.Combine(newLabelClass.DirectoryPath, file.Name), true);
            //}

            return Ok();
        }



        [HttpPost]
        [Route("MoveImages")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult MoveImages(MoveImagesModel moveImagesModel)
        {
            if (moveImagesModel == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(moveImagesModel.NewClassNameId)) return BadRequest();
            
            if (moveImagesModel.ImagesIds == null || moveImagesModel.ImagesIds.Count == 0) return BadRequest();

            ObjectId.TryParse(moveImagesModel.NewClassNameId, out ObjectId labelClassID);

            LabelClass newLabel = _labelClassService.GetAll().Where(t => t.Id == labelClassID).FirstOrDefault();
            if (newLabel == null) return NotFound();

            List<ObjectId> ImagesToMove = moveImagesModel.ImagesIds.Select(t => ObjectId.Parse(t)).ToList();

            List<Advertisement> allAdsFromList = _advertisementService.GetAll().Where(t => ImagesToMove.Contains(t.Id)).ToList();
            if (allAdsFromList == null || allAdsFromList.Count == 0) return NotFound();

            string newDirPath = Path.Combine(_systemSettingService.CUSTOMLOGOMODEL_TrainedImagesFolderPath, newLabel.ClassName);
            
            allAdsFromList.ForEach(t =>
            {
                string newFilePath = Path.Combine(newDirPath, string.Format("{0}_{1}", t.PredictedLabel, t.ImageId));

                Directory.Move(t.ImageFilePath, newFilePath);

                t.ImageId = string.Format("{0}_{1}", t.PredictedLabel, t.ImageId);
                t.PredictedLabel = newLabel.ClassName;
                t.ImageDirPath = newDirPath;
                t.ImageFilePath = newFilePath;
                t.ModifiedBy = "MoveImages";
                t.ModifiedOn = DateTime.UtcNow;

                _advertisementService.Update(t);
            });

            return Ok("Success");
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
