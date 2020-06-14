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
        public static string GetAbsolutePath(string relativePath)
        {
            var _dataRoot = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);
            return fullPath;
        }
    }
}
