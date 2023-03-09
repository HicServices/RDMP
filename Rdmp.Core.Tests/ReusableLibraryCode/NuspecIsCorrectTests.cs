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
using NUnit.Framework;

namespace Rdmp.Core.Tests.ReusableLibraryCode;

/// <summary>
/// Tests to confirm that the dependencies in csproj files (NuGet packages) match those in the .nuspec files and that packages.md 
/// lists the correct versions (in documentation)
/// </summary>
class NuspecIsCorrectTests
{
    private static readonly string[] Analyzers = new string[]{ "coverlet.collector", "SecurityCodeScan.VS2019" };
    //<PackageReference Include="NUnit3TestAdapter" Version="3.13.0" />
    private static readonly Regex RPackageRef = new(@"<PackageReference\s+Include=""(.*)""\s+Version=""([^""]*)""", RegexOptions.IgnoreCase|RegexOptions.Compiled|RegexOptions.CultureInvariant);

    //<dependency id="CsvHelper" version="12.1.2" />
    private static readonly Regex RDependencyRef = new(@"<dependency\s+id=""(.*)""\s+version=""([^""]*)""", RegexOptions.IgnoreCase|RegexOptions.Compiled|RegexOptions.CultureInvariant);
    private static readonly Dictionary<string, string> PathCache;

    static NuspecIsCorrectTests()
    {
        var solutionRoot = AppDomain.CurrentDomain.BaseDirectory;
        while (!Directory.EnumerateFiles(solutionRoot, "*.sln").Any())
            solutionRoot = Directory.GetParent(solutionRoot)?.FullName ?? throw new Exception($"No VS Solution file found above {AppDomain.CurrentDomain.BaseDirectory}");
        PathCache=Directory.EnumerateFiles(solutionRoot,"*.*", SearchOption.AllDirectories)
            .Where(p => p.EndsWith(".csproj") || p.EndsWith("Packages.md") || p.EndsWith(".nuspec"))
            .ToDictionary(Path.GetFileName, p => p);
    }

    //test dependencies should be in Plugin.Test.nuspec
    [TestCase("Tests.Common.csproj","Plugin.Test.nuspec","Packages.md")]
        
    //core dependencies should be in all nuspec files
    [TestCase("Rdmp.Core.csproj","Plugin.Test.nuspec","Packages.md")]
    [TestCase("Rdmp.Core.csproj","Plugin.nuspec","Packages.md")]
    [TestCase("Rdmp.Core.csproj","Plugin.UI.nuspec","Packages.md")]

    //ui dependencies should be in Plugin.UI.nuspec
    [TestCase("Rdmp.UI.csproj","Plugin.UI.nuspec","Packages.md")]
    public void TestDependencyCorrect( string csproj, string nuspec, string packagesMarkdown)
    {
        string nuspecContent=null;
        string[] packagesMarkdownContent=null;

        if (csproj != null)
            csproj = PathCache[csproj];
        if (nuspec != null)
            nuspec = PathCache[nuspec];
        if (packagesMarkdown != null)
            packagesMarkdown = PathCache[packagesMarkdown];

        if (!File.Exists(csproj))
            Assert.Fail("Could not find file {0}", csproj);
        if (nuspec != null && !File.Exists(nuspec))
            Assert.Fail("Could not find file {0}", nuspec);

        if (packagesMarkdown != null && !File.Exists(packagesMarkdown))
            Assert.Fail("Could not find file {0}", packagesMarkdown);

        if (nuspec !=null) nuspecContent = File.ReadAllText(nuspec);
        if (packagesMarkdown != null) packagesMarkdownContent = File.ReadAllLines(packagesMarkdown);

        //For each dependency listed in the csproj
        foreach (var p in RPackageRef.Matches(File.ReadAllText(csproj)).ToArray())
        {
            var package = p.Groups[1].Value;
            var version = p.Groups[2].Value;

            var found = false;

            // Not one we need to pass on to the package consumers
            if(package.Contains("Microsoft.NETFramework.ReferenceAssemblies.net461"))
                continue;

            //analyzers do not have to be listed as a dependency in nuspec (but we should document them in packages.md)
            if (!Analyzers.Contains(package) && nuspecContent != null)
            {
                //make sure it appears in the nuspec
                foreach (var d in RDependencyRef.Matches(nuspecContent).ToArray())
                {
                    var packageDependency = d.Groups[1].Value;
                    var versionDependency = d.Groups[2].Value;

                    if (!packageDependency.Equals(package)) continue;
                    Assert.AreEqual(version, versionDependency, "Package {0} is version {1} in {2} but version {3} in {4}", package, version, csproj, versionDependency, nuspec);
                    found = true;
                }

                if (!found)
                    Assert.Fail("Package {0} in {1} is not listed as a dependency of {2}. Recommended line is:\r\n{3}", package, csproj, nuspec,
                        BuildRecommendedDependencyLine(package, version));
            }


            // End early if not checking Markdown
            if (packagesMarkdown == null) continue;
            found = false;
            foreach (var count in from line in packagesMarkdownContent where Regex.IsMatch(line, $@"[\s[]{Regex.Escape(package)}[\s\]]", RegexOptions.IgnoreCase) select new Regex(Regex.Escape(version)).Matches(line).Count)
            {
                Assert.AreEqual(2, count, "Markdown file {0} did not contain 2 instances of the version {1} for package {2} in {3}", packagesMarkdown, version, package, csproj);
                found = true;
            }

            if (!found)
                Assert.Fail("Package {0} in {1} is not documented in {2}. Recommended line is:\r\n{3}", package, csproj, packagesMarkdown,
                    BuildRecommendedMarkdownLine(package, version));
        }
    }

    private static object BuildRecommendedDependencyLine(string package, string version) => $"<dependency id=\"{package}\" version=\"{version}\" />";

    private static object BuildRecommendedMarkdownLine(string package, string version) => $"| {package} | [GitHub]() | [{version}](https://www.nuget.org/packages/{package}/{version}) | | | |";
}