using ML.Domain.Entities.Mongo;
using ML.Domain.ReturnModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Helpers
{
    public static class SystemSettingHelper
    {
        public static SystemSettingReturnModel ToSystemSettingReturnModel(this SystemSetting entity)
        {
            if (entity == null) return new SystemSettingReturnModel();

            return new SystemSettingReturnModel()
            {
                Id = entity.Id.ToString(),
                SettingKey = entity.SettingKey,
                SettingValue = entity.SettingValue,
                ModifiedOn = entity.ModifiedOn,
                ModifiedBy = entity.ModifiedBy
            };
        }
    }
}
