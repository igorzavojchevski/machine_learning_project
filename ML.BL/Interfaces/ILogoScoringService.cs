using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Interfaces
{
    public interface ILogoScoringService
    {
        void Score(string imagesToCheckPath);
    }
}
