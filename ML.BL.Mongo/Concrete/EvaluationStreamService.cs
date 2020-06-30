using ML.BL.Mongo.Concrete.Base;
using ML.BL.Mongo.Interfaces;
using ML.Domain.Entities.Mongo;
using ML.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Mongo.Concrete
{
    public class EvaluationStreamService : MongoBaseService<EvaluationStream, IEvaluationStreamRepository>, IEvaluationStreamService
    {
        private readonly IEvaluationStreamRepository _evaluationStreamRepository;

        public override IEvaluationStreamRepository Repository
        {
            get
            {
                return _evaluationStreamRepository;
            }
        }

        public EvaluationStreamService(IEvaluationStreamRepository evaluationStreamRepository)
        {
            _evaluationStreamRepository = evaluationStreamRepository;
        }
    }
}
