using ML.Domain.Entities.Mongo;
using ML.Infrastructure.DataContext;
using ML.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Infrastructure.Repositories
{
    public class EvaluationStreamRepository : MongoBaseRepository<EvaluationStream>, IEvaluationStreamRepository
    {
        public EvaluationStreamRepository(IMongoDbContext mongoDbContext) : base(mongoDbContext)
        {
        }
    }
}
