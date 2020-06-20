using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.DataModels
{
    public class InMemoryImageData
    {
        public InMemoryImageData(byte[] image, string label, string imageFileName, string imageFilePath)
        {
            Image = image;
            Label = label;
            ImageFileName = imageFileName;
            ImageFilePath = imageFilePath;
        }

        public readonly byte[] Image;
        public readonly string Label;
        public readonly string ImageFileName;
        public readonly string ImageFilePath;
    }
}
