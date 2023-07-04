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

namespace Rdmp.UI.Tests.DesignPatternTests.ClassFileEvaluation;

public class RDMPFormInitializationTests
{
    private readonly List<string> _rdmpFormClassNames = new();
    private readonly List<string> _fails = new();

    private List<string> methodIgnoreList = new()
    {
        "if",
        "catch",
        "InitializeComponent"
    };

    //match anything on start of line followed by whitespace followed by a method name e.g. Fishfish( where capture group[1] is the method name
    private Regex methodCalls = new("^\\s*([A-Za-z0-9]*)\\s?\\(", RegexOptions.Multiline);
    private Regex rdmpFormClasses = new("class\\s+(.*)\\s*:\\s*RDMPForm");
    private Regex rdmpControlClasses = new("class\\s+(.*)\\s*:\\s*RDMPUserControl");
        
    public void FindUninitializedForms(List<string> csFiles )
    {
        foreach (var readToEnd in csFiles.Select(File.ReadAllText))
        {
            DealWithRDMPForms(readToEnd);
            DealWithRDMPUserControls(readToEnd);
        }

        //look for "new (myclass1|myclass2)\s*\("
        var sbFindInstantiations = new StringBuilder("new (");
        sbFindInstantiations.AppendJoin("|", _rdmpFormClassNames.Select(Regex.Escape));
        sbFindInstantiations.Append(")\\s*\\(");

        if (_fails.Any())
            Assert.Fail(
                "Fix the problems above (anything marked FAIL:) then Clean and Recompile the ENTIRE solution to ensure a fresh copy of SourceCodeForSelfAwareness.zip gets created and run the test again");
    }

    private void DealWithRDMPUserControls(string readToEnd)
    {
        var match = rdmpControlClasses.Match(readToEnd);
        if (!match.Success) return;
        var className = match.Groups[1].Value.Trim();
        ComplainIfUsesVisualStudioDesignerDetectionMagic(readToEnd);
        ComplainIfAccessesRepositoryLocatorInConstructor(readToEnd, className);
    }

    private void DealWithRDMPForms(string readToEnd)
    {
        var match = rdmpFormClasses.Match(readToEnd);
        if (!match.Success) return;
        var className = match.Groups[1].Value.Trim();
        _rdmpFormClassNames.Add(className);
        ComplainIfUsesVisualStudioDesignerDetectionMagic(readToEnd);
        ComplainIfAccessesRepositoryLocatorInConstructor(readToEnd, className);
    }

    private static void ComplainIfAccessesRepositoryLocatorInConstructor(string readToEnd, string className)
    {
        //find constructor
        var constructorRegex = GetConstructorRegex(className);

        var matchConstructor = constructorRegex.Match(readToEnd);
        if (matchConstructor.Success)
        {
            var curlyBracerCount = -1;
            var index = 0;

            var substring = readToEnd[matchConstructor.Index..];

            var sbConstructor = new StringBuilder();
            while (curlyBracerCount != 0 && index < substring.Length)
            {
                if (substring[index] == '{')
                {
                    if (curlyBracerCount == -1)
                        curlyBracerCount = 1; //first curly bracer
                    else
                        curlyBracerCount++;
                }

                if (substring[index] == '}')
                    curlyBracerCount--;

                sbConstructor.Append(substring[index]);

                index++;
            }

            var constructor = sbConstructor.ToString();

            if (Regex.IsMatch(constructor, "[^.]RepositoryLocator") || constructor.Contains("_repositoryLocator"))
                Assert.Fail(
                    $"Constructor of class {className} contains a reference to RepositoryLocator, this property cannot be used until OnLoad is called");
        }
        else
        {
            Console.WriteLine($"Class {className} is an RDMPForm/RDMPUserControl but doesn't have a constructor!");
        }
    }

    private void ComplainIfUsesVisualStudioDesignerDetectionMagic(string readToEnd)
    {
        if (!readToEnd.Contains("LicenseManager.UsageMode")) return;
        var lineNumber = readToEnd[..readToEnd.IndexOf("LicenseManager.UsageMode", StringComparison.Ordinal)].Count(c => c == '\n');

        var msg =
            $"FAIL: Use protected variable VisualStudioDesignMode instead of LicenseManager.UsageMode (line number:{lineNumber})";
        Console.WriteLine(msg);
        _fails.Add(msg);
    }
    private static Regex GetConstructorRegex(string className) => new($"(public|private)\\s+{className}\\s*\\(.*\\{{", RegexOptions.Singleline);
}