using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace CatalogueLibraryTests.SourceCodeEvaluation.ClassFileEvaluation
{
    public class RDMPFormInitializationTests
    {
        readonly List<string> _rdmpFormClassNames = new List<string>();
        readonly List<string> _fails = new List<string>();

        List<string> methodBlacklist = new List<string>()
         {
             "if",
             "catch",
             "InitializeComponent"
         };

        //match anything on start of line followed by whitespace followed by a method name e.g. Fishfish( where capture group[1] is the method name
        Regex methodCalls = new Regex("^\\s*([A-Za-z0-9]*)\\s?\\(", RegexOptions.Multiline);
        Regex rdmpFormClasses = new Regex("class\\s+(.*)\\s*:\\s*RDMPForm");
        Regex rdmpControlClasses = new Regex("class\\s+(.*)\\s*:\\s*RDMPUserControl");
        
        public void FindUninitializedForms(List<string> csFiles )
        {
            foreach (string file in csFiles)
            {
                var readToEnd = File.ReadAllText(file);
                DealWithRDMPForms(readToEnd);
                DealWithRDMPUserControls(readToEnd);
            }
            
            List<string> rdmpFormClassNames = _rdmpFormClassNames;

            //look for "new (myclass1|myclass2)\s*\("
            StringBuilder sbFindInstantiations = new StringBuilder();
            sbFindInstantiations.Append("new (");
            sbFindInstantiations.Append(string.Join("|", rdmpFormClassNames.Select(Regex.Escape)));
            sbFindInstantiations.Append(")\\s*\\(");

            Regex matchConstructorUse = new Regex(sbFindInstantiations.ToString());

            foreach (string csFile in csFiles)
            {
                string readToEnd = File.ReadAllText(csFile);

                var matches = matchConstructorUse.Matches(readToEnd);
                    
                foreach (Match match in matches)
                {
                    int lineNumberOfMatch = readToEnd.Substring(0, match.Index).Count(c => c == '\n');
                    Console.WriteLine(csFile + " uses constructor of class " + match.Groups[1].Value + " (Line number:" + lineNumberOfMatch+")");
                        
                    string substring = readToEnd.Substring(match.Index);

                    var lines = substring.Split('\n');
                    if(!lines[1].Contains("RepositoryLocator"))
                    {
                        string msg = "FAIL: The next line after the constructor should be RepositoryLocator = X so that the RDMPForm is initialized properly but it isn't!";
                        Console.WriteLine(msg);
                        _fails.Add(msg);
                            
                    }
                }
            }
            

            if(_fails.Any())
                Assert.Fail("Fix the problems above (anything marked FAIL:) then Clean and Recompile the ENTIRE solution to ensure a fresh copy of SourceCodeForSelfAwareness.zip gets created and run the test again");
            
        }

        private void DealWithRDMPUserControls(string readToEnd)
        {
            var match = rdmpControlClasses.Match(readToEnd);
            if (match.Success)
            {
                string className = match.Groups[1].Value.Trim();
                Console.WriteLine("Class " + className + " is an RDMPUSerControl");

                ComplainIfUsesVisualStudioDesignerDetectionMagic(readToEnd);
                ComplainIfAccessesRepositoryLocatorInConstructor(readToEnd, className);

            }
        }

        private void DealWithRDMPForms(string readToEnd)
        {
            var match = rdmpFormClasses.Match(readToEnd);
            if (match.Success)
            {
                string className = match.Groups[1].Value.Trim();
                Console.WriteLine("Class " + className + " is an RDMPForm");

                _rdmpFormClassNames.Add(className);

                ComplainIfUsesVisualStudioDesignerDetectionMagic(readToEnd);
                ComplainIfAccessesRepositoryLocatorInConstructor(readToEnd, className);
            }
        }

        private void ComplainIfAccessesRepositoryLocatorInConstructor(string readToEnd, string className)
        {
            //find constructor
            Regex constructorRegex = GetConstructoRegex(className);

            var matchConstructor = constructorRegex.Match(readToEnd);
            if (matchConstructor.Success)
            {
                int curlyBracerCount = -1;
                int index = 0;

                string substring = readToEnd.Substring(matchConstructor.Index);

                StringBuilder sbConstructor = new StringBuilder();
                while (curlyBracerCount != 0 && index < substring.Length)
                {
                    if (substring[index] == '{')
                    {
                        if (curlyBracerCount == -1)
                            curlyBracerCount = 1; //first curly bracer
                        else
                            curlyBracerCount ++;
                    }

                    if (substring[index] == '}')
                        curlyBracerCount --;

                    sbConstructor.Append(substring[index]);

                    index++;
                }

                var constructor = sbConstructor.ToString();

                //find other method the constructor calls
                foreach (Match m in methodCalls.Matches(constructor))
                {
                    string methodName = m.Groups[1].Value;

                    if (methodBlacklist.Contains(methodName))
                        continue;

                    Console.WriteLine("Constructor calls method:" + m.Groups[1].Value);
                }


                if (constructor.Contains("RepositoryLocator") || constructor.Contains("_repositoryLocator"))
                    Assert.Fail("Constructor of class " + className +
                                " contains a reference to RepositoryLocator, this property cannot be used until OnLoad is called");
            }
            else
            {
                Console.WriteLine("Class " + className + " is an RDMPForm/RDMPUserControl but doesn't have a constructor!");
            }
        }

        private void ComplainIfUsesVisualStudioDesignerDetectionMagic(string readToEnd)
        {
            if (readToEnd.Contains("LicenseManager.UsageMode"))
            {
                int lineNumber = readToEnd.Substring(0, readToEnd.IndexOf("LicenseManager.UsageMode")).Count(c => c == '\n');

                string msg =
                    "FAIL: Use protected variable VisualStudioDesignMode instead of LicenseManager.UsageMode (line number:" +
                    lineNumber + ")";
                Console.WriteLine(msg);
                _fails.Add(msg);
            }
        }
        private Regex GetConstructoRegex(string className)
        {
            return new Regex("(public|private)\\s+" + className + "\\s*\\(.*\\{", RegexOptions.Singleline);
        }

    }
}
