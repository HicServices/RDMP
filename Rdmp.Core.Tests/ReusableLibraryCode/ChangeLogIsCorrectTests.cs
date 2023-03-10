// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Rdmp.Core.Tests.ReusableLibraryCode;

class ChangeLogIsCorrectTests
{
    [TestCase("CHANGELOG.md")]
    public void TestChangeLogContents(string changeLogPath)
    {
        if (changeLogPath != null)
            changeLogPath = NuspecIsCorrectTests.PathCache[changeLogPath];

        if (!File.Exists(changeLogPath))
            Assert.Fail($"Could not find file {changeLogPath}");

        var fi = new FileInfo(changeLogPath);

        var assemblyInfo = Path.Combine(fi.Directory.FullName, "SharedAssemblyInfo.cs");

        if (!File.Exists(assemblyInfo))
            Assert.Fail($"Could not find file {assemblyInfo}");

        var match = Regex.Match(File.ReadAllText(assemblyInfo), @"AssemblyInformationalVersion\(""(.*)""\)");
        Assert.IsTrue(match.Success, $"Could not find AssemblyInformationalVersion tag in {assemblyInfo}");

        var currentVersion = match.Groups[1].Value;

        // When looking for the header in the change logs don't worry about -rc1 -rc2 etc
        if (currentVersion.Contains('-')) currentVersion = currentVersion[..currentVersion.IndexOf('-')];

        var changeLog = File.ReadLines(changeLogPath);
        Assert.IsTrue(changeLog.Any(l=>l.StartsWith($"## [{currentVersion}]",StringComparison.InvariantCulture)), $"{changeLogPath} did not contain a header for the current version '{currentVersion}'");
    }
}