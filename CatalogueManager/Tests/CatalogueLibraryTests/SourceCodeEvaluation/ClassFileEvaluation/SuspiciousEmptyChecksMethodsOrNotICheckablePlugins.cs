using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace CatalogueLibraryTests.SourceCodeEvaluation.ClassFileEvaluation
{
    public class SuspiciousEmptyChecksMethodsOrNotICheckablePlugins
    {

        List<string> _fails = new List<string>();

        public void FindProblems(List<string> csFilesFound)
        {
            const string checkMethodSignature = @"void Check(ICheckNotifier notifier)";
            Regex testPattern = new Regex(@"(\b|[a-z])Test(\b|[A-Z])");

            foreach (string file in csFilesFound)
            {
                if (testPattern.IsMatch(file))
                    continue;

                var contents = File.ReadAllText(file);

                if (!contents.Contains("[DemandsInitialization(\"") && !contents.Contains("[DemandsNestedInitialization(\""))
                    continue;

                Console.WriteLine("Found Demander:" + file);

                int index = contents.IndexOf(checkMethodSignature);

                if(index == -1)
                {
                    _fails.Add("FAIL:File " + file + " does not have a Check method implementation");
                    continue;
                }

                int curlyBracerCount = -1;
                StringBuilder sbChecksMethodBody = new StringBuilder();
                while (curlyBracerCount != 0 && index < contents.Length)
                {
                    if (contents[index] == '{')
                    {
                        if (curlyBracerCount == -1)
                            curlyBracerCount = 1; //first curly bracer
                        else
                            curlyBracerCount++;
                    }

                    if (contents[index] == '}')
                        curlyBracerCount--;

                    sbChecksMethodBody.Append(contents[index]);

                    index++;
                }

                var methodBody = sbChecksMethodBody.ToString();

                Console.WriteLine("Demander Check Method Is:" + Environment.NewLine + methodBody);

                if (!methodBody.Contains(';'))
                    _fails.Add("FAIL:Method body of Checks in file " + file + " is empty (does not contain any semicolons)");
            }

            foreach (string fail in _fails)
                Console.WriteLine(fail);

            Assert.AreEqual(0, _fails.Count);
        }
    }
}