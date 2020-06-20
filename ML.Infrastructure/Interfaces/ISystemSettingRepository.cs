using ML.Domain.Entities.Mongo;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Infrastructure.Interfaces
{
    public interface ISystemSettingRepository : IMongoBaseRepository<SystemSetting>
    {
    }
}
