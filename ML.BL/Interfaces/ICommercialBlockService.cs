using System;
using System.Collections.Generic;
using System.Text;

namespace ML.BL.Interfaces
{
    public interface ICommercialBlockService
    {
        void MakeCommercialBlocks();
        void MakeCommercialVideosForBlocks();
        void SendCommercialBlocks();
    }
}
