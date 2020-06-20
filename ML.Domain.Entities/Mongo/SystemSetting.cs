using ML.Domain.Entities.Mongo.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.Entities.Mongo
{
    public class SystemSetting : MongoBaseEntity
    {
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
    }
}
