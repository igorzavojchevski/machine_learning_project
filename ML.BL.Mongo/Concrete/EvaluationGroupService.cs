using ML.BL.Mongo.Concrete.Base;
using ML.BL.Mongo.Interfaces;
using ML.Domain.Entities.Mongo;
using ML.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Mongo.Concrete
{
    public class EvaluationGroupService : MongoBaseService<EvaluationGroup, IEvaluationGroupRepository>, IEvaluationGroupService
    {
        private readonly IEvaluationGroupRepository _evaluationGroupRepository;

        public override IEvaluationGroupRepository Repository
        {
            get
            {
                return _evaluationGroupRepository;
            }
        }

        public EvaluationGroupService(IEvaluationGroupRepository evaluationGroupRepository)
        {
            _evaluationGroupRepository = evaluationGroupRepository;
        }
    }
}
