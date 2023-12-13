// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Rdmp.Core.Tests.ReusableCodeTests;

internal class ChangeLogIsCorrectTests
{
    [Test]
    public void TestChangeLogContents()
    {
        var opts = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        var log = dir.GetFiles("changelog.md", opts).SingleOrDefault();
        while (log == null && dir.Parent != null)
        {
            dir = dir.Parent;
            log = dir.GetFiles("changelog.md", opts).SingleOrDefault();
        }

        Assert.That(log, Is.Not.Null, "CHANGELOG.md not found");

        var assemblyInfo = Path.Combine(log.Directory.FullName, "SharedAssemblyInfo.cs");

        if (!File.Exists(assemblyInfo))
            Assert.Fail($"Could not find file {assemblyInfo}");

        var match = Regex.Match(File.ReadAllText(assemblyInfo), @"AssemblyInformationalVersion\(""([^-]+).*""\)");
        Assert.That(match.Success, $"Could not find AssemblyInformationalVersion tag in {assemblyInfo}");

        var currentVersion = match.Groups[1].Value;

        Assert.That(File.ReadLines(log.FullName).Any(l => l.Contains($"## [{currentVersion}]")),
            $"{log.FullName} did not contain a header for the current version '{currentVersion}'");
    }
}