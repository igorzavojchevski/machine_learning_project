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
using System.Drawing;
using ML.Domain.Entities.Enums;

namespace ML.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageClassificationController : ControllerBase
    {
        #region Props
        private readonly ILogger<ImageClassificationController> _logger;
        private readonly ILabelScoringService _labelScoringService;
        private readonly ICommercialService _commercialService;
        private readonly ICommercialScoringService _commercialScoringService;
        private readonly ILabelClassService _labelClassService;
        private readonly ISystemSettingService _systemSettingService;
        private readonly IEvaluationStreamService _evaluationStreamService;
        #endregion

        #region ctor
        public ImageClassificationController(
            ILogger<ImageClassificationController> logger,
            ILabelScoringService labelScoringService,
            ICommercialService commercialService,
            ICommercialScoringService commercialScoringService,
            ILabelClassService labelClassService,
            ISystemSettingService systemSettingService,
            IEvaluationStreamService evaluationStreamService)
        {
            _logger = logger;
            _labelScoringService = labelScoringService;
            _commercialService = commercialService;
            _commercialScoringService = commercialScoringService;
            _labelClassService = labelClassService;
            _systemSettingService = systemSettingService;
            _evaluationStreamService = evaluationStreamService;
        }
        #endregion

        [Route("GetAllImages")]
        public IActionResult GetAllImages(int size, int page, bool? showTrained = null, string? search = null)
        {
            var lastTrainingVersion = _labelClassService.GetAll().Max(t => t.TrainingVersion);

            List<LabelClass> lastTrainingVersionLabels = _labelClassService
                .GetAll()
                .Where(t => t.TrainingVersion == lastTrainingVersion && !t.IsCleanedUp)
                .ToList();
            
            List<LabelClass> labelClasses = null;


            if (string.IsNullOrWhiteSpace(search))
            {
                labelClasses = lastTrainingVersionLabels
                .GroupBy(r => r.FirstVersionId)
                .Select(g => g.OrderByDescending(r => r.Version).First())
                .Select(t => new LabelClass { Id = t.Id, ClassName = t.ClassName, FirstVersionId = t.FirstVersionId, ModifiedOn = t.ModifiedOn })
                .ToList();
            }
            else
            {
                labelClasses = lastTrainingVersionLabels
                .Where(t => t.ClassName.ToLowerInvariant().Contains(search))
                .GroupBy(r => r.FirstVersionId)
                .Select(g => g.OrderByDescending(r => r.Version).First())
                .Select(t => new LabelClass { Id = t.Id, ClassName = t.ClassName, FirstVersionId = t.FirstVersionId, ModifiedOn = t.ModifiedOn })
                .ToList();
            }

            CommercialGroupModel groupList = new CommercialGroupModel();
            groupList.Count = labelClasses.Count;

            List<LabelClass> labelClassesFinal = labelClasses.OrderByDescending(t=>t.ModifiedOn).Skip(size * (page - 1)).Take(size).ToList();

            //Make bool param for isTrained
            foreach (var item in labelClassesFinal)
            {
                string id = item.Id.ToString();
                List<string> classNamesFromAllVersions = _labelClassService.GetAll().Where(t => t.FirstVersionId == item.FirstVersionId).Select(t => t.ClassName).ToList();
                List<Commercial> list = new List<Commercial>();

                if (!showTrained.HasValue)
                    list = _commercialService.GetAll().Where(t => t.ClassifiedBy == ClassifiedBy.ClassificationService && classNamesFromAllVersions.Contains(t.PredictedLabel)).ToList();
                else
                    list = _commercialService.GetAll().Where(t => t.ClassifiedBy == ClassifiedBy.ClassificationService && t.IsTrained == showTrained && classNamesFromAllVersions.Contains(t.PredictedLabel)).ToList();

                groupList.Group.Add(new CommercialImagesGroupModel() { ID = id, PredictedLabel = item.ClassName, ModifiedOn = item.ModifiedOn, Commercials = list.OrderByDescending(t => t.ImageDateTime).Select(t => t.ToCommercialModel()).ToList() });
            }

            return Ok(groupList);
        }

        [Route("GetAllAvailableLabels")]
        public IActionResult GetAllAvailableLabels()
        {
            var lastTrainingVersion = _labelClassService.GetAll().Max(t => t.TrainingVersion);

            var lastTrainingVersionLabels = _labelClassService
                .GetAll()
                .Where(t => t.TrainingVersion == lastTrainingVersion && !t.IsCleanedUp)
                .ToList();

            var labelClasses = lastTrainingVersionLabels
                .OrderBy(t => t.ClassName.ToLower().StartsWith("new_"))
                .GroupBy(r => r.FirstVersionId)
                .Select(g => g.OrderByDescending(r => r.Version).First())
                .Select(t => new LabelClassReturnModel { Id = t.Id.ToString(), ClassName = t.ClassName })
                .ToList();

            return Ok(labelClasses);
        }

        [Route("GetLabelTimeFrames")]
        public IActionResult GetLabelTimeFrames()
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

            List<CommercialTimeFrameModel> listOfCommercialsForTimeFrames = new List<CommercialTimeFrameModel>();

            foreach (var item in labelClasses)
            {
                List<string> classNamesFromAllVersions = _labelClassService.GetAll().Where(t => t.FirstVersionId == item.FirstVersionId).Select(t => t.ClassName).ToList();
                List<CommercialTimeFrameModel> listByClassName =
                    _commercialService
                    .GetAll()
                    .Where(t => classNamesFromAllVersions.Contains(t.PredictedLabel))
                    .OrderByDescending(t => t.ModifiedOn)
                    .Select(t => new CommercialTimeFrameModel
                    {
                        Id = t.Id.ToString(),
                        ClassName = item.ClassName,
                        GroupGuid = t.GroupGuid,
                        ImageDateTime = t.ImageDateTime,
                        ClassifiedBy = t.ClassifiedBy,
                        EvaluationStreamId = t.EvaluationStreamId
                    })
                    .ToList();

                listByClassName
                    .ForEach(t =>
                    t.EvaluationStreamName = _evaluationStreamService.GetAll().Where(es => es.Id == t.EvaluationStreamId).FirstOrDefault()?.Name
                    );

                listOfCommercialsForTimeFrames.AddRange(listByClassName);
            }

            List<LabelTimeFramesReturnModel>
                    groupList =
                    listOfCommercialsForTimeFrames
                    .GroupBy(t => t.ImageDateTime.Date)
                    .Select(g =>
                    new LabelTimeFramesReturnModel
                    {
                        DateTimeKey = g.Key,
                        LabelTimeFrameGroups =
                            g.OrderByDescending(t => t.ImageDateTime).GroupBy(t => new { t.ClassName, t.GroupGuid })
                            .Select(gsg =>
                            new LabelTimeFrameGroup
                            {
                                ClassName = gsg.Key.ClassName,
                                GroupGuid = gsg.Key.GroupGuid,
                                StartDate = gsg.Min(t => t.ImageDateTime),
                                EndDate = gsg.Max(t => t.ImageDateTime),
                                ClassifiedBy = gsg.Select(t => t.ClassifiedBy.ToString()).First(),
                                EvaluationStreamName = gsg.Select(t => t.EvaluationStreamName).First()
                            }).ToList()
                    })
                    .ToList();

            groupList = groupList.OrderByDescending(o => o.DateTimeKey).ToList();
            return Ok(groupList);
        }

        [Route("GetEvaluationStreams")]
        public IActionResult GetEvaluationStreams()
        {
            List<EvaluationStreamModel> evaluationStreams = _evaluationStreamService.GetAll().ToList().Select(t => t.ToEvaluationStreamModel()).ToList();
            return Ok(evaluationStreams);
        }

        [Route("GetSystemSettings")]
        public IActionResult GetSystemSettings()
        {
            List<SystemSettingReturnModel> systemSettings = _systemSettingService.GetAll().ToList().Select(t => t.ToSystemSettingReturnModel()).ToList();
            return Ok(systemSettings);
        }

        [Route("GetAllNewItemImages")]
        public IActionResult GetAllNewItemImages(int size, int page, bool? showTrained = null, string? search = null)
        {
            var lastTrainingVersion = _labelClassService.GetAll().Max(t => t.TrainingVersion);

            List<LabelClass> lastTrainingVersionLabels = _labelClassService
                .GetAll()
                .Where(t => t.TrainingVersion == lastTrainingVersion && !t.IsCleanedUp)
                .ToList();

            List<LabelClass> labelClasses = null;

            if (string.IsNullOrWhiteSpace(search))
            {
                labelClasses = lastTrainingVersionLabels
                .Where(t => t.ClassName.ToLowerInvariant().StartsWith("new_item"))
                .GroupBy(r => r.FirstVersionId)
                .Select(g => g.OrderByDescending(r => r.Version).First())
                .Select(t => new LabelClass { Id = t.Id, ClassName = t.ClassName, FirstVersionId = t.FirstVersionId, ModifiedOn = t.ModifiedOn })
                .ToList();
            }
            else
            {
                labelClasses = lastTrainingVersionLabels
                .Where(t => t.ClassName.ToLowerInvariant().StartsWith("new_item") && t.ClassName.ToLowerInvariant().Contains(search))
                .GroupBy(r => r.FirstVersionId)
                .Select(g => g.OrderByDescending(r => r.Version).First())
                .Select(t => new LabelClass { Id = t.Id, ClassName = t.ClassName, FirstVersionId = t.FirstVersionId, ModifiedOn = t.ModifiedOn })
                .ToList();
            }

            CommercialGroupModel groupList = new CommercialGroupModel();
            groupList.Count = labelClasses.Count;

            List<LabelClass> labelClassesFinal = labelClasses.OrderByDescending(t => t.ModifiedOn).Skip(size * (page - 1)).Take(size).ToList();

            //Make bool param for isTrained
            foreach (var item in labelClassesFinal)
            {
                string id = item.Id.ToString();
                List<string> classNamesFromAllVersions = _labelClassService.GetAll().Where(t => t.FirstVersionId == item.FirstVersionId).Select(t => t.ClassName).ToList();
                List<Commercial> list = new List<Commercial>();

                if (!showTrained.HasValue)
                    list = _commercialService.GetAll().Where(t => t.ClassifiedBy == ClassifiedBy.ClassificationService && classNamesFromAllVersions.Contains(t.PredictedLabel)).ToList();
                else
                    list = _commercialService.GetAll().Where(t => t.ClassifiedBy == ClassifiedBy.ClassificationService && t.IsTrained == showTrained && classNamesFromAllVersions.Contains(t.PredictedLabel)).ToList();

                groupList.Group.Add(new CommercialImagesGroupModel() { ID = id, PredictedLabel = item.ClassName, ModifiedOn = item.ModifiedOn, Commercials = list.OrderByDescending(t => t.ImageDateTime).Select(t => t.ToCommercialModel()).ToList() });
            }

            return Ok(groupList);
        }


        [HttpPost]
        [Route("CreateEvaluationStream")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult CreateEvaluationStream(EvaluationStreamItem evaluationStreamItem)
        {
            if (evaluationStreamItem == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(evaluationStreamItem.Name) || string.IsNullOrWhiteSpace(evaluationStreamItem.Stream) || string.IsNullOrWhiteSpace(evaluationStreamItem.Code)) return BadRequest();

            bool create = true;
            EvaluationStream evaluationStream = null;
            if (string.IsNullOrWhiteSpace(evaluationStreamItem.Id))
            {
                bool exists = _evaluationStreamService.GetAll().Any(t => t.Code == evaluationStreamItem.Code);
                if (exists) return BadRequest();
                evaluationStream = new EvaluationStream();
            }
            else
            {
                bool IsParsable = ObjectId.TryParse(evaluationStreamItem.Id, out ObjectId objectID);
                if (!IsParsable) return BadRequest();
                evaluationStream = _evaluationStreamService.GetAll().Where(t => t.Id == objectID).FirstOrDefault();
                if (evaluationStream == null) return NotFound();
                create = false;
            }

            evaluationStream.Name = evaluationStreamItem.Name;
            evaluationStream.Stream = evaluationStreamItem.Stream;
            evaluationStream.Code = evaluationStreamItem.Code;
            evaluationStream.IsActive = evaluationStreamItem.IsActive;
            evaluationStream.ModifiedBy = "CreateLabelClassName";
            evaluationStream.ModifiedOn = DateTime.UtcNow;

            if (create) _evaluationStreamService.InsertOne(evaluationStream);
            else _evaluationStreamService.Update(evaluationStream);

            return Ok();
        }

        [HttpPost]
        [Route("EditEvaluationStream")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult EditEvaluationStream(EvaluationStreamItem evaluationStreamItem)
        {
            if (evaluationStreamItem == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(evaluationStreamItem.Name) || string.IsNullOrWhiteSpace(evaluationStreamItem.Stream) || string.IsNullOrWhiteSpace(evaluationStreamItem.Code)) return BadRequest();

            ObjectId.TryParse(evaluationStreamItem.Id, out ObjectId evaluationStreamID);
            EvaluationStream evaluationStream = _evaluationStreamService.GetAll().Where(t => t.Id == evaluationStreamID).FirstOrDefault();
            if (evaluationStream == null) return BadRequest();

            evaluationStream.Name = evaluationStreamItem.Name;
            evaluationStream.Stream = evaluationStreamItem.Stream;
            evaluationStream.Code = evaluationStreamItem.Code;
            evaluationStream.IsActive = evaluationStreamItem.IsActive;
            evaluationStream.ModifiedBy = "CreateLabelClassName";
            evaluationStream.ModifiedOn = DateTime.UtcNow;

            _evaluationStreamService.Update(evaluationStream);

            return Ok();
        }



        [HttpPost]
        [Route("CreateSystemSetting")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult CreateSystemSetting(SystemSettingItem systemSettingItem)
        {
            if (systemSettingItem == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(systemSettingItem.SettingKey) || string.IsNullOrWhiteSpace(systemSettingItem.SettingKey)) return BadRequest();

            bool create = true;
            SystemSetting systemSetting = null;
            if (string.IsNullOrWhiteSpace(systemSettingItem.Id))
            {
                bool exists = _systemSettingService.GetAll().Any(t => t.SettingKey == systemSettingItem.SettingKey);
                if (exists) return BadRequest();
                systemSetting = new SystemSetting();
            }
            else
            {
                bool IsParsable = ObjectId.TryParse(systemSettingItem.Id, out ObjectId objectID);
                if (!IsParsable) return BadRequest();
                systemSetting = _systemSettingService.GetAll().Where(t => t.Id == objectID).FirstOrDefault();
                if (systemSetting == null) return NotFound();
                create = false;
            }
            systemSetting.SettingKey = systemSettingItem.SettingKey;
            systemSetting.SettingValue = systemSettingItem.SettingValue;
            systemSetting.ModifiedBy = "CreateSystemSetting";
            systemSetting.ModifiedOn = DateTime.UtcNow;

            if (create) _systemSettingService.InsertOne(systemSetting);
            else _systemSettingService.Update(systemSetting);

            return Ok();
        }

        [HttpPost]
        [Route("EditSystemSetting")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult EditSystemSetting(SystemSettingItem systemSettingItem)
        {
            if (systemSettingItem == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(systemSettingItem.SettingKey) || string.IsNullOrWhiteSpace(systemSettingItem.SettingValue)) return BadRequest();

            ObjectId.TryParse(systemSettingItem.Id, out ObjectId systemSettingID);
            SystemSetting systemSetting = _systemSettingService.GetAll().Where(t => t.Id == systemSettingID).FirstOrDefault();
            if (systemSetting == null) return BadRequest();

            systemSetting.SettingKey = systemSettingItem.SettingKey;
            systemSetting.SettingValue = systemSettingItem.SettingValue;
            systemSetting.ModifiedBy = "CreateSystemSetting";
            systemSetting.ModifiedOn = DateTime.UtcNow;

            _systemSettingService.Update(systemSetting);

            return Ok();
        }

        [HttpPost]
        [Route("CreateLabelClassName")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult CreateLabelClassName(LabelItem labelItem)
        {
            if (labelItem == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(labelItem.Name)) return BadRequest();

            Guid newGuid = Guid.NewGuid();

            LabelClass newLabelClass = new LabelClass()
            {
                ClassName = string.Format("{0}_{1}", labelItem.Name, newGuid),
                CategoryType = "Default", //make this enum in future
                ImagesGroupGuid = newGuid,
                DirectoryPath = Path.Combine(_systemSettingService.CUSTOMLOGOMODEL_TrainedImagesFolderPath, labelItem.Name + "_" + newGuid.ToString()),
                TrainingVersion = _labelClassService.GetAll().Any() ? _labelClassService.GetAll().Max(t => t.TrainingVersion) : 0,
                Version = 1,
                IsChanged = false,
                IsCustom = true,
                ModifiedBy = "CreateLabelClassName - by admin",
                ModifiedOn = DateTime.UtcNow
            };

            _labelClassService.InsertOne(newLabelClass);

            newLabelClass.FirstVersionId = newLabelClass.Id;
            newLabelClass.ModifiedOn = DateTime.UtcNow;
            _labelClassService.Update(newLabelClass);

            Directory.CreateDirectory(newLabelClass.DirectoryPath);

            return Ok();
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

            if (moveImagesModel.ImagesIds == null || moveImagesModel.ImagesIds.Count == 0 || moveImagesModel.ImagesIds.Any(t => t == null)) return BadRequest();

            ObjectId.TryParse(moveImagesModel.NewClassNameId, out ObjectId labelClassID);

            LabelClass newLabel = _labelClassService.GetAll().Where(t => t.Id == labelClassID).FirstOrDefault();
            if (newLabel == null) return NotFound();

            List<ObjectId> ImagesToMove = moveImagesModel.ImagesIds.Select(t => ObjectId.Parse(t)).ToList();

            List<Commercial> allAdsFromList = _commercialService.GetAll().Where(t => ImagesToMove.Contains(t.Id)).ToList();
            if (allAdsFromList == null || allAdsFromList.Count == 0) return NotFound();

            string newDirPath = Path.Combine(_systemSettingService.CUSTOMLOGOMODEL_TrainedImagesFolderPath, newLabel.ClassName);

            allAdsFromList.ForEach(t =>
            {
                string newFilePath = Path.Combine(newDirPath, t.ImageId);

                Directory.Move(t.ImageFilePath, newFilePath);

                //t.ImageId = string.Format("{0}_{1}", t.PredictedLabel, t.ImageId);
                t.PredictedLabel = newLabel.ClassName;
                t.ImageDirPath = newDirPath;
                t.ImageFilePath = newFilePath;
                t.ModifiedBy = "MoveImages";
                t.ModifiedOn = DateTime.UtcNow;

                _commercialService.Update(t);
            });

            return Ok("Success");
        }

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

            InMemoryImageData imageInputData = new InMemoryImageData(imageData, null, null, null, null, DateTime.UtcNow);

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
        [Route("PredictCustomImage")]
        public async Task<IActionResult> PredictCustomImage(IFormFile imageFile)
        {
            if (imageFile.Length == 0) return BadRequest();

            //Image to stream.
            var imageMemoryStream = new MemoryStream();
            await imageFile.CopyToAsync(imageMemoryStream);

            // Check that image is valid.
            byte[] imageData = imageMemoryStream.ToArray();
            if (!imageData.IsValidImage()) return StatusCode(StatusCodes.Status415UnsupportedMediaType);

            _logger.LogTrace("PredictCustomImage - Start processing image...");

            // Measure execution time.
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            InMemoryImageData imageInputData = new InMemoryImageData(imageData, null, imageFile.FileName, null, null, DateTime.UtcNow);
            // Predict code for provided image.
            ImagePrediction imagePrediction = _commercialScoringService.PredictImage(imageInputData);
            ImagePredictionReturnModel returnModel = new ImagePredictionReturnModel() { MaxScore = imagePrediction.Score.Max(), PredictedLabel = imagePrediction.PredictedLabel };

            // Stop measuring time.
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            _logger.LogTrace($"PredictCustomImage - Image processed in {elapsedMs} miliseconds");

            return Ok(returnModel);
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Route("SaveCustomImage")]
        public IActionResult SaveCustomImage([FromForm]CustomImageDetails imageDetails)
        {
            _logger.LogInformation("ClassifyCustomImage - Start");
            if (imageDetails == null || imageDetails.ImageFile == null || imageDetails.ImageFile.Length == 0) return BadRequest();

            //Image to stream.
            var imageMemoryStream = new MemoryStream();
            imageDetails.ImageFile.CopyToAsync(imageMemoryStream);

            // Check that image is valid.
            byte[] imageData = imageMemoryStream.ToArray();
            if (!imageData.IsValidImage()) return StatusCode(StatusCodes.Status415UnsupportedMediaType);

            MemoryStream ms = new MemoryStream(imageData);
            Image i = Image.FromStream(ms);

            ObjectId.TryParse(imageDetails.LabelID, out ObjectId labelID);
            LabelClass labelClass = _labelClassService.GetAll().Where(t => t.Id == labelID).FirstOrDefault();
            if (labelClass == null) return NotFound();

            string destOutputPath = Path.Combine(_systemSettingService.CUSTOMLOGOMODEL_TrainedImagesFolderPath, labelClass.ClassName);
            Directory.CreateDirectory(destOutputPath);

            string imageFilePath = Path.Combine(destOutputPath, imageDetails.ImageFile.FileName);
            i.Save(imageFilePath);

            InMemoryImageData imageInputData =
                new InMemoryImageData(imageData, labelClass.ClassName, imageDetails.ImageFile.FileName, imageFilePath, destOutputPath, DateTime.UtcNow);

            ImagePrediction prediction = new ImagePrediction() { PredictedLabel = labelClass.ClassName, Score = new float[] { 1f } }; //100% score due to custom eval.

            _commercialScoringService.SaveImageScoringInfo(imageInputData, prediction, Guid.NewGuid(), ClassifiedBy.Custom);

            _logger.LogInformation("ClassifyCustomImage - Finished");

            return Ok();
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
