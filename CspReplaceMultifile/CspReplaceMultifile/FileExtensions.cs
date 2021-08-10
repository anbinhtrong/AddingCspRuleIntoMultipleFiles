using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CspReplaceMultifile
{
    public class FileExtensions
    {
        private string _javascriptVoidPattern = "href=\\s*[\",\']\\s*javascript:\\s*void\\(0\\)\\s*;*\\s*[\",\']";
        public List<string> GetFilePaths(string folderName, string fileExtension)
        {
            if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(fileExtension)) return new List<string>();
            var folderPath = new DirectoryInfo(folderName);
            if (!folderPath.Exists) return new List<string>();
            var assemblyFilenames = Directory.GetFiles(folderName, $"*.{fileExtension}", SearchOption.AllDirectories).ToList();            
            return assemblyFilenames;
        }

        public bool HasJavascriptVoid(List<string> lines)
        {
            var hasJsVoid = false;
            foreach (var line in lines)
            {
                var regex = Regex.Match(line, _javascriptVoidPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (regex.Success)
                {
                    hasJsVoid = true;
                    break;
                }
            }
                
            return hasJsVoid;
        }
        /// <summary>
        /// suppose 1 line has 1 _javascriptVoidPattern
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public List<string> RemoveJavasscriptVoid(List<string> lines)
        {
            var pattern = "href=\\s*[\",\']\\s*javascript:\\s*void\\(0\\)\\s*;*\\s*[\",\']";
            var result = new List<string>();
            foreach (var line in lines)
            {                
                var regex = Regex.Match(line, pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var hasModified = false;
                var tmpLine = string.Empty;
                if (regex.Success)
                {
                    // Find matches.
                    var matchValue = regex.Value;
                    Console.WriteLine(matchValue);
                    tmpLine = line.Replace(matchValue, "");
                    hasModified = true;
                }
                if (hasModified)
                    result.Add(tmpLine);
                else
                    result.Add(line);
            }            
            return result;
        }

        public List<string> ReplaceJavasscriptVoid(List<string> lines)
        {
            var pattern = "href=\\s*[\",\']\\s*javascript:\\s*void\\(0\\)\\s*;*\\s*[\",\']";
            var jsFilePattern = "<script.*?src=[\",\'](.*?)[\",\']>";
            var result = new List<string>();
            var hasJsVoid = false;
            foreach (var line in lines)
            {
                var regex = Regex.Match(line, pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var hasModified = false;
                var tmpLine = string.Empty;
                if (regex.Success)
                {
                    // Find matches.
                    var matchValue = regex.Value;
                    Console.WriteLine(matchValue);
                    tmpLine = line.Replace(matchValue, "href class='hyperlink'");
                    hasModified = true;
                    hasJsVoid = true;
                }
                if (hasModified)
                {                                        
                    result.Add(tmpLine);                    
                }
                else
                    result.Add(line);
            }            
            if (!hasJsVoid) return result;
            var finalResult = new List<string>();
            for (var i = 0; i < result.Count; i++)
            {
                finalResult.Add(result[i]);
                if (result[i].Contains("<script") && hasJsVoid)
                {
                    var regexSrc = Regex.Match(result[i], jsFilePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if (regexSrc.Success)
                    {
                        continue;
                    }                    
                    var newLine = @"        $('document').ready(function () {
            $('.hyperlink').click(function (e) {
                e.preventDefault();
            }
        });";
                    finalResult.Add(newLine);
                    hasJsVoid = false;
                }                
            }
            if (hasJsVoid)
            {
                var newLine = @"<script type='text/javascript'>
$('document').ready(function () {
            $('.hyperlink').click(function (e) {
                e.preventDefault();
            }
        });
</script>";
                finalResult.Add(newLine);
            }
            return finalResult;
        }

        public List<string> AddNonce(List<string> lines, string pattern, string scriptTemplateStartWith)
        {
            var tmpLines = new List<string>
            {
                "@using NWebsec.Mvc.HttpHeaders.Csp"
            };
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
            return tmpLines;
        }
    }
}
