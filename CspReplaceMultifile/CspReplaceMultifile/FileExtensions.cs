using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CspReplaceMultifile
{
    public static class FileExtensions
    {
        public static List<string> GetFilePaths(string folderName, string fileExtension)
        {
            if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(fileExtension)) return new List<string>();
            var folderPath = new DirectoryInfo(folderName);
            if (!folderPath.Exists) return new List<string>();
            var assemblyFilenames = Directory.GetFiles(folderName, $"*.{fileExtension}", SearchOption.AllDirectories).ToList();            
            return assemblyFilenames;
        }
    }
}
