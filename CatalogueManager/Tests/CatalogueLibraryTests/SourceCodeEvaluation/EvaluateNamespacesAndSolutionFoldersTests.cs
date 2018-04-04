using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CatalogueLibrary.Reports;
using CatalogueLibrary.Repositories;
using CatalogueLibraryTests.SourceCodeEvaluation.ClassFileEvaluation;
using CatalogueManager.SimpleDialogs.Reports;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.VisualStudioSolutionFileProcessing;
using Tests.Common;


namespace CatalogueLibraryTests.SourceCodeEvaluation
{
    public class EvaluateNamespacesAndSolutionFoldersTests : DatabaseTests
    {
        private const string SolutionName = "HIC.DataManagementPlatform.sln";
        public List<string> csFilesFound = new List<string>();

        private string[] PermissableDuplicates = new[]
        {
            "Program.cs",
            "Settings.Designer.cs",
            "Class1.cs",
            "Images.Designer.cs",
            "ToolTips.Designer.cs",
            "Resources.Designer.cs",
            "ProjectInstaller.cs",
            "ProjectInstaller.Designer.cs"
        };

        [Test]
        public void EvaluateNamespacesAndSolutionFolders()
        {
            var slndir = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory);

            while (slndir != null)
            {
                if (slndir.GetFiles().Any(f => f.Name.Equals(SolutionName)))
                    break;

                Console.WriteLine("Looking for solution folder in directory " + slndir.FullName);

                slndir = slndir.Parent;
            }
            Assert.IsNotNull(slndir, "Failed to find " + SolutionName + " in any parent directories");

            Console.WriteLine("Found solution folder in directory:" + slndir.FullName);

            var sln = new VisualStudioSolutionFile(slndir.GetFiles().Single(f => f.Name.Equals(SolutionName)));

            ProcessFolderRecursive(sln.RootFolders, slndir);

            foreach (VisualStudioProjectReference rootLevelProjects in sln.RootProjects)
                FindProjectInFolder(rootLevelProjects, slndir);

            var foundProjects = new Dictionary<VisualStudioProjectReference, List<string>>();

            foreach (VisualStudioProjectReference project in sln.Projects)
                foundProjects.Add(project, new List<string>());

            FindUnreferencedProjectsRescursively(foundProjects, slndir);

            foreach (KeyValuePair<VisualStudioProjectReference, List<string>> kvp in foundProjects)
            {
                if (kvp.Value.Count == 0)
                    Error("FAIL: Did not find project " + kvp.Key.Name + " while traversing solution directories and subdirectories");

                if (kvp.Value.Count > 1)
                    Error("FAIL: Found 2+ copies of project " + kvp.Key.Name + " while traversing solution directories and subdirectories:" + Environment.NewLine + string.Join(Environment.NewLine, kvp.Value));
            }

            Assert.AreEqual(0, errors.Count);

            InterfaceDeclarationsCorrect interfaces = new InterfaceDeclarationsCorrect();
            interfaces.FindProblems(CatalogueRepository.MEF);

            AllImportantClassesDocumented documented = new AllImportantClassesDocumented();
            documented.FindProblems(csFilesFound);

            var crossExamination = new DocumentationCrossExaminationTest(slndir);
            crossExamination.FindProblems(csFilesFound);

            var uiStandardisationTest = new UserInterfaceStandardisationChecker();
            uiStandardisationTest.FindProblems(csFilesFound, RepositoryLocator.CatalogueRepository.MEF);

            //Assuming all files are present and correct we can now evaluate the RDMP specific stuff:
            var otherTestRunner = new RDMPFormInitializationTests();
            otherTestRunner.FindUninitializedForms(csFilesFound);

            var propertyChecker = new SuspiciousRelationshipPropertyUse(CatalogueRepository.MEF);
            propertyChecker.FindPropertyMisuse(csFilesFound);

            var weakChecks = new SuspiciousEmptyChecksMethodsOrNotICheckablePlugins();
            weakChecks.FindProblems(csFilesFound);

            var explicitDatabaseNamesChecker = new ExplicitDatabaseNameChecker();
            explicitDatabaseNamesChecker.FindProblems(csFilesFound);

            var singlePropertyUIs = new SinglePropertyUISourceCodeEvaluator();
            singlePropertyUIs.FindProblems(csFilesFound);

            var noMappingToDatabaseComments = new AutoCommentsEvaluator();
            noMappingToDatabaseComments.FindProblems(CatalogueRepository.MEF, csFilesFound);

        }

        private void FindUnreferencedProjectsRescursively(Dictionary<VisualStudioProjectReference, List<string>> projects, DirectoryInfo dir)
        {
            foreach (var subdir in dir.EnumerateDirectories())
                FindUnreferencedProjectsRescursively(projects, subdir);

            var projFiles = dir.EnumerateFiles("*.csproj");

            foreach (FileInfo projFile in projFiles)
            {
                if (projFile.Directory.FullName.Contains("CodeTutorials"))
                    continue;

                var key = projects.Keys.SingleOrDefault(p => (p.Name + ".csproj").Equals(projFile.Name));
                if (key == null)
                    Error("FAIL:Unreferenced csproj file spotted :" + projFile.FullName);
                else
                    projects[key].Add(projFile.FullName);
            }
        }

