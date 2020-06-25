using ML.BL.Mongo.Interfaces.Base;
using ML.Domain.Entities.Mongo;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Mongo.Interfaces
{
    public interface ILabelClassService : IMongoBaseService<LabelClass>
    {
    }
}
