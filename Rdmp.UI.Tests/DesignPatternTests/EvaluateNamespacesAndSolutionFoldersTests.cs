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
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.VisualStudioSolutionFileProcessing;
using Rdmp.UI.Tests.DesignPatternTests.ClassFileEvaluation;
using Tests.Common;

namespace Rdmp.UI.Tests.DesignPatternTests;

public class EvaluateNamespacesAndSolutionFoldersTests : DatabaseTests
{
    private const string SolutionName = "HIC.DataManagementPlatform.sln";
    private readonly List<string> _csFilesFound = new();

    public static readonly HashSet<string> IgnoreList = new()
    {
        "Program.cs",
        "Settings.Designer.cs",
        "Class1.cs",
        "Images.Designer.cs",
        "ToolTips.Designer.cs",
        "Resources.Designer.cs",
        "ProjectInstaller.cs",
        "ProjectInstaller.Designer.cs",
        "TableView.cs",
        "TreeView.cs",
        "JiraDataset.cs",
        "JiraAPIObjects.cs",
        "HDRDataset.cs",
        "HDRDatasetPatch.cs"
    };

    [Test]
    public void EvaluateNamespacesAndSolutionFolders()
    {
        var solutionDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (solutionDir?.GetFiles("*.sln").Any() != true) solutionDir = solutionDir?.Parent;
        Assert.That(solutionDir, Is.Not.Null, $"Failed to find {SolutionName} in any parent directories");

        var sln = new VisualStudioSolutionFile(solutionDir, solutionDir.GetFiles(SolutionName).Single());

        ProcessFolderRecursive(sln.RootFolders, solutionDir);

        foreach (var rootLevelProjects in sln.RootProjects)
            FindProjectInFolder(rootLevelProjects, solutionDir);

        var foundProjects = sln.Projects.ToDictionary(project => project, project => new List<string>());

        FindUnreferencedProjectsRecursively(foundProjects, solutionDir);

        foreach (var kvp in foundProjects)
            switch (kvp.Value.Count)
            {
                case 0:
                    Error(
                        $"FAIL: Did not find project {kvp.Key.Name} while traversing solution directories and subdirectories");
                    break;
                case > 1:
                    Error(
                        $"FAIL: Found 2+ copies of project {kvp.Key.Name} while traversing solution directories and subdirectories:{Environment.NewLine}{string.Join(Environment.NewLine, kvp.Value)}");
                    break;
            }

        Assert.That(_errors, Is.Empty);
        //      DependenciesEvaluation dependencies = new DependenciesEvaluation();
        //      dependencies.FindProblems(sln);

        InterfaceDeclarationsCorrect.FindProblems();

        var documented = new AllImportantClassesDocumented();
        documented.FindProblems(_csFilesFound);

        var uiStandardisationTest = new UserInterfaceStandardisationChecker();
        uiStandardisationTest.FindProblems(_csFilesFound);

        var crossExamination = new DocumentationCrossExaminationTest(solutionDir);
        crossExamination.FindProblems(_csFilesFound);

        //Assuming all files are present and correct we can now evaluate the RDMP specific stuff:
        var otherTestRunner = new RDMPFormInitializationTests();
        otherTestRunner.FindUninitializedForms(_csFilesFound);

        var propertyChecker = new SuspiciousRelationshipPropertyUse();
        propertyChecker.FindPropertyMisuse(_csFilesFound);

        ExplicitDatabaseNameChecker.FindProblems(_csFilesFound);

        var noMappingToDatabaseComments = new AutoCommentsEvaluator();
        AutoCommentsEvaluator.FindProblems(_csFilesFound);

        CopyrightHeaderEvaluator.FindProblems(_csFilesFound);

        //foreach (var file in slndir.EnumerateFiles("*.cs", SearchOption.AllDirectories))
        //{

        //    if (file.Name.StartsWith("AssemblyInfo") || file.Name.StartsWith("TemporaryGenerated") || file.Name.EndsWith("Designer.cs"))
        //        continue;

        //    var line = File.ReadLines(file.FullName).FirstOrDefault();
        //    if (line != null && line.StartsWith("// Copyright"))
        //        continue;


        //    Console.WriteLine(file.FullName);
        //}
    }

