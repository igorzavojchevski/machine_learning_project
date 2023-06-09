﻿using System;
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

        public readonly byte[] Image;
        public readonly string Label;
        public readonly string ImageFileName;
        public readonly string ImageFilePath;
        public readonly string ImageDirPath;
        public readonly DateTime ImageDateTime;
    }
}
