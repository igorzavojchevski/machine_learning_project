using Microsoft.Extensions.DependencyInjection;
using ML.BL.Mongo.Interfaces;
using ML.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Helpers
{
    public static class ServiceHelper
    {
        private static ISystemSettingService _settingServiceHelper;
        public static ISystemSettingService SystemSettingService
        {
            get
            {
                if (_settingServiceHelper == null) _settingServiceHelper = ServiceProviderHelper.ServiceProvider.GetRequiredService<ISystemSettingService>();
                return _settingServiceHelper;
            }
        }
    }
}
