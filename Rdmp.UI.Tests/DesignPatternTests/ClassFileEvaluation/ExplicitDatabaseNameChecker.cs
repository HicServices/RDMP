// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Rdmp.UI.Tests.DesignPatternTests.ClassFileEvaluation;

public class ExplicitDatabaseNameChecker
{
    public static void FindProblems(List<string> csFilesFound)
    {
        var problemFiles = new Dictionary<string, string>();
        var prohibitedStrings = new List<string>();

        var ignoreList = new List<string>
        {
            "ExplicitDatabaseNameChecker.cs", //us obviously since we do contain that text!
            "DatabaseCreationProgramOptions.cs", //allowed because it is the usage text for the program.
            "AutomationServiceOptions.cs", //allowed because it is the usage text for the program.
            "DatabaseTests.cs", //allowed because it is telling user about how you can setup database tests support
            "ChoosePlatformDatabasesUI.Designer.cs", //allowed because it is a suggestion to user about what prefix to use
            "PluginPackagerProgramOptions.cs", //allwed because it's a suggestion to the user about command line arguments
            "DocumentationCrossExaminationTest.cs", //allowed because its basically a list of comments that are allowed despite not appearing in the codebase
            "ResearchDataManagementPlatformOptions.cs", //allowed because it's an Example
            "AWSS3BucketReleaseDestination.cs" //allowed as it uses it as a temp file identifier
        };


        ignoreList.AddRange(
            new string[]
            {
                "DleOptions.cs",
                "CacheOptions.cs",
                "RDMPCommandLineOptions.cs",
                "Settings.Designer.cs",
                "PlatformDatabaseCreationOptions.cs",
                "PackOptions.cs",
                "PasswordEncryptionKeyLocation.cs",
                "ToLoggingDatabaseDataLoadEventListener.cs",
            }); //allowed because it's default arguments for CLI

        prohibitedStrings.Add("TEST_");
        prohibitedStrings.Add("RDMP_");

        foreach (var file in csFilesFound)
        {
            if (ignoreList.Any(str => str.Equals(Path.GetFileName(file))))
                continue;

            var contents = File.ReadAllText(file);

            foreach (var prohibited in prohibitedStrings.Where(contents.Contains))
            {
                problemFiles.Add(file, prohibited);
                break;
            }
        }

        foreach (var kvp in problemFiles)
            Console.WriteLine(
                $"FAIL: File '{kvp.Key}' contains a reference to an explicitly prohibited database name string ('{kvp.Value}')");

        Assert.That(problemFiles, Is.Empty);
    }
}