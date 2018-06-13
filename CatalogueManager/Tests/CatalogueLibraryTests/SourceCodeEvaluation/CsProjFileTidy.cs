using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace CatalogueLibraryTests.SourceCodeEvaluation
{
    internal class CsProjFileTidy
    {
        public List<string>  csFilesFound = new List<string>();

        private Dictionary<string, int> _expectedToSee = new Dictionary<string,int>(StringComparer.InvariantCultureIgnoreCase);

        public List<string> UntidyMessages { get; set; }

        private string _expectedRootNamespace;
        private DirectoryInfo _root;

        //these class files are excused from having 2+ class files in them or 0
        private string[] Whitelist = new[] { "Attributes.cs", "AssemblyInfo.cs", "Annotations.cs", "StageArgs.cs" ,"ICustomUI.cs","MapsDirectlyToDatabaseTableStatelessDefinition.cs"};

        public CsProjFileTidy(FileInfo csProjFile)
        {
            UntidyMessages = new List<string>();

            _root = csProjFile.Directory;
            _expectedRootNamespace = csProjFile.Name.Replace(".csproj","");

            //e.g. <Compile Include="Overview\OverviewScreen.Designer.cs"
            string allText = File.ReadAllText(csProjFile.FullName);

            if (allText.Contains("<CopyToOutputDirectory>Always</CopyToOutputDirectory>"))
                Console.WriteLine("WARNING:Csproj '" + csProjFile + "' contains CopyAlways");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(allText);

            var compilables = doc.GetElementsByTagName("Compile");
            bool foundSharedAssemblyInfo = false;

            foreach (XmlNode compilable in compilables)
            {

                bool isLinked = false;

                if(compilable.HasChildNodes)
                    foreach (XmlNode childNode in compilable.ChildNodes)
                        if (childNode.Name == "Link")
                        {
                            if (childNode.InnerText.Contains("SharedAssemblyInfo.cs"))
                                foundSharedAssemblyInfo = true;

                            isLinked = true;
                        }

                if(isLinked)
                    continue;//it is a virtual file specified by a soft link
                
                if (compilable.Attributes != null && compilable.Attributes["Include"] != null)
                    _expectedToSee.Add(Path.Combine(csProjFile.Directory.FullName, compilable.Attributes["Include"].Value), 0);
            }

            if (!foundSharedAssemblyInfo)
                UntidyMessages.Add("Did not find a SharedAssemblyInfo.cs reference in project " + csProjFile.Name);

            RecursivelyProcessSubfolders(csProjFile.Directory);

            foreach (KeyValuePair<string, int> kvp in _expectedToSee.Where(kvp => kvp.Value == 0))
                UntidyMessages.Add("Did not find .cs file in subdirectories of the .csproj file:" + kvp.Key);
        }

        private void RecursivelyProcessSubfolders(DirectoryInfo directory)
        {
            foreach (FileInfo enumerateFile in directory.EnumerateFiles("*.cs"))
            {
                if (_expectedToSee.ContainsKey(enumerateFile.FullName))
                {

                    _expectedToSee[enumerateFile.FullName]++;
                    ConfirmClassNameAndNamespaces(enumerateFile);
                }
                else
                    UntidyMessages.Add("FAIL: Unexpected .cs file found that is not referenced by project:" + enumerateFile.FullName);
            }

            foreach (var dir in directory.EnumerateDirectories())
            {
                if(dir.Name.Equals("bin") || dir.Name.Equals("obj"))
                    continue;

                RecursivelyProcessSubfolders(dir);
            }
        }
        
        private void ConfirmClassNameAndNamespaces(FileInfo csFile)
        {
            if(Whitelist.Contains(csFile.Name))
                return;

            csFilesFound.Add(csFile.FullName);

            string contents = File.ReadAllText(csFile.FullName);

            Regex rNamespace = new Regex(@"^namespace ([A-Za-z0-9.]*)", RegexOptions.Multiline);
            Regex rPublicClasses = new Regex(@"^\s*public (class|interface) ([A-Za-z0-9_]*)", RegexOptions.Multiline);

            var classes = rPublicClasses.Matches(contents);
            var namespaces = rNamespace.Matches(contents);

            if(namespaces.Count == 0)
                UntidyMessages.Add("FAIL: .cs file does not have any namespaces listed in it:" + csFile.FullName);
            else if (namespaces.Count > 1)
                UntidyMessages.Add("FAIL: .cs file has more than 1 namespaces listed in it!:" + csFile.FullName);
            else
            {
                string subspace = GetSubspace(csFile);
                string expectedNamespace = _expectedRootNamespace + subspace;

                string actualNamespace = namespaces[0].Groups[1].Value;

                if(!actualNamespace.Equals(expectedNamespace))
                    UntidyMessages.Add("Expected file " + csFile.FullName + " to have namespace " + expectedNamespace + " but it's listed namespace is " + actualNamespace);

            }
            

            //it's probably a enum or interface or delegates file
            if(classes.Count ==0)
                return;

            //we are ok with Microsoft doing whatever it wants in these files
            if(csFile.Name.Contains(".Designer.cs"))
                return;

            if(classes.Count > 1)
            {
                //The only files allowed 2+ class files in them are tests and factories
                if (csFile.Name.Contains("Test") || csFile.Name.Contains("Factory"))
                    return;
                
                UntidyMessages.Add("FAIL: .cs file contains 2+ classes/interfaces " + csFile.FullName);
            }
            else
            {
                
                var firstClassNameInFile = classes[0].Groups[2].Value;

                if (firstClassNameInFile.Contains("_Design"))
                    return;

                if (!csFile.Name.Equals(firstClassNameInFile + ".cs",StringComparison.CurrentCultureIgnoreCase))
                    UntidyMessages.Add("File " + csFile.FullName + " contains a class is called " + firstClassNameInFile + " (does not match file name of file)");
            }

        }

        private string GetSubspace(FileInfo csFile)
        {
            string reltive = csFile.FullName.Replace(_root.FullName, "");

            //trim off the "\myclass.cs" bit
            reltive = reltive.Substring(0, reltive.Length - (csFile.Name.Length + 1));
            return reltive.Replace('\\', '.');
        }
    }
}