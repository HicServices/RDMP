using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using NUnit.Framework;
using ReusableLibraryCode.VisualStudioSolutionFileProcessing;

namespace CatalogueLibraryTests.SourceCodeEvaluation
{
    public class DependenciesEvaluation
    {
        private string[] _nuspecFiles =
        {
            "Plugin/Plugin.Test/Plugin.Test.nuspec",
            "Plugin/Plugin/Plugin.nuspec",
            "Plugin/Plugin.UI/Plugin.UI.nuspec"

        };
        
        Dictionary<string, string> Dependencies = new Dictionary<string, string>();

        public void FindProblems(VisualStudioSolutionFile sln)
        {
            List<string> problems = new List<string>();

            foreach (string nuspecFile in _nuspecFiles)
            {
                var filePath = Path.Combine(sln.SolutionDirectory.FullName, nuspecFile);
                var text = File.ReadAllText(filePath);


                //<dependency id="jacobslusser.ScintillaNET" version="3.6.3"

                Regex r = new Regex(@"dependency id=([A-Za-z.0-9""]*)\s+version=([0-9.""]*)");

                foreach (Match match in r.Matches(text))
                {
                    var assembly = match.Groups[1].Value;
                    var version = match.Groups[2].Value;

                    if(!Dependencies.ContainsKey(assembly))
                        Dependencies.Add(assembly,version);
                    else
                    {
                        if (!Equals(Dependencies[assembly], version))
                            throw new Exception("nuspec files could not agree on standard version for " + assembly);
                    }
                }
            }

            foreach (var project in sln.Projects)
            { 
                FileInfo csproj = new FileInfo(Path.Combine(sln.SolutionDirectory.FullName,project.Path));
                
                FileInfo fappConfig = new FileInfo(Path.Combine(csproj.Directory.FullName, "app.config"));

                if (fappConfig.Exists)
                {
                    //look for dodgy binding redirects

                    /*<assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
                <bindingRedirect oldVersion="0.0.0.0-11.0.0.0" newVersion="11.0.2.0" />*
                     */
                    XmlDocument dc = new XmlDocument();
                    dc.Load(fappConfig.FullName);

                    foreach (XmlElement dependency in dc.GetElementsByTagName("dependentAssembly"))
                    {
                        var assemblyIdentity = dependency.GetElementsByTagName("assemblyIdentity").OfType<XmlElement>().SingleOrDefault();
                        var bindingRedirect = dependency.GetElementsByTagName("bindingRedirect").OfType<XmlElement>().SingleOrDefault();

                        if (assemblyIdentity != null && bindingRedirect != null)
                        {
                            var version ='"' + bindingRedirect.Attributes["newVersion"].Value + '"';
                            var assembly = '"' + assemblyIdentity.Attributes["name"].Value + '"';

                            if(Dependencies.ContainsKey(assembly))
                            {
                                if (!AreProbablyCompatibleVersions(Dependencies[assembly], version))
                                    problems.Add("You have a binding redirect in " + fappConfig.FullName  + " for assembly " + assembly + " to version " + version + " but your nuspec has version " + Dependencies[assembly]);
                            }
                            else
                            {
                                problems.Add("You have a binding redirect in "+fappConfig.FullName+" for assembly " + assembly + " but no corresponding dependency listed in any of your nuspec files.  Why do you have binding redirects for assemblies that are not redistributed with RDMP?");
                            }
                        }
                    }
                }

                FileInfo fappPackages = new FileInfo(Path.Combine(csproj.Directory.FullName, "packages.config"));
                if (fappPackages.Exists)
                {
                    //look for dodgy packages
                    //<package id="NUnit" version="2.6.4" />

                    XmlDocument dc = new XmlDocument();
                    dc.Load(fappPackages.FullName);

                    foreach (XmlElement dependency in dc.GetElementsByTagName("package"))
                    {
                        var assembly = '"' + dependency.Attributes["id"].Value + '"';
                        var version = '"' + dependency.Attributes["version"].Value + '"';
                            
                        if (Dependencies.ContainsKey(assembly))
                        {
                            if (!Equals(Dependencies[assembly], version))
                                problems.Add("In package " + fappPackages.FullName + " you reference " + assembly + " with version " + version + " but your nuspec has version " + Dependencies[assembly]);
                        }
                        else
                        {
                            problems.Add("In package " + fappPackages.FullName + " you reference "  + assembly + "  (version "+version+") but no corresponding dependency listed in any of your nuspec files.");
                        }
                        
                    }

                }

                



                var csprojFileContexnts = File.ReadAllText(csproj.FullName);
                
                //look for dodgy reference includes
                foreach (KeyValuePair<string, string> dependency in Dependencies)
                {
                    //Reference Include="MySql.Data, Version=8.0.12.0
                    Regex r = new Regex(dependency.Key.Trim('"') + @", Version=([0-9.""]*)");
                    
                    foreach (Match match in r.Matches(csprojFileContexnts))
                    {
                        var versionInCsproj = match.Groups[1].Value;
                        var versionInNuspec = dependency.Value;

                        if (!AreProbablyCompatibleVersions(versionInNuspec, versionInCsproj))
                            problems.Add("csproj file " + project.Name + " lists dependency of " + dependency.Key + " with version " + versionInCsproj + " while in the nuspec it is " + versionInNuspec);
                    }
                }
            }

            foreach (var problem in problems)
                Console.WriteLine(problem);

            Assert.AreEqual(0,problems.Count);

        }

        /// <summary>
        /// Returns true if the assemblies are likely to be compatible
        /// </summary>
        /// <param name="availableVersion">The version available</param>
        /// <param name="requiredVersion">The version required, can include 0 elements for wildcards e.g. 11.0.0.0 would be compatible with 11.2.0.0</param>
        /// <returns></returns>
        private bool AreProbablyCompatibleVersions(string availableVersion, string requiredVersion)
        {
            var v1 = new Version(availableVersion.Trim('"'));
            var v2 = new Version(requiredVersion.Trim('"'));

            //must be equal on first 3 numbers (Revision is allowed to differ)
            return
                v1.Major == v2.Major&&
                (v1.Minor == v2.Minor || v2.Minor == 0 || v2.Minor == -1)&&
                (v1.Build == v2.Build || v2.Build == 0 || v2.Build == -1) &&
                (v1.Revision == v2.Revision || v2.Revision == 0 || v2.Revision == -1);
        }
    }
}