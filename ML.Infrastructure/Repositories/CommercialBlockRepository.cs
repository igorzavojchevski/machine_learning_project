using ML.Domain.Entities.Mongo;
using ML.Infrastructure.DataContext;
using ML.Infrastructure.Interfaces;

namespace ML.Infrastructure.Repositories
{
    public class CommercialBlockRepository : MongoBaseRepository<CommercialBlock>, ICommercialBlockRepository
    {
        public CommercialBlockRepository(IMongoDbContext mongoDbContext) : base(mongoDbContext)
        {
        }
    }
}
