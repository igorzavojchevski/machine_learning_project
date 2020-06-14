using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using ML.BL.Interfaces;
using System;

namespace ML.BL.Concrete
{
    public class FrameExporterService : IFrameExporterService
    {
        //TODO - Rework this part for dynamic export
        public void Export()
        {
            string filePath = @"C:\Users\igor.zavojchevski\Desktop\Master\TestMaterial\Videos\Oakley Ski & Snowboarding Prizm Commercial 2017.mp4";
            string outputPath = @"C:\Users\igor.zavojchevski\Desktop\Master\TestMaterial\Frames\Ski";

            using (var engine = new Engine())
            {
                var mp4 = new MediaFile { Filename = filePath };

                engine.GetMetadata(mp4);

                var i = 0;
                while (i < mp4.Metadata.Duration.TotalSeconds)
                {
                    var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(i) };
                    var outputFile = new MediaFile { Filename = string.Format("{0}\\image-{1}.jpeg", outputPath, i) };
                    engine.GetThumbnail(mp4, outputFile, options);
                    i++;
                }
            }
        }
    }
}
