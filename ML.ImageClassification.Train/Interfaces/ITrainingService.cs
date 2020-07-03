using System;
using System.Collections.Generic;
using System.Text;

namespace ML.ImageClassification.Train.Interfaces
{
    public interface ITrainingService
    {
        void DoBeforeTrainingStart();
        void DoAfterTrainingFinished();
        void Train();
        void DoCleanup();
    }
}
