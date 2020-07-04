using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.ReturnModels
{
    public class SystemSettingReturnModel
    {
        public string Id { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
}
