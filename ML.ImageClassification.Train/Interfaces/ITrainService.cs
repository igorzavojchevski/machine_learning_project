using System;
using System.Collections.Generic;
using System.Text;

namespace ML.ImageClassification.Train.Interfaces
{
    public interface ITrainService
    {
        void DoBeforeTrainingStart();
        void DoAfterTrainingFinished();

        void Train();
    }
}
