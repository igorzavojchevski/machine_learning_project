using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ML.Utils.Extensions.Base
{
    public static class BaseExtensions
    {
        //TODO - Rewrite this method
        public static string GetPath(string path, bool isAbsolute)
        {
            if (isAbsolute) return path;

            var _dataRoot = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, path);
            return fullPath;
        }
    }
}
