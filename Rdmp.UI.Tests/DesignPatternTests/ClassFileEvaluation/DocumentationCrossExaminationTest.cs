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

namespace Rdmp.UI.Tests.DesignPatternTests.ClassFileEvaluation
{
    class DocumentationCrossExaminationTest
    {
        private readonly DirectoryInfo _slndir;
        Regex matchComments = new Regex(@"///[^;\r\n]*");

        private string[] _mdFiles;
        Regex matchMdReferences = new Regex(@"`(.*)`");

        public const bool ReWriteMarkdownToReferenceGlossary = true;

        //words that are in Pascal case and you can use in comments despite not being in the codebase... this is an ironic variable to be honest
        //since the very fact that you add something to _whitelist means that it is in the codebase after all!
        #region Whitelist Terms
        private string[] _whitelist = new []
        {
            "NormalCohorts",
            "FreakyCohorts",
            "PublicKeyToken",
            "DatabaseEntities",
            "MyDateCol",
            "NumberOfResults",
            "ANOGPCode",
            "ANOPatientIdentifier",
            "PracticeGP",
            "UNIONing",
            "PatientId",
            "PatientIds",
            "MotherId",
            "BabyId",
            "MyColumn",
            "DrugCode",
            "DrugCode_Desc",
            "DrugName",
            "LabNumber",
            "DrugName",
            "SendingLocation",
            "DischargeLocation",
            "SendingLocation_Desc",
            "DischargeLocation_Desc",
            "MyDb",
            "NewBiochemistry",
            "AdmissionDateTime",
            "DataAge",
            "PatientCareNumber",
            "MyDb",
            "MyTransform",
            "PatCHI",
            "PatientCHI",
            "MotherCHI",
            "FatherChiNo",
            "LabNumber",
            "ANOchi",
            "INFOs",
            "PluginDatabase",
            "PatientDateOfBirth",
            "PatientDateOfBirthApprox",
            "ANOLocation",
            "ConditionList",
            "DataAnalyst",
            "HumanReadableDrugName",
            "DrugPrescribed",
            "DrugAbuse",
            "RoutineLoaderAccount",
            "ReadonlyUserAccount",
            "HBA1c",
            "PrescribedDate",
            "NodaTime",
            "MotherCHI",
            "BabyCHI",
            "ANOIdentifier",
            "ANOLabNumber",
            "LabNumber",
            "EmailAddressOfAuthorizor",
            "GrandParent",
            "TestLabCode",
            "DataAge",
            "CatalogueItem1",
            "CatalogueItem2",
            "CatalogueItem1",
            "CatalogueItem2",
            "UPDATEd",
            "GUIDs",
            "MyColRenamed",
            "DateConsentedToStudy",
            "PrescribingGP",
            "CasesForProject123",
            "ControlsForProject123",
            "GUIDs",
            "UPPERd",
            "StackOverflow",
            "LabNumbers",
            "LabNumber",
            "ANOLabNumber",
            "PatientName",
            "PatientDob",
            "DataAge",
            "DemographyLoading",
            "MyDb",
            "MyTbl",
            "CapsName",
            "SummaryComment",
            "MySoftwareSuite",
            "MyApplication",
            "MyResources",
            "MyClass1",
            "FishFishFish",
            "CASCADEing",
            "MyAssembly",
            "HBA1c",
            "EnumNameFavourite",
            "EnumName",
            "CatalogueFavourite",
            "PatientDeleted",
            "MyDoc",
            "MyDataFiles",
            "MyServer",
            "MyData",
            "RemoteDataFetcher",
            "EndpointDefinition",
            "LoadID_Data_STAGING",
            "AuditObject",
            "EmailAddressOfAuthorizor",
            "FixedSource",
            "AverageResult",
            "MaxResult",
            "FormatFile",
            "IndexedExtractionIdentifierList_AggregateConfiguration5",
            "YAXLib",
            "DependsOn",
            "DependingOnThis",
            "ProposedFixes",
            "PropertyX",
            "FamilyMembers",
            
            //CreatingANewCollectionTreeNode.md
            "FolderOfX", 

            //PluginWriting.md
            "MyPluginUserInterface",
            "ExecuteCommandRenameCatalogueToBunnies",
            "BasicDataTableAnonymiser1",
            "BasicDataTableAnonymiser3",
            "CodeExamples",
            "MyExamplePluginTests",
            "TEST_Catalogue",
            "BasicDataTableAnonymiser4",
            "GetCommonNamesTable",
            "TestBasicDataTableAnonymiser3",
            "BasicDataTableAnonymiser5",
            "TestAnonymisationPlugins",
            "LoggerTestCase",
            "ToConsole",
            "ToMemory",
            "ToDatabase",
            "TEST_Logging",
            "DatePrescribed",

            //This is a legit verb
            "ANDed",
            "FetchBytesExpensive",

            "RecordLoadedDate",
            "TblPatIndx",
            "DataLoadRunId",

            "DataStructures", //class diagram
            "MyYetToExistTable",

            "SexCode",
            "SexCode_Desc",
            "SendingLocationCode",
            "LocationTable",
            "AddressLine1",
            "AddressLine2",
            "PatientSexCode",
            "SexDescription",
            "SexDescriptionLong",
            "MyTransform",
            
            "MyObject",
            "MyObjectMenu",
            "AllServersNodeMenu",
            "SomeClass",
            "ProposeExecutionWhenCommandIs",
            "Log4Net",
            "ReleaseLocation",
            "MyCoolDb",
            "OperationType",
            "DrugList",
            "DrugCodeFormat",
            "GenderCodeDescription",
            "MyParam",

            "GPCode",
            "RepoType",
            "LoadingBiochem",

            //Stuff now in FAnsi
            "MySqlAggregateHelper",
            "MicrosoftSQLAggregateHelper",
            "DbCommandBuilder",
            "EnvironmentPotential",
            "MySqlConnection",
            "DbCommandBuilder",
            "DbCommandBuilder",
            "IDecideTypesForStrings",
            "TypeCompatibilityGroup",
            "DatatypeComputerTests",
            "DataTypeRequest",
            "CrossPlatformTests",
            "IDecideTypesForStrings",
            "TypeCompatibilityGroup",
            "TypeTranslaterTests",
            "DatatypeComputerTests",
            "LoadRunID",
            "HelpDocs",
            "HicServices",
            "FAnsiSql",
            "ProposeExecutionWhenTargetIsX",
            "InternalsVisibleTo",
            "AxisDimension",
            "MyDataset1",
             "HicHash",
            "UserManual",
            "ObjectType",
            "UserInterfaceOverview",
            "MyPipelinePlugin",
            "TestAnonymisationPluginsDatabaseTests",
            "PDFs",
            "MyPatIndexTable",
            "MSBuild15CMD",
            "SetupLazy",
            "TestCaseSourceAttribute",
            "DescribeCommand", //aka ExecuteCommandDescribeCommand
            "MySqlConnector",
            "DoSomething",
            "MyTest",
        };
        #endregion
        public DocumentationCrossExaminationTest(DirectoryInfo slndir)
        {
            _slndir = slndir;
            _mdFiles = Directory.GetFiles(slndir.FullName, "*.md",SearchOption.AllDirectories);
        }

