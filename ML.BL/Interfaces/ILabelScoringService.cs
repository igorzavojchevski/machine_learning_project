using ML.Domain.DataModels;

namespace ML.BL.Interfaces
{
    public interface ILabelScoringService
    {
        void Score();
        ImagePredictedLabelWithProbability CheckImageForLabelScoring(InMemoryImageData image);
    }
}
