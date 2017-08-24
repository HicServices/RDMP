using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BundleUpSourceIntoZip
{
    public class Cleanser
    {
        private DirectoryInfo _directoryToCleanse;
        
        public Cleanser(DirectoryInfo toCleanse)
        {
            if(!toCleanse.Exists)
                throw new DirectoryNotFoundException("Could not find directory we were supposed to cleanse:" + toCleanse.FullName);

            _directoryToCleanse = toCleanse;
        }

        public void Cleanse()
        {
            FileInfo[] filesToCleasne = _directoryToCleanse.GetFiles();

            if(!filesToCleasne.Any())
                throw new FileNotFoundException("There were no files found in directory " + _directoryToCleanse.FullName);

            string[] blackList = new string[]
            {
                //add any files you don't want appearing in source code zip here
            };

            foreach (FileInfo fileInfo in filesToCleasne)
            {
                string[] lines = File.ReadAllLines(fileInfo.FullName);
                lines = CleanseLines(lines);
                File.WriteAllLines(fileInfo.FullName,lines);


                if(blackList.Contains(fileInfo.Name))
                    fileInfo.Delete();
            }
        }

        Regex passwords = new Regex(@"password\s*=\s*""(.*)""", RegexOptions.IgnoreCase);
        private string[] CleanseLines(string[] lines)
        {
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                
                Match match = passwords.Match(line);
                if (match.Success)
                    if (!string.IsNullOrWhiteSpace(match.Groups[1].Value))//something like Password = "" is fine
                    lines[index] = line.Replace(match.Groups[1].Value, "REDACTED");
            }
            

            //Important do not change the number of lines or line numbers will be off for stack traces
            
            return lines;
        }
    }
}