        public void FindProblems(List<string> csFilesFound)
        {
            //find all non coment code and extract all unique tokens

            //find all .md files and extract all `` code blocks

            //for each commend and `` code block

            //identify Pascal case words

            //are they in the codebase tokens?

            List<string> problems = new List<string>();

            HashSet<string> codeTokens = new HashSet<string>();
            Dictionary<string, HashSet<string>> fileCommentTokens = new Dictionary<string, HashSet<string>>();

            //find all comments in class files
            foreach (string file in csFilesFound)
            {
                bool isDesignerFile = file.Contains(".Designer.cs");
                
                if(file.Contains("CodeTutorials"))
                    continue;
                
                //don't look in the packages dir!
                if(file.Contains("packages"))
                    continue;

                foreach (string line in File.ReadAllLines(file))
                {
                    //if it is a comment
                    if (matchComments.IsMatch(line))
                    {
                        if (isDesignerFile)
                            continue;

                        if (!fileCommentTokens.ContainsKey(file))
                            fileCommentTokens.Add(file, new HashSet<string>());
                        
                        //its a comment extract all pascal case words
                        foreach (Match word in Regex.Matches(line, @"\b([A-Z]\w+){2,}"))
                            fileCommentTokens[file].Add(word.Value);
                    }
                    else
                    {
                        //else it is a code line, extract all tokens
                        foreach (Match word in Regex.Matches(line, @"\w+"))
                            codeTokens.Add(word.Value);
                    }
                }
            }

            //find all comments in .md tutorials
            foreach (string mdFile in _mdFiles)
            {
                //don't look in the packages dir!
                if(mdFile.Contains("packages"))
                    continue;

                fileCommentTokens.Add(mdFile,new HashSet<string>());
                var fileContents = File.ReadAllText(mdFile);
                
                foreach (Match m in matchMdReferences.Matches(fileContents))
                    foreach (Match word in Regex.Matches(m.Groups[1].Value, @"([A-Z]\w+){2,}"))
                        fileCommentTokens[mdFile].Add(word.Value);

                EnsureMaximumGlossaryUse(mdFile,problems);

                EnsureCodeBlocksCompile(mdFile, problems);
            }


            foreach (KeyValuePair<string, HashSet<string>> kvp in fileCommentTokens)
            {
                foreach (string s in kvp.Value)
                {
                    if(!codeTokens.Contains(s))
                    {
                        if (_whitelist.Contains(s))
                            continue;

                        //it's SHOUTY TEXT
                        if (s.ToUpper() == s)
                            continue;

                        //if it's a plural e.g. TableInfos then we are still ok if we find TableInfo
                        if (s.Length > 2 && s.EndsWith("s"))
                        {
                            if (codeTokens.Contains(s.Substring(0, s.Length - 1)))
                                continue;
                        }
                        
                        problems.Add("FATAL PROBLEM: File '" + kvp.Key +"' talks about something which isn't in the codebase, called a:" +Environment.NewLine + s);
                        
                    }
                }
            }

            if (problems.Any())
            {
                Console.WriteLine("Found problem words in comments (Scroll down to see by file then if you think they are fine add them to DocumentationCrossExaminationTest._whitelist):");
                foreach (var pLine in problems.Where(l=>l.Contains('\n')).Select(p => p.Split('\n')))
                    Console.WriteLine("\"" + pLine[1] + "\",");
                
            }

            foreach (string problem in problems)
                Console.WriteLine(problem);

            Assert.AreEqual(0,problems.Count,"Expected there to be nothing talked about in comments that doesn't appear in the codebase somewhere");
        }

