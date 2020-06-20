using ML.Domain.DataModels.TrainingModels;
using ML.Utils.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ML.Utils.Extensions.Base
{
    public static class BaseExtensions
    {
        //TODO - Rewrite this method
        public static string GetPath(string path, bool isAbsolute = true)
        {
            if (isAbsolute) return path;

            var _dataRoot = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, path);
            return fullPath;
        }

        public static IEnumerable<(string imagePath, string label)> LoadImagesFromDirectory(
            string folder,
            bool useFolderNameasLabel)
        {
            string[] imageSupportedFormats = Enum.GetNames(typeof(ImageFormat));

            var imagesPath = Directory
                .GetFiles(folder, "*", searchOption: SearchOption.AllDirectories)
                .Where(x => imageSupportedFormats.Contains(Path.GetExtension(x).Replace(".", "")));
            //.Where(x => imageSupportedFormats.Any(supportedformat => nameof(supportedformat) == Path.GetExtension(x).Replace(".", "")));
            //Path.GetExtension(x) == ".jpg" || Path.GetExtension(x) == ".png");

            return useFolderNameasLabel
                ? imagesPath.Select(imagePath => (imagePath, Directory.GetParent(imagePath).Name))
                : imagesPath.Select(imagePath =>
                {
                    var label = Path.GetFileName(imagePath);
                    for (var index = 0; index < label.Length; index++)
                    {
                        if (!char.IsLetter(label[index]))
                        {
                            label = label.Substring(0, index);
                            break;
                        }
                    }
                    return (imagePath, label);
                });
        }

        public static IEnumerable<InMemoryImageData> LoadInMemoryImagesFromDirectory(
            string folder,
            bool useFolderNameAsLabel = true)
            => LoadImagesFromDirectory(folder, useFolderNameAsLabel)
                .Select(x => new InMemoryImageData(
                    image: File.ReadAllBytes(x.imagePath),
                    label: x.label,
                    imageFileName: Path.GetFileName(x.imagePath)));
    }
}
