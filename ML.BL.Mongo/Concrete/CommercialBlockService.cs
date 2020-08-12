using ML.BL.Mongo.Concrete.Base;
using ML.BL.Mongo.Interfaces;
using ML.Domain.Entities.Mongo;
using ML.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Mongo.Concrete
{
    public class CommercialBlockService : MongoBaseService<CommercialBlock, ICommercialBlockRepository>, ICommercialBlockService
    {
        private readonly ICommercialBlockRepository _commercialRepository;

        public override ICommercialBlockRepository Repository
        {
            get
            {
                return _commercialRepository;
            }
        }

        public CommercialBlockService(ICommercialBlockRepository commercialRepository)
        {
            _commercialRepository = commercialRepository;
        }
    }
}