        private void EnsureCodeBlocksCompile(string mdFile, List<string> problems)
        {
            string codeBlocks = Path.Combine(TestContext.CurrentContext.TestDirectory,"../../../DesignPatternTests/MarkdownCodeBlockTests.cs");

            Console.WriteLine("Starting " + mdFile);

            var codeBlocksContent = File.ReadAllText(codeBlocks);

            Regex rGuidComment = new Regex("<!--- (.{32}) --->");
            Regex rStartCodeBlock = new Regex("```csharp");
            Regex rEndCodeBlock = new Regex("```");

            Dictionary<string,string> markdownCodeBlocks = new Dictionary<string, string>();
            
            var lines = File.ReadAllLines(mdFile);

            for (int i = 0; i < lines.Length; i++)
            {
                var match = rGuidComment.Match(lines[i]);

                //match a line like <!--- df7d2bb4cd6145719f933f6f15218b1a --->
                if (match.Success)
                {
                    var guid = match.Groups[1].Value;
                    var sb = new StringBuilder();

                    markdownCodeBlocks.Add(guid,null);

                    //consume the line and look for ```csharp on the next line
                    if(!rStartCodeBlock.IsMatch(lines[++i]))
                        throw new Exception($"Expected code block in markdown for GUID {guid} to be followed by a line {rStartCodeBlock}");

                    //skip the ```csharp line
                    i++;

                    //consume until the closing ``` line
                    while (!rEndCodeBlock.IsMatch(lines[i]))
                        sb.AppendLine(lines[i++]);

                    markdownCodeBlocks[guid] = sb.ToString();
                }
            }

            foreach (var kvp in markdownCodeBlocks)
            {
                Regex rBlock = new Regex($"#region {kvp.Key}([^#]*)#endregion",RegexOptions.Singleline);
                var m = rBlock.Match(codeBlocksContent);

                if (!m.Success)
                    throw new Exception(
                        $"No code block found in {codeBlocks} for guid {kvp.Key}.  Try adding a #region section for the guid");

                var code = Regex.Replace(m.Groups[1].Value, "\\s+", " ");
                var docs = Regex.Replace(kvp.Value, "\\s+", " ");

                Assert.AreEqual(code.Trim(), docs.Trim(),        
                    $"Code in the documentation markdown (actual) did not match the corresponding compiled code (expected) for code guid {kvp.Key} markdown file was {mdFile} and code file was {codeBlocks}");

                Console.WriteLine("Validated markdown block " + kvp.Key);
            }
            
            Console.WriteLine("Validated " + markdownCodeBlocks.Count + " markdown blocks");
        }

