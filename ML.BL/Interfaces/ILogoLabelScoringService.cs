﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Interfaces
{
    public interface ILogoLabelScoringService
    {
        void Score(string imagesToCheckPath);
    }
}