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

namespace Rdmp.UI.Tests.DesignPatternTests;

internal class CsProjFileTidy
{
    public List<string> csFilesFound = new();

    public List<string> UntidyMessages { get; set; }

    private string _expectedRootNamespace;
    private DirectoryInfo _root;

    //these class files are excused from having 2+ class files in them or 0
    private string[] Ignorelist =
    {
        "Attributes.cs", "AssemblyInfo.cs", "Annotations.cs", "StageArgs.cs", "ICustomUI.cs",
        "MapsDirectlyToDatabaseTableStatelessDefinition.cs",
        "IObjectUsedByOtherObjectNode.cs", "IInjectKnown.cs", "Themes.cs", "TableView.cs", "TreeView.cs",
        "MemoryCatalogueRepository.cs"
    };

    public CsProjFileTidy(FileInfo csProjFile)
    {
        UntidyMessages = new List<string>();

        _root = csProjFile.Directory;
        _expectedRootNamespace = csProjFile.Name.Replace(".csproj", "");

        //e.g. <Compile Include="Overview\OverviewScreen.Designer.cs"
        var allText = File.ReadAllText(csProjFile.FullName);

        if (allText.Contains("<CopyToOutputDirectory>Always</CopyToOutputDirectory>"))
            Console.WriteLine($"WARNING:Csproj '{csProjFile}' contains CopyAlways");

        var doc = new XmlDocument();
        doc.LoadXml(allText);

        //var compilables = doc.GetElementsByTagName("Compile");


        RecursivelyProcessSubfolders(csProjFile.Directory);
    }

    private void RecursivelyProcessSubfolders(DirectoryInfo directory)
    {
        foreach (var enumerateFile in directory.EnumerateFiles("*.cs"))
            ConfirmClassNameAndNamespaces(enumerateFile);

        foreach (var dir in directory.EnumerateDirectories())
        {
            if (dir.Name.Equals("bin") || dir.Name.Equals("obj"))
                continue;

            RecursivelyProcessSubfolders(dir);
        }
    }

    private void ConfirmClassNameAndNamespaces(FileInfo csFile)
    {
        if (Ignorelist.Contains(csFile.Name))
            return;

        csFilesFound.Add(csFile.FullName);

        var contents = File.ReadAllText(csFile.FullName);

        var rNamespace = new Regex(@"^namespace ([A-Za-z0-9.]*)", RegexOptions.Multiline);
        var rPublicClasses = new Regex(@"^\s*public (class|interface) ([A-Za-z0-9_]*)", RegexOptions.Multiline);

        var classes = rPublicClasses.Matches(contents);
        var namespaces = rNamespace.Matches(contents);

        if (namespaces.Count == 0)
        {
            UntidyMessages.Add($"FAIL: .cs file does not have any namespaces listed in it:{csFile.FullName}");
        }
        else if (namespaces.Count > 1)
        {
            UntidyMessages.Add($"FAIL: .cs file has more than 1 namespaces listed in it!:{csFile.FullName}");
        }
        else
        {
            var subspace = GetSubspace(csFile);
            var expectedNamespace = _expectedRootNamespace + subspace;

            if (expectedNamespace.StartsWith("rdmp"))
                expectedNamespace = Regex.Replace(expectedNamespace, "^rdmp", "Rdmp.Core");

            var actualNamespace = namespaces[0].Groups[1].Value;

            if (!actualNamespace.Equals(expectedNamespace))
                UntidyMessages.Add(
                    $"Expected file {csFile.FullName} to have namespace {expectedNamespace} but its listed namespace is {actualNamespace}");
        }


        //it's probably a enum or interface or delegates file
        if (classes.Count == 0)
            return;

        //we are ok with Microsoft doing whatever it wants in these files
        if (csFile.Name.Contains(".Designer.cs"))
            return;

        if (csFile.Name.Equals("MarkdownCodeBlockTests.cs"))
            return;

        if (classes.Count > 1)
        {
            //The only files allowed 2+ class files in them are tests and factories
            if (csFile.Name.Contains("Test") || csFile.Name.Contains("Factory"))
                return;

            UntidyMessages.Add($"FAIL: .cs file contains 2+ classes/interfaces {csFile.FullName}");
        }
        else
        {
            var firstClassNameInFile = classes[0].Groups[2].Value;

            if (firstClassNameInFile.Contains("_Design"))
                return;

            if (!csFile.Name.Equals($"{firstClassNameInFile}.cs", StringComparison.CurrentCultureIgnoreCase))
                UntidyMessages.Add(
                    $"File {csFile.FullName} contains a class is called {firstClassNameInFile} (does not match file name of file)");
        }
    }

    private string GetSubspace(FileInfo csFile)
    {
        var relative = csFile.FullName.Replace(_root.FullName, "");

        //trim off the "\myclass.cs" bit
        relative = relative[..^(csFile.Name.Length + 1)];
        return relative.Replace('\\', '.');
    }
}