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

namespace Rdmp.UI.Tests.DesignPatternTests.ClassFileEvaluation;

public class AllImportantClassesDocumented
{
    private List<string> _csFilesList;
    private List<string> problems = new List<string>();
    private int commentedCount = 0;
    private int commentLineCount = 0;
    private bool strict = false;

    private string[] excusedClassFileNames =
    {
        "Class1.cs",
        "Program.cs",
        "PluginNugetClass.cs",
        "PluginTest.cs",
        "PluginUI.cs",

        //todo resolve the following:
        "ReleasePipeline.cs", //needs refactoring
        "ReleaseUseCase.cs",//needs refactoring
        "FixedDataReleaseSource.cs",//needs refactoring
        "CacheFetchRequestProvider.cs", //why do we need this class?
            
        "SharedObjectImporter.cs", //deprecated by the anonymisation object sharing framework?
        "Relationship.cs",//deprecated by the anonymisation object sharing framework?
        "RelationshipMap.cs"//deprecated by the anonymisation object sharing framework?

    };

    public void FindProblems(List<string> csFilesList)
    {
        _csFilesList = csFilesList;

        foreach (var f in _csFilesList)
        {
            if(excusedClassFileNames.Contains(Path.GetFileName(f)))
                continue;

            var text = File.ReadAllText(f);

            var startAt = text.IndexOf("public class");
            if(startAt == -1)
                startAt = text.IndexOf("public interface");

            if (startAt != -1)
            {
                var beforeDeclaration = text[..startAt];

                var mNamespace = Regex.Match(beforeDeclaration, "namespace (.*)");

                if(!mNamespace.Success)
                    Assert.Fail($"No namespace found in class file {f}");//no namespace in class!

                var nameSpace = mNamespace.Groups[1].Value;

                //skip tests
                if (nameSpace.Contains("Tests"))
                    continue;

                if (nameSpace.Contains("TestData.Relational"))//this has never been tested / used
                    continue;

                if (nameSpace.Contains("CohortManagerLibrary.FreeText"))//this has never been tested / used
                    continue;

                if (nameSpace.Contains("CatalogueWebService"))//this has never been tested / used
                    continue;

                if (nameSpace.Contains("CommitAssemblyEmptyAssembly"))
                    continue;

                var match = Regex.Match(beforeDeclaration, "<summary>(.*)</summary>", RegexOptions.Singleline);
                var matchInherit = Regex.Match(beforeDeclaration, "<inheritdoc", RegexOptions.Singleline);

                //are there comments?
                if (!match.Success && !matchInherit.Success)
                {
                    //no!
                    if (!strict) //are we being strict?
                    {
                        //User interface namespaces/related classes
                        if (nameSpace.Contains("Nodes"))
                            continue;
                        if (nameSpace.Contains("CommandExecution"))
                            continue;
                            
                        if (nameSpace.Contains("Copying"))
                            continue;
                        if (nameSpace.Contains("Icons"))
                            continue;

                        if (nameSpace.Contains("Diagnostics"))
                            continue;

                        if (nameSpace.Contains("Dashboard"))
                            continue;

                        if (nameSpace.Contains("MapsDirectlyToDatabaseTableUI"))
                            continue;
                            
                        //Provider specific implementations of stuff that is documented at interface level
                        if (nameSpace.Contains(".Discovery.Microsoft") ||nameSpace.Contains(".Discovery.Oracle") ||nameSpace.Contains(".Discovery.MySql"))
                            continue;
                    }

                    var idxLastSlash = f.LastIndexOf("\\");

                    if(idxLastSlash != -1)
                        problems.Add(string.Format("FAIL UNDOCUMENTED CLASS:{0} ({1})", 
                            f[(f.LastIndexOf("\\") + 1)..],
                            f[..idxLastSlash])
                        );
                    else
                        problems.Add($"FAIL UNDOCUMENTED CLASS:{f}");
                }
                else
                {
                    var lines = match.Groups[1].Value.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).Count();
                    commentLineCount += lines;
                    commentedCount++;
                }
            }
        }
            
        foreach (var fail in problems)
            Console.WriteLine(fail);

        Console.WriteLine($"Total Documented Classes:{commentedCount}");
        Console.WriteLine($"Total Lines of Classes Documentation:{commentLineCount}");
            
        Assert.AreEqual(0, problems.Count);
    }
}