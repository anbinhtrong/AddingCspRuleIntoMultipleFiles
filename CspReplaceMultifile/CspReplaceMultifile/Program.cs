using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CspReplaceMultifile
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var path = @"D:\Practice\.NET\ContentSecurityPolicyExample\ContentSecurityPolicyExample";
            //ModifyRazorFile(path, "cshtml");
            ReplaceJavascriptVoid(path, "cshtml");
        }

        private static bool ModifyRazorFile(string folderPath, string extension)
        {
            var fileExtension = new FileExtensions();
            var files = fileExtension.GetFilePaths(folderPath, extension);
            if (files.Count == 0) return false;
            var scriptTemplateStartWith = "<script";//
            var cspNameSpaceStartWith = "@using NWebsec.Mvc.HttpHeaders.Csp";
            var pattern = "<script(.)*>";
            foreach (var path in files)
            {
                var file = new FileInfo(path);
                if (!file.Exists)
                    continue;
                //check csp exist
                //1-check script is exist in the file
                var lines = File.ReadLines(path).ToList();
                var hasCspNamespace = lines.Any(line => (line.StartsWith(cspNameSpaceStartWith)));
                var modified = false;
                if (hasCspNamespace == false)
                {                    
                    var hasUnsafeScript = lines.Any(line => (line.Contains(scriptTemplateStartWith) && !line.Contains("src")));
                    if (hasUnsafeScript)
                    {
                        lines = fileExtension.AddNonce(lines, pattern, scriptTemplateStartWith);
                        modified = true;
                    }                                        
                }
                if (fileExtension.HasJavascriptVoid(lines))
                {
                    lines = fileExtension.RemoveJavasscriptVoid(lines);
                    modified = true;
                }
                if (modified)
                    File.WriteAllLines(path, lines);
            }           
            return true;
        }

        private static bool ReplaceJavascriptVoid(string folderPath, string extension)
        {
            var fileExtension = new FileExtensions();
            var files = fileExtension.GetFilePaths(folderPath, extension);
            if (files.Count == 0) return false;            
            foreach (var path in files)
            {
                var file = new FileInfo(path);
                if (!file.Exists)
                    continue;
                //check csp exist
                //1-check script is exist in the file
                var lines = File.ReadLines(path).ToList();
                var modified = false;                
                if (fileExtension.HasJavascriptVoid(lines))
                {
                    lines = fileExtension.ReplaceJavasscriptVoid(lines);
                    modified = true;
                }
                if (modified)
                    File.WriteAllLines(path, lines);
            }
            return true;
        }


    }
}
