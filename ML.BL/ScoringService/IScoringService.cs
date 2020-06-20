using ML.Domain.DataModels;
using System;

namespace ML.BL
{
    public interface IScoringService
    {
        void Score(string imagesToCheckPath);
        void DoLabelScoring(Guid GroupGuid, InMemoryImageData image);
    }
}
