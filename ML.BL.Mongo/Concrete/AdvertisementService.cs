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
    public class AdvertisementService : MongoBaseService<Advertisement, IAdvertisementRepository>, IAdvertisementService
    {
        private readonly IAdvertisementRepository _advertisementRepository;

        public override IAdvertisementRepository Repository
        {
            get
            {
                return _advertisementRepository;
            }
        }

        public AdvertisementService(IAdvertisementRepository advertisementRepository)
        {
            _advertisementRepository = advertisementRepository;
        }
    }
}
