using ML.BL.Mongo.Concrete.Base;
using ML.BL.Mongo.Interfaces;
using ML.Domain.Entities.Mongo;
using ML.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Mongo.Concrete
{
    public class LabelClassService : MongoBaseService<LabelClass, ILabelClassRepository>, ILabelClassService
    {
        private readonly ILabelClassRepository _labelClassRepository;

        public override ILabelClassRepository Repository
        {
            get
            {
                return _labelClassRepository;
            }
        }

        public LabelClassService(ILabelClassRepository labelClassRepository)
        {
            _labelClassRepository = labelClassRepository;
        }
    }
}
