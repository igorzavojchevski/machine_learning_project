using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.Entities.Enums;
using ML.Domain.Entities.Mongo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ML.BL.Concrete
{
    public class FrameExporterService : IFrameExporterService
    {
        private readonly ILogger<FrameExporterService> _logger;
        private readonly ISystemSettingService _systemSettingService;
        private readonly IEvaluationGroupService _evaluationGroupService;
        private readonly IEvaluationStreamService _evaluationStreamService;

        public FrameExporterService(
            ILogger<FrameExporterService> logger, 
            ISystemSettingService systemSettingService, 
            IEvaluationGroupService evaluationGroupService,
            IEvaluationStreamService evaluationStreamService)
        {
            _logger = logger;
            _systemSettingService = systemSettingService;
            _evaluationGroupService = evaluationGroupService;
            _evaluationStreamService = evaluationStreamService;
        }

        public void Export()
        {
            _logger.LogInformation("FrameExporterService - Export started");

            string dirName = _systemSettingService.CUSTOMLOGOMODEL_ExportedFromService_ImagesToEvaluateFolderPath;

            List<EvaluationStream> evaluationStreams = _evaluationStreamService.GetAll().Where(t => t.IsActive).ToList();
            if (evaluationStreams == null || evaluationStreams.Count == 0) { _logger.LogInformation("FrameExporterService - no evaluation streams"); return; }
            
            foreach(EvaluationStream evaluationStream in evaluationStreams)
            {
                Task.Factory.StartNew(() =>
                {
                    DoWork(evaluationStream, dirName);
                });
            }
        }

        public void DoWork(EvaluationStream evaluationStream, string dirName)
        {
            _logger.LogInformation("FrameExporterService - DoWork - started for EvaluationStream - {0}, Stream - {1}", evaluationStream.Name, evaluationStream.Stream);
            
            Process process = new Process();
            process.StartInfo.FileName = _systemSettingService.FFMPEG_ExecutablePath;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = true;

            while (true)
            {
                string newDir = Path.Combine(dirName, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + evaluationStream.Code);
                Task.Factory.StartNew(() =>
                {
                    ProcessExport(process, evaluationStream.Stream, newDir);
                });
                Thread.Sleep(TimeSpan.FromMinutes(1));
                process.Kill();
            }
        }

        public void ProcessExport(Process process, string hlsstream, string newDir)
        {
            _logger.LogInformation("FrameExporterService - Export - DoWork started - {0}", hlsstream);

            Guid newGuid = Guid.NewGuid();
            TrainingStatus status = TrainingStatus.New;
            try
            {
                Directory.CreateDirectory(newDir);
                process.StartInfo.Arguments = $"-skip_frame nokey -i {hlsstream} -vsync 0 -r 30 -f image2 -strftime 1 {newDir}\\{newGuid}_%Y%m%d%H%M%S.jpeg";
                process.Start();

            }
            catch (Exception ex)
            {
                status = TrainingStatus.CreatedWithError;
                _logger.LogError("FrameExporterService - Export - DoWork - {0}", hlsstream);
                _logger.LogError(ex, "FrameExporterService - Export - DoWork - {0}", hlsstream);
            }
            finally
            {
                _evaluationGroupService.InsertOne(new EvaluationGroup() { DirPath = newDir, EvaluationGroupGuid = newGuid, ParentGroupGuid = newGuid, Status = status, ModifiedBy = "ExportService", ModifiedOn = DateTime.UtcNow });
            }

            _logger.LogInformation("FrameExporterService - Export - DoWork finished {0}", hlsstream);
        }
    }
}
