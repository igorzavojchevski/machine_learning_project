﻿using ML.Domain.DataModels;
using MongoDB.Bson;
using System;

namespace ML.BL
{
    public interface IScoringService
    {
        void Score();
        void DoLabelScoring(Guid GroupGuid, InMemoryImageData image, ObjectId evaluationStreamID);
    }
}
