using ML.Domain.DataModels;
using ML.Domain.DataModels.TrainingModels;
using System.IO;

namespace ML.BL.Interfaces
{
    public interface ITensorFlowInceptionLabelScoringService
    {
        ImagePredictedLabelWithProbability DoLabelScoring(InMemoryImageData fileInfo);
    }
}
