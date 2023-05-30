// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Rdmp.UI.Tests.DesignPatternTests.ClassFileEvaluation;

public class SuspiciousEmptyChecksMethodsOrNotICheckablePlugins
{

    List<string> _fails = new List<string>();

    public void FindProblems(List<string> csFilesFound)
    {
        const string checkMethodSignature = @"void Check(ICheckNotifier notifier)";
        var testPattern = new Regex(@"(\b|[a-z])Test(\b|[A-Z])");

        foreach (var file in csFilesFound)
        {
            if (testPattern.IsMatch(file))
                continue;

            var contents = File.ReadAllText(file);

            if (!contents.Contains("[DemandsInitialization(\"") && !contents.Contains("[DemandsNestedInitialization(\""))
                continue;

            Console.WriteLine($"Found Demander:{file}");

            var index = contents.IndexOf(checkMethodSignature);

            if(index == -1)
            {
                _fails.Add(
                    $"FAIL:File {file} does not have a Check method implementation but contains the text [DemandsInitialization");
                continue;
            }

            var curlyBracerCount = -1;
            var sbChecksMethodBody = new StringBuilder();
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

            Console.WriteLine($"Demander Check Method Is:{Environment.NewLine}{methodBody}");

            if (!methodBody.Contains(";"))
                _fails.Add($"FAIL:Method body of Checks in file {file} is empty (does not contain any semicolons)");
        }

        foreach (var fail in _fails)
            Console.WriteLine(fail);

        Assert.AreEqual(0, _fails.Count);
    }
}