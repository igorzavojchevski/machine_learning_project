using ML.BL.Mongo.Concrete.Base;
using ML.BL.Mongo.Interfaces;
using ML.Domain.Entities.Mongo;
using ML.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Mongo.Concrete
{
    public class AdvertisementClassService : MongoBaseService<AdvertisementClass, IAdvertisementClassRepository>, IAdvertisementClassService
    {
        private readonly IAdvertisementClassRepository _advertisementClassRepository;

        public override IAdvertisementClassRepository Repository
        {
            get
            {
                return _advertisementClassRepository;
            }
        }

        public AdvertisementClassService(IAdvertisementClassRepository advertisementClassRepository)
        {
            _advertisementClassRepository = advertisementClassRepository;
        }
    }
}
