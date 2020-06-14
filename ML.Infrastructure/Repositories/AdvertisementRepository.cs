using DnsClient.Internal;
using ML.Domain.Entities.Mongo;
using ML.Infrastructure.DataContext;
using ML.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Infrastructure.Repositories
{
    public class AdvertisementRepository : MongoBaseRepository<Advertisement>, IAdvertisementRepository
    {
        public AdvertisementRepository(IMongoDbContext mongoDbContext) : base(mongoDbContext)
        {
        }
    }
}
