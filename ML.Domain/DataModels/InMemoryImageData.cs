using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.DataModels
{
    public class InMemoryImageData
    {
        public InMemoryImageData(byte[] image, string label, string imageFileName, string imageFilePath, string imageDirPath, DateTime imageDateTime)
        {
            Image = image;
            Label = label;
            ImageFileName = imageFileName;
            ImageFilePath = imageFilePath;
            ImageDirPath = imageDirPath;
            ImageDateTime = imageDateTime;
        }

        public byte[] Image { get; set; }
        public string Label { get; set; }
        public string ImageFileName { get; set; }
        public string ImageFilePath { get; set; }
        public string ImageDirPath { get; set; }
        public DateTime ImageDateTime { get; set; }
    }
}
