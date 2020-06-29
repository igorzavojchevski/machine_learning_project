using ML.BL.Mongo.Concrete.Base;
using ML.BL.Mongo.Interfaces;
using ML.BL.Mongo.Interfaces.Base;
using ML.Domain.Entities.Mongo;
using ML.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Mongo.Concrete
{
    public class CommercialService : MongoBaseService<Commercial, ICommercialRepository>, ICommercialService
    {
        private readonly ICommercialRepository _commercialRepository;

        public override ICommercialRepository Repository
        {
            get
            {
                return _commercialRepository;
            }
        }

        public CommercialService(ICommercialRepository commercialRepository)
        {
            _commercialRepository = commercialRepository;
        }
    }
}
