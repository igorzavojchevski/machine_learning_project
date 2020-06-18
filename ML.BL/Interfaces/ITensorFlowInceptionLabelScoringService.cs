using ML.Domain.DataModels;
using System.IO;

namespace ML.BL.Interfaces
{
    public interface ITensorFlowInceptionLabelScoringService
    {
        ImagePredictedLabelWithProbability DoLabelScoring(FileInfo fileInfo);
    }
}
