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
            var path = @"D:\Practice\.NET\ContentSecurityPolicyExample\ContentSecurityPolicyExample\Views";
            ModifyRazorFile(path, "cshtml");
        }

        private static bool ModifyRazorFile(string folderPath, string extension)
        {
            var files = FileExtensions.GetFilePaths(folderPath, extension);
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
                if(hasCspNamespace == false)
                {
                    var hasUnsafeScript = lines.Any(line => (line.Contains(scriptTemplateStartWith) && !line.Contains("src")));
                    if (!hasUnsafeScript)
                    {
                        continue;
                    }
                    var tmpLines = new List<string>();
                    tmpLines.Add("@using NWebsec.Mvc.HttpHeaders.Csp");
                    foreach (var line in lines)
                    {
                        var regex = Regex.Match(line, pattern);
                        if (regex.Success)
                        {
                            if (line.Contains("@Html.CspScriptNonce()") || line.Contains("src"))
                            {
                                tmpLines.Add(line);
                                continue;
                            }
                            var index = line.IndexOf(scriptTemplateStartWith);
                            var newLine = line.Substring(0, index + 7) + " @Html.CspScriptNonce()" + line.Substring(index + 7, line.Length - index - 7);
                            tmpLines.Add(newLine);
                        }
                        else
                        {
                            tmpLines.Add(line);
                        }
                    }
                    File.WriteAllLines(path, tmpLines);
                    tmpLines.Clear();
                }                
            }           
            return true;
        }
    }
}
