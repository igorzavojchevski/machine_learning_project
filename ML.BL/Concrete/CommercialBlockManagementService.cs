using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.Entities.Mongo;
using ML.Domain.RequestModels;
using ML.Utils.Extensions;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ML.BL.Concrete
{
    public class CommercialBlockManagementService : ICommercialBlockManagementService
    {
        private readonly ILogger<CommercialBlockManagementService> _logger;
        private readonly ICommercialBlockService _commercialBlockService;
        private readonly ICommercialService _commercialService;
        private readonly IEvaluationStreamService _evaluationStreamService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISystemSettingService _systemSettingService;

        public CommercialBlockManagementService(
            ILogger<CommercialBlockManagementService> logger, 
            ICommercialBlockService commercialBlockService, 
            ICommercialService commercialService, 
            IEvaluationStreamService evaluationStreamService,
            IHttpClientFactory httpClientFactory,
            ISystemSettingService systemSettingService)
        {
            _logger = logger;
            _commercialBlockService = commercialBlockService;
            _commercialService = commercialService;
            _evaluationStreamService = evaluationStreamService;
            _httpClientFactory = httpClientFactory;
            _systemSettingService = systemSettingService;
        }

        public void MakeCommercialBlocks()
        {
            try
            {
                _logger.LogInformation("MakeCommercialBlocks - Start");

                DateTime? lastCommercialBlockDateChecked = 
                    _commercialBlockService.GetAll().Any() ? 
                    _commercialBlockService.GetAll().Max(t => t.EndDate) :
                    _commercialService.GetAll().Any() ? _commercialService.GetAll().Min(t => t.ImageDateTime) : (DateTime?)null;

                if (!lastCommercialBlockDateChecked.HasValue) return;

                lastCommercialBlockDateChecked =
                    _commercialBlockService.GetAll().Any() ?
                    lastCommercialBlockDateChecked + TimeSpan.FromSeconds(1) :
                    lastCommercialBlockDateChecked;

                _logger.LogInformation("MakeCommercialBlocks - lastCommercialBlockDateChecked=" + lastCommercialBlockDateChecked.ToString());

                List<Commercial> commercials = _commercialService.GetAll().Where(t => t.ImageDateTime >= lastCommercialBlockDateChecked && !t.PredictedLabel.ToLower().StartsWith("new_item")).ToList();
                if (commercials == null || !commercials.Any()) return;

                List<EvaluationStream> evaluationStreams = _evaluationStreamService.GetAll().ToList() ?? new List<EvaluationStream>(); 

                List<CommercialBlock> groupedCommercials = commercials
                    .OrderBy(t => t.ImageDateTime).GroupWhile((preceeding, next) => preceeding.PredictedLabel == next.PredictedLabel)
                    .Select(t =>
                    new CommercialBlock()
                    {
                        Label = t.Select(tt => tt.PredictedLabel).FirstOrDefault(),
                        CommercialIDs = t.Select(tt => tt.Id.ToString()).ToList(),
                        StartDate = t.Min(tt => tt.ImageDateTime),
                        EndDate = t.Max(tt => tt.ImageDateTime),
                        ModifiedOn = DateTime.UtcNow,
                        ModifiedBy = "CommercialBlockService",
                        EvaluationStreamID = t.Select(tt => tt.EvaluationStreamId).FirstOrDefault(),
                        EvaluationStreamName = evaluationStreams.Where(es => es.Id == t.Select(tt => tt.EvaluationStreamId).FirstOrDefault()).Select(t => t.Name).FirstOrDefault(),
                        IsSentToAdPointer = false
                    })
                    .ToList();

                _commercialBlockService.InsertMany(groupedCommercials);

                _logger.LogInformation("MakeCommercialBlocks - End");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MakeCommercialBlocks - Error");
            }
        }


        public void MakeCommercialVideosForBlocks()
        {

        }

        public void SendCommercialBlocks()
        {
            try
            {
                string baseUrlENV = Environment.GetEnvironmentVariable("Inellipse_Ad_Pointer_BaseURL");
                string endpointENV = Environment.GetEnvironmentVariable("Inellipse_Ad_Pointer_Endpoint");
                string baseurl = !string.IsNullOrWhiteSpace(baseUrlENV) ? baseUrlENV : _systemSettingService.Inellipse_Ad_Pointer_BaseURL;
                string endpoint = !string.IsNullOrWhiteSpace(endpointENV) ? endpointENV : _systemSettingService.Inellipse_Ad_Pointer_Endpoint;
                string requestURL = baseurl + endpoint;

                List<AdPointerCommercialBlockModel> commercialBlocksToSend = _commercialBlockService.GetAll().Where(t => !t.IsSentToAdPointer).ToList()
                    .Select(t => new AdPointerCommercialBlockModel()
                    {
                        AdId = t.Id.ToString(),
                        AdEventTime = t.StartDate, //or getutcdate
                        AdStartEventTime = t.StartDate,
                        AdEndEventTime = t.EndDate,
                        ChannelId = string.Format("{0}_{1}", t.EvaluationStreamID, t.EvaluationStreamName),
                        VideoUrl = t.VideoURL
                    }).ToList();

                var serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

                foreach (var commercialBlock in commercialBlocksToSend)
                {
                    using (var client = _httpClientFactory.CreateClient("httpclient"))
                    {
                        var body = JsonConvert.SerializeObject(commercialBlock, serializerSettings);
                        var requestBody = new StringContent(body, Encoding.UTF8, "application/json");
                        requestBody.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        _logger.LogInformation(string.Format("SendCommercialBlocks - Sending {0}", commercialBlock.AdId));

                        var response = client.PostAsync(requestURL, requestBody).Result;

                        _logger.LogInformation(string.Format("SendCommercialBlocks - Sent {0}", commercialBlock.AdId));

                        if (response.IsSuccessStatusCode)
                        {
                            string responseString = response.Content.ReadAsStringAsync().Result;

                            _logger.LogInformation(string.Format("SendCommercialBlocks - response {0} - {1}", commercialBlock.AdId, responseString));

                            ObjectId id = ObjectId.Parse(commercialBlock.AdId);
                            _commercialBlockService.Update(t => t.Id == id, t => t.IsSentToAdPointer, true);

                            _logger.LogInformation(string.Format("SendCommercialBlocks - {0} - Update IsSentToAdPointer True", commercialBlock.AdId));
                        }
                        else
                        {
                            //do failover logic with execution history
                            _logger.LogError(string.Format("SendCommercialBlocks - Error sending {0}", commercialBlock.AdId));
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "SendCommercialBlocks");
            }
        }
    }
}
