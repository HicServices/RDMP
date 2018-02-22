using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace CatalogueLibraryTests.SourceCodeEvaluation.ClassFileEvaluation
{
    public class ExplicitDatabaseNameChecker
    {
        public void FindProblems(List<string> csFilesFound)
        {
            Dictionary<string,string> problemFiles = new Dictionary<string, string>();
            List<string> prohibitedStrings = new List<string>();
            
            List<string> whitelist = new List<string>();
            whitelist.Add("ExplicitDatabaseNameChecker.cs"); //us obviously since we do contain that text!
            whitelist.Add("DatabaseCreationProgramOptions.cs"); //allowed because it is the usage text for the program.
            whitelist.Add("AutomationServiceOptions.cs");//allowed because it is the usage text for the program.
            whitelist.Add("DatabaseTests.cs"); //allowed because it is telling user about how you can setup database tests support
            whitelist.Add("ChoosePlatformDatabases.Designer.cs"); //allowed because it is a suggestion to user about what prefix to use
            whitelist.Add("PluginPackagerProgramOptions.cs"); //allwed because it's a suggestion to the user about command line arguments
            whitelist.Add("DocumentationCrossExaminationTest.cs"); //allowed because its basically a list of comments that are allowed despite not appearing in the codebase

            prohibitedStrings.Add("TEST_");
            prohibitedStrings.Add("RDMP_");

            foreach (string file in csFilesFound)
            {
                if (whitelist.Any(str=>str.Equals(Path.GetFileName(file))))
                    continue;
                
                var contents = File.ReadAllText(file);
                
                foreach (string prohibited in prohibitedStrings)
                    if (contents.Contains(prohibited))
                    {
                        problemFiles.Add(file, prohibited);
                        break;
                    }
            }

            foreach (var kvp in problemFiles)
                Console.WriteLine("FAIL: File '" + kvp.Key + "' contains a reference to an explicitly prohibited database name string ('" + kvp.Value + "')");

            Assert.AreEqual(0,problemFiles.Count);
        }
    }
}