        private void EnsureMaximumGlossaryUse(string mdFile, List<string> problems)
        {
            const string glossaryRelativePath = "./Documentation/CodeTutorials/Glossary.md";
            
            Regex rGlossary = new Regex("##([A-z ]*)");
            Regex rWords = new Regex(@"\[?\w*\]?");
            Regex rGlossaryLink = new Regex(@"^\[\w*\]:");

            var glossaryPath = Path.Combine(_slndir.FullName, glossaryRelativePath);
            
            //don't evaluate the glossary!
            if(Path.GetFileName(mdFile) == "Glossary.md")
                return;

            if(Path.GetFileName(mdFile) == "template.md")
                return;

            var glossaryHeaders = 
                new HashSet<string>(
                File.ReadAllLines(glossaryPath)
                .Where(l=>rGlossary.IsMatch(l))
                .Select(l=>rGlossary.Match(l).Groups[1].Value.Trim()));

            bool inCodeBlock = false;
            int lineNumber = 0;

            var allLines = File.ReadAllLines(mdFile);
            var allLinesRevised = allLines;

            Dictionary<string,string> suggestedLinks = new Dictionary<string, string>();

            foreach (string line in allLines)
            {
                lineNumber++;

                if(string.IsNullOrWhiteSpace(line))
                    continue;
                
                //don't complain about the glossary links at the bottom of the file.
                if(rGlossaryLink.IsMatch(line))
                    continue;

                //don't complain about keywords in code blocks
                if(line.TrimStart().StartsWith("```"))
                    inCodeBlock = !inCodeBlock;

                if (!inCodeBlock)
                {
                    foreach (Match match in rWords.Matches(line))
                    {
                        if (glossaryHeaders.Contains(match.Value))
                        {
                            //It's already got a link on it e.g. [DBMS] or it's "UNION - sometext"
                            if(match.Index - 1 > 0 
                               && 
                               (line[match.Index-1] == '[' || line[match.Index-1] == '"'))
                                continue;


                            Uri path1 = new Uri(mdFile);
                            Uri path2 = new Uri(glossaryPath);
                            Uri diff = path1.MakeRelativeUri(path2);
                            string relPath = diff.OriginalString;

                            if (!relPath.StartsWith("."))
                                relPath = "./" + relPath;

                            string suggestedLine = $"[{match.Value}]: {relPath}#{match.Value}";

                            //if it has spaces on either side
                            if(line[Math.Max(0,match.Index-1)] == ' ' && line[Math.Min(line.Length-1,match.Index + match.Length)] == ' '
                               //don't mess with lines that contain an image
                               && !line.Contains("!["))
                                allLinesRevised[lineNumber - 1] = line.Replace($" {match.Value} ", $" [{match.Value}] ");

                            //also if we have a name like `Catalogue` it should probably be [Catalogue] instead so it works as a link
                            allLinesRevised[lineNumber - 1] = line.Replace($"`{match.Value}`", $"[{match.Value}]");

                            //if it is a novel occurrence
                            if (!allLines.Contains(suggestedLine) && !suggestedLinks.ContainsValue(suggestedLine))
                            {
                                suggestedLinks.Add(match.Value,suggestedLine);
                                problems.Add($"Glossary term should be link in {mdFile} line number {lineNumber}.  Term is {match.Value}.  Suggested link line is:\"{suggestedLine}\"" );
                            }
                        }
                    }
                }
            }
            
            // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (suggestedLinks.Any() && ReWriteMarkdownToReferenceGlossary)
            {
                File.WriteAllLines(mdFile,allLinesRevised);

                File.AppendAllText(mdFile,Environment.NewLine);
                File.AppendAllLines(mdFile, suggestedLinks.Values.Distinct());
            }

        }
    }
}
