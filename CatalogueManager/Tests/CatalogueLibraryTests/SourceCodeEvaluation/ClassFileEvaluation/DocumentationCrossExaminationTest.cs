using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CatalogueLibraryTests.SourceCodeEvaluation.ClassFileEvaluation
{
    class DocumentationCrossExaminationTest
    {
        Regex matchComments = new Regex(@"///[^;\r\n]*");

        private string[] _mdFiles;
        Regex matchMdReferences = new Regex(@"`(.*)`");

        //words that are in Pascal case and you can use in comments despite not being in the codebase... this is an ironic variable to be honest
        //since the very fact that you add something to _whitelist means that it is in the codebase after all!
        private string[] _whitelist = new []
        {
            "MyDateCol",
        };

        public DocumentationCrossExaminationTest(DirectoryInfo slndir)
        {
            var mdDirectory = Path.Combine(slndir.FullName, @"Documentation", "CodeTutorials");

            _mdFiles = Directory.GetFiles(mdDirectory, "*.md");
        }

        public void FindProblems(List<string> csFilesFound)
        {
            List<string> problems = new List<string>();

            HashSet<string> codeTokens = new HashSet<string>();
            Dictionary<string, HashSet<string>> fileCommentTokens = new Dictionary<string, HashSet<string>>();

            //find all comments in class files
            foreach (string file in csFilesFound)
            {
                if(file.Contains(".Designer.cs"))
                    continue;

                foreach (string line in File.ReadAllLines(file))
                {
                    //if it is a comment
                    if (matchComments.IsMatch(line))
                    {
                        if (!fileCommentTokens.ContainsKey(file))
                            fileCommentTokens.Add(file, new HashSet<string>());
                        
                        //its a comment extract all pascal case words
                        foreach (Match word in Regex.Matches(line, @"([A-Z]\w+){2,}"))
                            fileCommentTokens[file].Add(word.Value);
                    }
                    else
                    {
                        //else it is a code line, extract all tokens
                        foreach (Match word in Regex.Matches(line, @"\w+"))
                            codeTokens.Add(word.Value);
                    }
                }
            }

            //find all comments in .md tutorials
            foreach (string mdFile in _mdFiles)
            {
                fileCommentTokens.Add(mdFile,new HashSet<string>());
                var fileContents = File.ReadAllText(mdFile);
                
                foreach (Match m in matchMdReferences.Matches(fileContents))
                    foreach (Match word in Regex.Matches(m.Groups[1].Value, @"([A-Z]\w+){2,}"))
                        fileCommentTokens[mdFile].Add(word.Value);
            }

            foreach (KeyValuePair<string, HashSet<string>> kvp in fileCommentTokens)
            {
                foreach (string s in kvp.Value)
                {
                    if(_whitelist.Contains(s))
                        continue;
                    
                    if(!codeTokens.Contains(s))
                        problems.Add("FATAL PROBLEM: File " + kvp.Key +" talks about something which isn't in the codebase, called a:" +Environment.NewLine + s);
                }
            }


            foreach (string problem in problems)
                Console.WriteLine(problem);

            Assert.AreEqual(0,problems.Count,"Expected there to be nothing talked about in comments that doesn't appear in the codebase somewhere");

            //find all non coment code and extract all unique tokens

            //find all .md files and extract all `` code blocks

            //for each commend and `` code block

            //identify Pascal case words

            //are they in the codebase tokens?
        }
    }
}