        private void ProcessFolderRecursive(IEnumerable<VisualStudioSolutionFolder> folders, DirectoryInfo currentPhysicalDirectory)
        {

            //Process root folders
            foreach (VisualStudioSolutionFolder solutionFolder in folders)
            {
                var physicalSolutionFolder = currentPhysicalDirectory.EnumerateDirectories().SingleOrDefault(d => d.Name.Equals(solutionFolder.Name));

                if (physicalSolutionFolder == null)
                {
                    Error("FAIL: Solution Folder exists called " + solutionFolder.Name + " but there is no corresponding physical folder in " + currentPhysicalDirectory.FullName);
                    continue;
                }

                foreach (VisualStudioProjectReference p in solutionFolder.ChildrenProjects)
                    FindProjectInFolder(p, physicalSolutionFolder);

                if (solutionFolder.ChildrenFolders.Any())
                    ProcessFolderRecursive(solutionFolder.ChildrenFolders, physicalSolutionFolder);
            }
        }

        private void FindProjectInFolder(VisualStudioProjectReference p, DirectoryInfo physicalSolutionFolder)
        {
            var physicalProjectFolder = physicalSolutionFolder.EnumerateDirectories().SingleOrDefault(f => f.Name.Equals(p.Name));

            if (physicalProjectFolder == null)
                Error("FAIL: Physical folder " + p.Name + " does not exist in directory " + physicalSolutionFolder.FullName);
            else
            {
                var csProjFile = physicalProjectFolder.EnumerateFiles("*.csproj").SingleOrDefault(f => f.Name.Equals(p.Name + ".csproj"));
                if (csProjFile == null)
                    Error("FAIL: .csproj file " + p.Name + ".csproj" + " was not found in folder " + physicalProjectFolder.FullName);
                else
                {
                    var tidy = new CsProjFileTidy(csProjFile);

                    foreach (string str in tidy.UntidyMessages)
                        Error(str);

                    foreach (var found in tidy.csFilesFound)
                        if (csFilesFound.Any(otherFile => Path.GetFileName(otherFile).Equals(Path.GetFileName(found))))
                            if (!PermissableDuplicates.Contains(Path.GetFileName(found)))
                                Error("Found 2+ files called " + Path.GetFileName(found));

                    csFilesFound.AddRange(tidy.csFilesFound);
                }
            }
        }

        List<string> errors = new List<string>();
        private void Error(string s)
        {
            Console.WriteLine(s);
            errors.Add(s);
        }
    }

    public class AutoCommentsEvaluator
    {
        public void FindProblems(MEF mef, List<string> csFilesFound)
        {
            Dictionary<string, string> suggestedNewFileContents = new Dictionary<string, string>();

            foreach (var f in csFilesFound)
            {
                if(f.Contains(".Designer.cs"))
                    continue;

                bool changes = false;

                StringBuilder sbSuggestedText = new StringBuilder();

                var text = File.ReadAllLines(f);
                bool areInSummary = false;
                bool paraOpened = false;

                for (int i = 0; i < text.Length; i++)
                {

                    //////////////////////////////////MAX LENGTH FIELDS////////////////////////////////////////////////////
                    Regex rMaxLength = new Regex(@"(\s+)public static int .*_MaxLength(\s?)=(\s?)-1;");

                    Match mMaxLength = rMaxLength.Match(text[i]);
                    if(mMaxLength.Success)
                    {
                        //if previous line didn't have an inherit doc
                        if (!text[i - 1].Trim().StartsWith("///"))
                        {
                            var whitespace = mMaxLength.Groups[1].Value;
                            sbSuggestedText.AppendLine(whitespace + @"///<inheritdoc cref=""IRepository.FigureOutMaxLengths""/>");
                            changes = true;
                        }
                    }
                    
                    //////////////////////////////////No Mapping Properties////////////////////////////////////////////////////
                    if (text[i].Trim().Equals("[NoMappingToDatabase]"))
                    {
                        var currentClassName = GetUniqueTypeName(Path.GetFileNameWithoutExtension(f));

                        Type t = mef.GetTypeByNameFromAnyLoadedAssembly(currentClassName);

                        //if the previous line isn't a summary comment
                        if (!text[i - 1].Trim().StartsWith("///"))
                        {
                            string next = text[i + 1];

                            var m = Regex.Match(next, @"(.*)public\b(.*)\s+(.*)\b");
                            if (m.Success)
                            {
                                
                                var whitespace = m.Groups[1].Value;
                                var type = m.Groups[2].Value;
                                var member = m.Groups[3].Value;

                                Assert.IsTrue(string.IsNullOrWhiteSpace(whitespace));
                                Assert.IsNotNull(t);

                                if (t.GetProperty(member + "_ID") != null)
                                {
                                    changes = true;
                                    sbSuggestedText.AppendLine(whitespace + string.Format("/// <inheritdoc cref=\"{0}\"/>",member + "_ID"));
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
                    if (areInSummary && text[i].Trim().Equals("///") && !text[i+1].Trim().Equals("/// </summary>"))
                    {
                        if(paraOpened)
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
                            string nextLine = text[i + 1].Insert(text[i+1].IndexOf("///")+4,"<para>");
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

            Assert.IsEmpty(suggestedNewFileContents);

            //drag your debugger stack pointer to here to mess up all your files to match the suggestedNewFileContents :)
            foreach (KeyValuePair<string, string> suggestedNewFileContent in suggestedNewFileContents)
                File.WriteAllText(suggestedNewFileContent.Key, suggestedNewFileContent.Value);
        }

        private string GetUniqueTypeName(string typename)
        {
            switch (typename)
            {
                case "ColumnInfo": return "CatalogueLibrary.Data.ColumnInfo";
                case "IFilter": return "CatalogueLibrary.Data.IFilter";
            }

            return typename;
        }
    }
}