    private void FindUnreferencedProjectsRecursively(Dictionary<VisualStudioProjectReference, List<string>> projects,
        DirectoryInfo dir)
    {
        var projFiles = dir.EnumerateFiles("*.csproj");

        foreach (var projFile in projFiles)
        {
            if (projFile.Directory.FullName.Contains("CodeTutorials"))
                continue;

            var key = projects.Keys.SingleOrDefault(p => (p.Name + ".csproj").Equals(projFile.Name));
            if (key == null)
                Error($"FAIL:Unreferenced csproj file spotted :{projFile.FullName}");
            else
                projects[key].Add(projFile.FullName);
        }

        foreach (var subdir in dir.EnumerateDirectories())
            FindUnreferencedProjectsRecursively(projects, subdir);
    }

    private void ProcessFolderRecursive(IEnumerable<VisualStudioSolutionFolder> folders,
        DirectoryInfo currentPhysicalDirectory)
    {
        //Process root folders
        foreach (var solutionFolder in folders)
        {
            var physicalSolutionFolder = currentPhysicalDirectory.EnumerateDirectories()
                .SingleOrDefault(d => d.Name.Equals(solutionFolder.Name));

            if (physicalSolutionFolder == null)
            {
                Error(
                    $"FAIL: Solution Folder exists called {solutionFolder.Name} but there is no corresponding physical folder in {currentPhysicalDirectory.FullName}");
                continue;
            }

            foreach (var p in solutionFolder.ChildrenProjects)
                FindProjectInFolder(p, physicalSolutionFolder);

            if (solutionFolder.ChildrenFolders.Any())
                ProcessFolderRecursive(solutionFolder.ChildrenFolders, physicalSolutionFolder);
        }
    }

    private void FindProjectInFolder(VisualStudioProjectReference p, DirectoryInfo physicalSolutionFolder)
    {
        var physicalProjectFolder =
            physicalSolutionFolder.EnumerateDirectories().SingleOrDefault(f => f.Name.Equals(p.Name));

        if (physicalProjectFolder == null)
        {
            Error($"FAIL: Physical folder {p.Name} does not exist in directory {physicalSolutionFolder.FullName}");
        }
        else
        {
            var csProjFile = physicalProjectFolder.EnumerateFiles("*.csproj").SingleOrDefault(f => f.Name.Equals(
                $"{p.Name}.csproj"));
            if (csProjFile == null)
            {
                Error(
                    $"FAIL: .csproj file {p.Name}.csproj was not found in folder {physicalProjectFolder.FullName}");
            }
            else
            {
                var tidy = new CsProjFileTidy(csProjFile);

                foreach (var str in tidy.UntidyMessages)
                    Error(str);

                foreach (var found in tidy.csFilesFound
                             .Where(found => _csFilesFound.Any(otherFile =>
                                 Path.GetFileName(otherFile).Equals(Path.GetFileName(found)))).Where(found =>
                                 !IgnoreList.Contains(Path.GetFileName(found))))
                    Error($"Found 2+ files called {Path.GetFileName(found)}");

                _csFilesFound.AddRange(tidy.csFilesFound);
            }
        }
    }

    private readonly List<string> _errors = new();

    private void Error(string s)
    {
        Console.WriteLine(s);
        _errors.Add(s);
    }
}

public class CopyrightHeaderEvaluator
{
    public static void FindProblems(List<string> csFilesFound)
    {
        var suggestedNewFileContents = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        foreach (var file in csFilesFound)
        {
            if (file.Contains(".Designer.cs") || EvaluateNamespacesAndSolutionFoldersTests.IgnoreList.Contains(file))
                continue;

            var changes = false;

            var sbSuggestedText = new StringBuilder();

            var text = File.ReadLines(file).First();

            if (!text.StartsWith("// Copyright (c) The University of Dundee 2018-20")
                && text !=
                @"// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications")
            {
                changes = true;
                sbSuggestedText.AppendLine(@"// Copyright (c) The University of Dundee 2018-2023");
                sbSuggestedText.AppendLine(@"// This file is part of the Research Data Management Platform (RDMP).");
                sbSuggestedText.AppendLine(
                    @"// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.");
                sbSuggestedText.AppendLine(
                    @"// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.");
                sbSuggestedText.AppendLine(
                    @"// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.");
                sbSuggestedText.AppendLine();
                sbSuggestedText.AppendJoin(Environment.NewLine, text);
            }

            if (changes)
                suggestedNewFileContents.Add(file, sbSuggestedText.ToString());
        }

        Assert.That(suggestedNewFileContents, Is.Empty, $"The following files did not contain copyright:{Environment.NewLine}{string.Join(Environment.NewLine, suggestedNewFileContents.Keys.Select(Path.GetFileName))}");

        //drag your debugger stack pointer to here to mess up all your files to match the suggestedNewFileContents :)
        foreach (var suggestedNewFileContent in suggestedNewFileContents)
            File.WriteAllText(suggestedNewFileContent.Key, suggestedNewFileContent.Value);
    }
}

