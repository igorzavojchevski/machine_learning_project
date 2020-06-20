using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ML.BL.Interfaces;
using ML.Domain.DataModels.TrainingModels;
using ML.Utils.Extensions.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ML.BL
{
    //public abstract class ScoringService : IScoringService
    //{
    //    private readonly ILogger<ScoringService> _logger;

    //    public ScoringService()
    //    {

    //    }

    //    public virtual void Score()
    //    {
    //    }
    //}

    public interface IScoringService
    {
        void Score(string imagesToCheckPath);
        void DoLabelScoring(Guid GroupGuid, InMemoryImageData image);
    }

    public interface IScoringServiceFactory
    {
        IScoringService Create(ScoringServiceType type);
    }

    public class ScoringServiceFactory : IScoringServiceFactory
    {
        private readonly IServiceProvider serviceProvider;
        public ScoringServiceFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IScoringService Create(ScoringServiceType type)
        {
            switch (type)
            {
                case ScoringServiceType.LogoScoring:
                    return (IScoringService)serviceProvider.GetRequiredService<ILogoScoringService>();
                case ScoringServiceType.TensorFlowInception:
                    return (IScoringService)serviceProvider.GetRequiredService<ILabelScoringService>();
                default:
                    return null;
            }
            //(IScoringService)serviceProvider.GetService(typeof(ScoringServiceType));
        }
    }
}
