using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ML.BL.Concrete
{
    public class FrameExporterService : IFrameExporterService
    {
        private readonly ILogger<FrameExporterService> _logger;
        private readonly ISystemSettingService _systemSettingService;

        public FrameExporterService(ILogger<FrameExporterService> logger, ISystemSettingService systemSettingService)
        {
            _logger = logger;
            _systemSettingService = systemSettingService;
        }

        public void Export(Process process)
        {
            try
            {
                string dirName = _systemSettingService.CUSTOMLOGOMODEL_ExportedFromService_ImagesToEvaluateFolderPath;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.FileName = @"C:\ffmpeg\bin\ffmpeg.exe";

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                string hlsstream = _systemSettingService.HLSStream_URL;

                process.StartInfo.Arguments = $"-skip_frame nokey -i {hlsstream} -vsync 0 -r 30 -f image2 {dirName}\\{Guid.NewGuid()}-%02d.jpeg";
                process.Start();

                while ((DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalMinutes <= 1)
                {
                }
                process.Kill();
                Export(new Process());
            }
            catch (Exception ex)
            {
                _logger.LogError("FrameExporterService - Export");
                _logger.LogError(ex, "FrameExporterService - Export");
            }
        }
    }
}