public partial class AutoCommentsEvaluator
{
    public static void FindProblems(List<string> csFilesFound)
    {
        var suggestedNewFileContents = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        foreach (var f in csFilesFound)
        {
            if (f.Contains(".Designer.cs"))
                continue;

            var changes = false;

            var sbSuggestedText = new StringBuilder();

            var text = File.ReadAllLines(f);
            var areInSummary = false;
            var paraOpened = false;

            for (var i = 0; i < text.Length; i++)
            {
                //////////////////////////////////No Mapping Properties////////////////////////////////////////////////////
                if (text[i].Trim().Equals("[NoMappingToDatabase]"))
                {
                    var currentClassName = GetUniqueTypeName(Path.GetFileNameWithoutExtension(f));

                    var t = MEF.GetType(currentClassName);

                    //if the previous line isn't a summary comment
                    if (!text[i - 1].Trim().StartsWith("///"))
                    {
                        var next = text[i + 1];

                        var m = PublicRegex().Match(next);
                        if (m.Success)
                        {
                            var whitespace = m.Groups[1].Value;
                            var member = m.Groups[3].Value;

                            Assert.Multiple(() =>
                            {
                                Assert.That(string.IsNullOrWhiteSpace(whitespace));
                                Assert.That(t, Is.Not.Null);
                            });

                            if (t.GetProperty($"{member}_ID") != null)
                            {
                                changes = true;
                                sbSuggestedText.AppendLine(whitespace + $"/// <inheritdoc cref=\"{member}_ID\"/>");
                            }
                            else
                            {
                                sbSuggestedText.AppendLine(text[i]);
                                continue;
                            }
                        }
                    }
                }


                if (text[i].Trim().Equals("/// <summary>"))
                {
                    areInSummary = true;
                    paraOpened = false;
                }

                if (text[i].Trim().Equals("/// </summary>"))
                {
                    if (paraOpened)
                    {
                        //
                        sbSuggestedText.Insert(sbSuggestedText.Length - 2, "</para>");
                        paraOpened = false;
                    }

                    areInSummary = false;
                }

                //if we have a paragraph break in the summary comments and the next line isn't an end summary
                if (areInSummary && text[i].Trim().Equals("///") && !text[i + 1].Trim().Equals("/// </summary>"))
                {
                    if (paraOpened)
                    {
                        sbSuggestedText.Insert(sbSuggestedText.Length - 2, "</para>");
                        paraOpened = false;
                    }

                    //there should be a para tag
                    if (!text[i + 1].Contains("<para>") && text[i + 1].Contains("///"))
                    {
                        changes = true;

                        //add current line
                        sbSuggestedText.AppendLine(text[i]);

                        //add the para tag
                        var nextLine = text[i + 1].Insert(text[i + 1].IndexOf("///", StringComparison.Ordinal) + 4,
                            "<para>");
                        sbSuggestedText.AppendLine(nextLine);
                        i++;
                        paraOpened = true;
                        continue;
                    }
                }

                sbSuggestedText.AppendLine(text[i]);
            }

            if (changes)
                suggestedNewFileContents.Add(f, sbSuggestedText.ToString());
        }


        //drag your debugger stack pointer to here to mess up all your files to match the suggestedNewFileContents :)
        if (suggestedNewFileContents.Count == 0)
            Assert.Pass();
        else
        {
            Console.WriteLine($"Replacing file contents in {string.Join(";", suggestedNewFileContents.Keys)}");
        }
        foreach (var suggestedNewFileContent in suggestedNewFileContents)
            File.WriteAllText(suggestedNewFileContent.Key, suggestedNewFileContent.Value);

        Assert.That(suggestedNewFileContents, Is.Empty);
    }

    private static string GetUniqueTypeName(string typename)
    {
        return typename switch
        {
            "ColumnInfo" => "Rdmp.Core.Curation.Data.ColumnInfo",
            "IFilter" => "Rdmp.Core.Curation.Data.IFilter",
            _ => typename
        };
    }

    [GeneratedRegex("(.*)public\\b(.*)\\s+(.*)\\b")]
    private static partial Regex PublicRegex();
}