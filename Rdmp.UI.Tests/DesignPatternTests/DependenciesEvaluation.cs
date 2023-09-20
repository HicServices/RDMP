// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using NUnit.Framework;
using Rdmp.Core.ReusableLibraryCode.VisualStudioSolutionFileProcessing;

namespace Rdmp.UI.Tests.DesignPatternTests;

public class DependenciesEvaluation
{
    private string[] _nuspecFiles =
    {
        "Plugin/Plugin.Test/Plugin.Test.nuspec",
        "Plugin/Plugin/Plugin.nuspec",
        "Plugin/Plugin.UI/Plugin.UI.nuspec"
    };

    private Dictionary<string, string> Dependencies = new();

    public void FindProblems(VisualStudioSolutionFile sln)
    {
        var problems = new List<string>();

        foreach (var nuspecFile in _nuspecFiles)
        {
            var filePath = Path.Combine(sln.SolutionDirectory.FullName, nuspecFile);
            var text = File.ReadAllText(filePath);


            //<dependency id="jacobslusser.ScintillaNET" version="3.6.3"

            var r = new Regex(@"dependency id=([A-Za-z.0-9""]*)\s+version=([0-9.""]*)");

            foreach (Match match in r.Matches(text))
            {
                var assembly = match.Groups[1].Value;
                var version = match.Groups[2].Value;

                if (!Dependencies.ContainsKey(assembly))
                {
                    Dependencies.Add(assembly, version);
                }
                else
                {
                    if (!Equals(Dependencies[assembly], version))
                        throw new Exception($"nuspec files could not agree on standard version for {assembly}");
                }
            }
        }

        foreach (var project in sln.Projects)
        {
            var csproj = new FileInfo(Path.Combine(sln.SolutionDirectory.FullName, project.Path));

            var fappConfig = new FileInfo(Path.Combine(csproj.Directory.FullName, "app.config"));

            if (fappConfig.Exists)
                ProcessAppConfig(fappConfig, problems);

            var fExeConfig = new FileInfo(Path.Combine(csproj.Directory.FullName, "RDMPAutomationService.exe.config"));

            if (fExeConfig.Exists)
                ProcessAppConfig(fExeConfig, problems);

            var fappPackages = new FileInfo(Path.Combine(csproj.Directory.FullName, "packages.config"));
            if (fappPackages.Exists)
            {
                //look for dodgy packages
                //<package id="NUnit" version="2.6.4" />

                var dc = new XmlDocument();
                dc.Load(fappPackages.FullName);

                foreach (XmlElement dependency in dc.GetElementsByTagName("package"))
                {
                    var assembly = $"\"{dependency.Attributes["id"].Value}\"";
                    var version = $"\"{dependency.Attributes["version"].Value}\"";

                    if (Dependencies.ContainsKey(assembly))
                    {
                        if (!Equals(Dependencies[assembly], version))
                            problems.Add(
                                $"In package {fappPackages.FullName} you reference {assembly} with version {version} but your nuspec has version {Dependencies[assembly]}");
                    }
                    else
                    {
                        problems.Add(
                            $"In package {fappPackages.FullName} you reference {assembly}  (version {version}) but no corresponding dependency listed in any of your nuspec files.");
                    }
                }
            }

            var csprojFileContents = File.ReadAllText(csproj.FullName);

            //look for dodgy reference includes
            foreach (var (key, versionInNuspec) in Dependencies)
            {
                //Reference Include="MySql.Data, Version=8.0.12.0
                var r = new Regex($@"{key.Trim('"')}, Version=([0-9.""]*)");

                foreach (Match match in r.Matches(csprojFileContents))
                {
                    var versionInCsproj = match.Groups[1].Value;

                    if (!AreProbablyCompatibleVersions(versionInNuspec, versionInCsproj))
                        problems.Add(
                            $"csproj file {project.Name} lists dependency of {key} with version {versionInCsproj} while in the nuspec it is {versionInNuspec}");
                }
            }
        }

        foreach (var problem in problems)
            Console.WriteLine(problem);

        Assert.AreEqual(0, problems.Count);
    }

    private void ProcessAppConfig(FileInfo fappConfig, List<string> problems)
    {
        //look for dodgy binding redirects

        /*<assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-11.0.0.0" newVersion="11.0.2.0" />*
             */
        var dc = new XmlDocument();
        dc.Load(fappConfig.FullName);

        foreach (XmlElement dependency in dc.GetElementsByTagName("dependentAssembly"))
        {
            var assemblyIdentity =
                dependency.GetElementsByTagName("assemblyIdentity").OfType<XmlElement>().SingleOrDefault();
            var bindingRedirect =
                dependency.GetElementsByTagName("bindingRedirect").OfType<XmlElement>().SingleOrDefault();

            if (assemblyIdentity != null && bindingRedirect != null)
            {
                var version = $"\"{bindingRedirect.Attributes["newVersion"].Value}\"";
                var assembly = $"\"{assemblyIdentity.Attributes["name"].Value}\"";

                if (Dependencies.ContainsKey(assembly))
                {
                    if (!AreProbablyCompatibleVersions(Dependencies[assembly], version))
                        problems.Add(
                            $"You have a binding redirect in {fappConfig.FullName} for assembly {assembly} to version {version} but your nuspec has version {Dependencies[assembly]}");
                }
                else
                {
                    problems.Add(
                        $"You have a binding redirect in {fappConfig.FullName} for assembly {assembly} but no corresponding dependency listed in any of your nuspec files.  Why do you have binding redirects for assemblies that are not redistributed with RDMP?");
                }
            }
        }
    }

    /// <summary>
    /// Returns true if the assemblies are likely to be compatible
    /// </summary>
    /// <param name="availableVersion">The version available</param>
    /// <param name="requiredVersion">The version required, can include 0 elements for wildcards e.g. 11.0.0.0 would be compatible with 11.2.0.0</param>
    /// <returns></returns>
    private static bool AreProbablyCompatibleVersions(string availableVersion, string requiredVersion)
    {
        var v1 = new Version(availableVersion.Trim('"'));
        var v2 = new Version(requiredVersion.Trim('"'));

        //must be equal on first 3 numbers (Revision is allowed to differ)
        return
            v1.Major == v2.Major &&
            (v1.Minor == v2.Minor || v2.Minor == 0 || v2.Minor == -1) &&
            (v1.Build == v2.Build || v2.Build == 0 || v2.Build == -1) &&
            (v1.Revision == v2.Revision || v2.Revision == 0 || v2.Revision == -1);
    }
}