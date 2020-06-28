using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.Entities.Enums;
using ML.Domain.Entities.Mongo;
using System;
using System.Diagnostics;
using System.IO;
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

        public FrameExporterService(ILogger<FrameExporterService> logger, ISystemSettingService systemSettingService, IEvaluationGroupService evaluationGroupService)
        {
            _logger = logger;
            _systemSettingService = systemSettingService;
            _evaluationGroupService = evaluationGroupService;
        }

        public void Export()
        {
            _logger.LogInformation("FrameExporterService - Export started");

            Process process = new Process();
            string dirName = _systemSettingService.CUSTOMLOGOMODEL_ExportedFromService_ImagesToEvaluateFolderPath;
            process.StartInfo.FileName = _systemSettingService.FFMPEG_ExecutablePath;

            process.StartInfo.UseShellExecute = true;
            process.StartInfo.CreateNoWindow = true;

            string hlsstream = _systemSettingService.HLSStream_URL;

            while (true)
            {
                string newDir = Path.Combine(dirName, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
                Task.Factory.StartNew(() =>
                {
                    DoWork(process, hlsstream, newDir);
                });
                Thread.Sleep(TimeSpan.FromMinutes(1));
                process.Kill();
            }
        }

        public void DoWork(Process process, string hlsstream, string newDir)
        {
            _logger.LogInformation("FrameExporterService - Export - DoWork started");

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
                _logger.LogError("FrameExporterService - Export - DoWork");
                _logger.LogError(ex, "FrameExporterService - Export - DoWork");
            }
            finally
            {
                _evaluationGroupService.InsertOne(new EvaluationGroup() { EvaluationGroupDirPath = newDir, EvaluationGroupGuid = newGuid, ParentGroupGuid = newGuid, Status = status, ModifiedBy = "ExportService", ModifiedOn = DateTime.UtcNow });
            }

            _logger.LogInformation("FrameExporterService - Export - DoWork finished");
        }
    }
}
