using ML.Domain.DataModels;
using ML.Domain.DataModels.CustomLogoTrainingModel;
using ML.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ML.Utils.Extensions.Base
{
    public static class BaseExtensions
    {
        //TODO - Rewrite this method
        public static string GetPath(string path) //, bool isAbsolute = true)
        {
            //if (isAbsolute) return path;
            return path;
            //var _dataRoot = new FileInfo(Assembly.GetExecutingAssembly().Location);
            //string assemblyFolderPath = _dataRoot.Directory.FullName;

            //string fullPath = Path.Combine(assemblyFolderPath, path);
            //return fullPath;
        }

        public static IEnumerable<(string imagePath, string label)> LoadImagesFromDirectory(
            string folder,
            bool useFolderNameAsLabel,
            bool useNewItems = true)
        {
            string[] imageSupportedFormats = Enum.GetNames(typeof(ImageFormat));

            var imagesPath = 
                useNewItems ?
                Directory.GetFiles(folder, "*", searchOption: SearchOption.AllDirectories)
                .Where(x => imageSupportedFormats.Contains(Path.GetExtension(x).Replace(".", ""))) :
                Directory.GetFiles(folder, "*", searchOption: SearchOption.AllDirectories)
                .Where(x => !x.ToLower().Contains("new_item") && imageSupportedFormats.Contains(Path.GetExtension(x).Replace(".", "")));

            return useFolderNameAsLabel
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

        public static IEnumerable<InMemoryImageData> LoadInMemoryImagesFromDirectory(string folder, bool useFolderNameAsLabel = true, bool useNewItem = true)
        {
            return LoadImagesFromDirectory(folder, useFolderNameAsLabel, useNewItem)
                           .Select(x => new InMemoryImageData(
                               image: File.ReadAllBytes(x.imagePath),
                               label: x.label,
                               imageFileName: Path.GetFileName(x.imagePath),
                               imageFilePath: x.imagePath,
                               imageDirPath: Path.GetDirectoryName(x.imagePath),
                               imageDateTime: DateTime.ParseExact(Path.GetFileNameWithoutExtension(x.imagePath).Split("_")[1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture)
                               ));
        }

        public static bool IsFileLocked(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists) return false;

                using (FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
    }
}
