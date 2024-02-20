// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Rdmp.Core.Tests.ReusableCodeTests;

/// <summary>
/// Tests to confirm that the dependencies in csproj files (NuGet packages) match those in the .nuspec files and that packages.md
/// lists the correct versions (in documentation)
/// </summary>
public class PackageListIsCorrectTests
{
    private static readonly EnumerationOptions EnumerationOptions = new()
        { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive, IgnoreInaccessible = true };

    //<PackageReference Include="NUnit3TestAdapter" Version="3.13.0" />
    private static readonly Regex RPackageRefNoVersion =
        new(@"<PackageReference\s+Include=""(.*)""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);


    // | Org.SomePackage |
    //
    private static readonly Regex RMarkdownEntry = new(@"^\|\s*\[?([^ |\]]+)(\]\([^)]+\))?\s*\|",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);


    /// <summary>
    /// Enumerate non-test packages, check that they are listed in PACKAGES.md
    /// </summary>
    /// <param name="rootPath"></param>
    [TestCase]
    public void TestPackagesDocumentCorrect(string rootPath = null)
    {
        var root = FindRoot(rootPath);
        var undocumented = new StringBuilder();

        // Extract the named packages from PACKAGES.md
        var packagesMarkdown = File.ReadAllLines(GetPackagesMarkdown(root))
            .Select(line => RMarkdownEntry.Match(line))
            .Where(m => m.Success)
            .Skip(2) // Jump over the header
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

        // Extract the named packages from csproj files
        var usedPackages = GetCsprojFiles(root).Select(File.ReadAllText)
          .SelectMany(s => RPackageRefNoVersion.Matches(s))
            .Select(m => m.Groups[1].Value)
              .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

        // Then subtract those listed in PACKAGES.md (should be empty)
        var undocumentedPackages = usedPackages.Except(packagesMarkdown).Select(BuildRecommendedMarkdownLine).ToList();
        undocumented.AppendJoin(Environment.NewLine, undocumentedPackages);

        var unusedPackages = packagesMarkdown.Except(usedPackages).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(unusedPackages, Is.Empty,
                    $"The following packages are listed in PACKAGES.md but are not used in any csproj file: {string.Join(", ", unusedPackages)}");
            Assert.That(undocumentedPackages, Is.Empty,
                $"One or more packages not documented in PACKAGES.md. Recommended addition:{Environment.NewLine}{undocumented}");
        });
    }

    /// <summary>
    /// Generate the report entry for an undocumented package
    /// </summary>
    /// <param name="package"></param>
    /// <returns></returns>
    private static object BuildRecommendedMarkdownLine(string package) =>
        $"| {package} | [GitHub]() | LICENCE GOES HERE | |";

    /// <summary>
    /// Find the root of this repo, which is usually the directory containing the .sln file
    /// If the .sln file lives elsewhere, you can override this by passing in a path explicitly.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static DirectoryInfo FindRoot(string path = null)
    {
        if (path != null)
        {
            if (!Path.IsPathRooted(path)) path = Path.Combine(TestContext.CurrentContext.TestDirectory, path);
            return new DirectoryInfo(path);
        }

        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (!root.EnumerateFiles("*.sln", SearchOption.TopDirectoryOnly).Any() && root.Parent != null)
            root = root.Parent;
        Assert.That(root.Parent, Is.Not.Null, "Could not find root of repository");
        return root;
    }

    /// <summary>
    /// Returns all csproj files in the repository, except those containing the string 'tests'
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    private static IEnumerable<string> GetCsprojFiles(DirectoryInfo root)
    {
        return root.EnumerateFiles("*.csproj", EnumerationOptions).Select(f => f.FullName)
            .Where(f => !f.Contains("tests", StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Find the sole packages.md file wherever in the repo it lives. Error if multiple or none.
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    private static string GetPackagesMarkdown(DirectoryInfo root)
    {
        var path = root.EnumerateFiles("packages.md", EnumerationOptions).Select(f => f.FullName).SingleOrDefault();
        Assert.That(path, Is.Not.Null, "Could not find packages.md");
        return path;
    }
}