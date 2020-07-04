using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.BL.Mongo.Interfaces;
using ML.Domain.Entities.Enums;
using ML.Domain.Entities.Mongo;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace ML.BL.Concrete
{
    public class ArchivingService : IArchivingService
    {
        private readonly ILogger<ArchivingService> _logger;
        private readonly ISystemSettingService _systemSettingService;
        private readonly IEvaluationGroupService _evaluationGroupService;

        public ArchivingService(
            ILogger<ArchivingService> logger,
            ISystemSettingService systemSettingService,
            IEvaluationGroupService evaluationGroupService)
        {
            _logger = logger;
            _systemSettingService = systemSettingService;
            _evaluationGroupService = evaluationGroupService;
        }

        public void ArchiveImages()
        {
            _logger.LogInformation("ArchivingService - ArchiveImages - started");

            string archiveLastStartDateString = _systemSettingService.Archive_LastStartDate;
            bool isParsable = DateTime.TryParse(archiveLastStartDateString, out DateTime archiveLastStartDate);
            if (!isParsable) { _logger.LogInformation("ArchivingService - ArchiveImages - Archive_StartDate does not have appropriate format"); return; }

            if ((DateTime.UtcNow - archiveLastStartDate).TotalMinutes < _systemSettingService.Archive_NextStartPeriod_Minutes)
            { 
                _logger.LogInformation("ArchivingService - ArchiveImages - Not in Archive_NextStartPeriod_Minutes"); 
                return; 
            }

            List<EvaluationGroup> evaluationGroupsToArchive = _evaluationGroupService.GetAll().Where(t => t.Status == TrainingStatus.Processed).ToList();

            foreach (EvaluationGroup evaluationGroup in evaluationGroupsToArchive)
            {
                UpdateEvaluationGroup(evaluationGroup, TrainingStatus.Archiving, null);

                string dirName = Path.GetFileName(evaluationGroup.DirPath);
                string zipDirPath = Path.Combine(_systemSettingService.Archive_Path_For_Trained_Images, dirName + ".zip");
                if (File.Exists(zipDirPath)) continue;

                ZipFile.CreateFromDirectory(evaluationGroup.DirPath, zipDirPath, CompressionLevel.Optimal, false);
                Directory.Delete(evaluationGroup.DirPath, true);

                UpdateEvaluationGroup(evaluationGroup, TrainingStatus.Archived, zipDirPath);
            }

            UpdateArchiveLastStartDate();

            _logger.LogInformation("ArchivingService - ArchiveImages - finished");
        }

        private void UpdateArchiveLastStartDate()
        {
            var systemSetting = _systemSettingService.GetAll().Where(t => t.SettingKey == "Archive_LastStartDate").FirstOrDefault();
            systemSetting.SettingValue = DateTime.UtcNow.ToString();
            _systemSettingService.Update(systemSetting);
        }

        private void UpdateEvaluationGroup(EvaluationGroup evaluationGroup, TrainingStatus newStatus, string zipDirPath)
        {
            evaluationGroup.ZipDirPath = zipDirPath;
            evaluationGroup.Status = newStatus;
            evaluationGroup.ModifiedOn = DateTime.UtcNow;
            evaluationGroup.ModifiedBy = "ArchivingService";

            _evaluationGroupService.Update(evaluationGroup);
        }
    }
}
