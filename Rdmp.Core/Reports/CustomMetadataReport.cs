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
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataQualityEngine;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Reports
{
    /// <summary>
    /// Create a custom report e.g. markdown, xml etc by taking a template file and replicating it with replacements for each <see cref="Catalogue"/> property
    /// </summary>
    public class CustomMetadataReport
    {
        /// <summary>
        /// Substitutions that are used during template value replacement e.g. $Name => Catalogue.Name
        /// </summary>

        Dictionary<string,Func<Catalogue,object>> Replacements = new Dictionary<string, Func<Catalogue,object>>();

        /// <summary>
        /// Substitutions that are used during template value replacement when inside a '$foreach CatalogueItem' block e.g. $Name => CatalogueItem.Name
        /// </summary>

        Dictionary<string,Func<CatalogueItem,object>> ReplacementsCatalogueItem = new Dictionary<string, Func<CatalogueItem,object>>();

        
        /// <summary>
        /// Control line that begins looping Catalogues
        /// </summary>
        public const string LoopCatalogues = "$foreach Catalogue";

        /// <summary>
        /// Control line that begins looping CatalogueItems of a Catalogue
        /// </summary>
        public const string LoopCatalogueItems = "$foreach CatalogueItem";

        /// <summary>
        /// Ends looping
        /// </summary>
        public const string EndLoop = "$end";

        /// <summary>
        /// How the range of data in each <see cref="Catalogue"/> is determined, defaults to <see cref="DatasetTimespanCalculator"/> (using the DQE results)
        /// </summary>
        public IDetermineDatasetTimespan TimespanCalculator { get; set; }  = new DatasetTimespanCalculator();

        /// <summary>
        /// Specify a replacement for newlines when found in fields e.g. with space.  Leave as null to leave newlines intact.
        /// </summary>
        public string NewlineSubstitution { get; internal set; }
        
        /// <summary>
        /// The repository where column completeness metrics will come from.  Note this is usually the same source as <see cref="TimespanCalculator"/> 
        /// </summary>
        public DQERepository DQERepository { get; set; }

        Dictionary<ICatalogue, Evaluation> evaluationCache = new Dictionary<ICatalogue, Evaluation>();

        public CustomMetadataReport(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            // Catalogue level properties  (works in root or $foreach Catalogue block)

            //add basic properties
            foreach (var prop in typeof(Catalogue).GetProperties())
                Replacements.Add("$" + prop.Name, (c) => prop.GetValue(c));

            //add basic properties TableInfo
            foreach (var prop in typeof(TableInfo).GetProperties())
            {
                // if it's not already a property of Catalogue
                Replacements.TryAdd("$" + prop.Name, (c) => GetTable(c) == null ? null : prop.GetValue(GetTable(c)));
            }

            AddDQEReplacements();

            // Catalogue Item level properties (only work in a $foreach CatalogueItem block)

            //add basic properties CatalogueItem
            foreach (var prop in typeof(CatalogueItem).GetProperties())
                ReplacementsCatalogueItem.Add("$" + prop.Name, (s) => prop.GetValue(s));

            //add basic properties ColumnInfo
            foreach (var prop in typeof(ColumnInfo).GetProperties())
            {
                // if it's not already a property of CatalogueItem
                ReplacementsCatalogueItem.TryAdd("$" + prop.Name, (s) => s.ColumnInfo_ID == null ? null : prop.GetValue(s.ColumnInfo));
            }

            try
            {
                DQERepository = new DQERepository(repositoryLocator.CatalogueRepository);
            }
            catch (NotSupportedException)
            {
                DQERepository = null;
            }

            
        }

        private ITableInfo GetTable(Catalogue c)
        {
            return c.GetTableInfosIdeallyJustFromMainTables().OrderBy(t => t.IsPrimaryExtractionTable).FirstOrDefault();
        }

        private void AddDQEReplacements()
        {
            // Catalogue level shortcuts
            Replacements.Add("$DQE_StartDate",
                (c) => GetStartDate(c)?.ToString());
            Replacements.Add("$DQE_EndDate",
                (c) => GetEndDate(c)?.ToString());
            Replacements.Add("$DQE_DateRange",
                (c) => TimespanCalculator?.GetHumanReadableTimepsanIfKnownOf(c, true, out _));

            Replacements.Add("$DQE_StartYear",
                (c) => GetStartDate(c)?.ToString("yyyy"));
            Replacements.Add("$DQE_StartMonth",
                (c) => GetStartDate(c)?.ToString("MM"));
            Replacements.Add("$DQE_StartDay",
                (c) => GetStartDate(c)?.ToString("dd"));

            Replacements.Add("$DQE_EndYear",
                (c) => GetEndDate(c)?.ToString("yyyy"));
            Replacements.Add("$DQE_EndMonth",
                (c) => GetEndDate(c)?.ToString("MM"));
            Replacements.Add("$DQE_EndDay",
                (c) => GetEndDate(c)?.ToString("dd"));

            ReplacementsCatalogueItem.Add("$DQE_PercentNull",
                (ci) => GetPercentNull(ci));
        }

        private double? GetPercentNull(CatalogueItem ci)
        {
            var cata = ci.Catalogue;
            Evaluation evaluation = null;

            if(!evaluationCache.ContainsKey(cata))
            {
                evaluation = DQERepository?.GetMostRecentEvaluationFor(cata);
                evaluationCache.Add(cata, evaluation);
            }

            if(evaluation == null)
            {
                return null;
            }

            var columnStats = evaluation.ColumnStates.FirstOrDefault(c => string.Equals(c.TargetProperty, ci.Name));

            if (columnStats == null)
            {
                return null;
            }

            var total = columnStats.CountCorrect + columnStats.CountInvalidatesRow + columnStats.CountMissing + columnStats.CountWrong;

            if (total == 0)
            {
                return null;
            }

            return (int)(columnStats.CountDBNull / (double)total * 100);
        }

        private DateTime? GetStartDate(Catalogue c)
        {
            return TimespanCalculator?.GetMachineReadableTimepsanIfKnownOf(c, true, out _)?.Item1;
        }

        private DateTime? GetEndDate(Catalogue c)
        {
            return TimespanCalculator?.GetMachineReadableTimepsanIfKnownOf(c, true, out _)?.Item2;
        }

        /// <summary>
        /// Reads the contents of <paramref name="template"/> and generates one or more files (see <paramref name="oneFile"/>) by substituting tokens (e.g. $Name) for the values in the provided <paramref name="catalogues"/>
        /// </summary>
        /// <param name="catalogues">All catalogues that you want to produce metadata for</param>
        /// <param name="outputDirectory">The directory to write output file(s) into</param>
        /// <param name="template">Template file with free text and substitutions (e.g. $Name).  Also supports looping e.g. $foreach CatalogueItem</param>
        /// <param name="fileNaming">Determines how output file(s) will be named in the <paramref name="outputDirectory"/>.  Supports substitution e.g. $Name.md</param>
        /// <param name="oneFile">True to concatenate the results together and output in a single file.  If true then <paramref name="fileNaming"/> should not contain substitutions.  If false then <paramref name="fileNaming"/> should contain substitutions (e.g. $Name.doc) to prevent duplicate file names</param>
        public void GenerateReport(Catalogue[] catalogues, DirectoryInfo outputDirectory, FileInfo template, string fileNaming, bool oneFile)
        {
            if(catalogues == null || !catalogues.Any())
                return;
            
            var templateBody = File.ReadAllLines(template.FullName);

            string outname = DoReplacements(new []{fileNaming},catalogues.First(),null).Trim();

            StreamWriter outFile = null;
            
            try
            {
                if(oneFile)
                    outFile = new StreamWriter(File.Create(Path.Combine(outputDirectory.FullName, outname)));

                if (templateBody.Contains(LoopCatalogues))
                {
                    if (oneFile)
                    {
                        foreach(var section in SplitCatalogueLoops(templateBody))
                        {
                            if(section.IsPlainText)
                            {
                                outFile.WriteLine(string.Join(Environment.NewLine,section.Body));
                            }
                            else
                            {
                                foreach (Catalogue catalogue in catalogues)
                                {
                                    var newContents = DoReplacements(section.Body.ToArray(), catalogue,section);
                                    outFile.WriteLine(newContents);
                                }
                            }
                        }
                    }
                    else
                        throw new Exception($"'{LoopCatalogues}' is on valid when extracting in oneFile mode (a single document for all Catalogues' metadata)");
                }
                else
                {
                    foreach (Catalogue catalogue in catalogues)
                    {
                        var newContents = DoReplacements(templateBody, catalogue,null);

                        if (oneFile) 
                            outFile.WriteLine(newContents);
                        else
                        {
                            string filename = DoReplacements(new[] {fileNaming}, catalogue,null).Trim();

                            using (var sw = new StreamWriter(Path.Combine(outputDirectory.FullName,filename)))
                            {
                                sw.Write(newContents);
                                sw.Flush();
                                sw.Close();
                            }
                        }
                    }
                }
            }
            finally
            {
                outFile?.Flush();
                outFile?.Dispose();
            }   
        }

        private IEnumerable<CatalogueSection> SplitCatalogueLoops(string[] templateBody)
        {
            if(templateBody.Length == 0)
                yield break;

            CatalogueSection currentSection = null;
            int depth = 0;

            for(int i=0;i< templateBody.Length ;i++)
            {
                var str = templateBody[i];

                // is it trying to loop catalogue items
                if(str.Trim().Equals(LoopCatalogueItems))
                {
                    if(currentSection == null || currentSection.IsPlainText)
                        throw new CustomMetadataReportException($"Error, Unexpected '{str}' on line {i+1}.  Current section is plain text, '{LoopCatalogueItems}' can only appear within a '{LoopCatalogues}' block (you cannot mix and match top level loop elements)",i+1);

                    // ignore dives into CatalogueItems
                    depth++;

                    // but preserve it in the body because it will be needed later
                    currentSection.Body.Add(str);
                }
                else
                // is it a loop Catalogues
                if(str.Trim().Equals(LoopCatalogues))
                {
                    if(currentSection != null)
                        if(currentSection.IsPlainText)
                            yield return currentSection;
                        else
                            throw new CustomMetadataReportException($"Unexpected '{str}' before the end of the last one on line {i+1}",i+1);
                    
                    // start new section looping Catalogues
                    currentSection = new CatalogueSection(false,i);
                    depth = 1;
                }
                else
                // is it an end loop
                if(str.Trim().Equals(EndLoop))
                {
                    if(currentSection == null || currentSection.IsPlainText)
                        throw new CustomMetadataReportException($"Error, encountered '{str}' on line {i+1} while not in a {LoopCatalogues} block",i+1);

                    depth--;

                    // does end loop correspond to ending a $foreach Catalogue
                    if(depth == 0)
                    {
                        yield return currentSection;
                        currentSection = null;
                    }
                    else
                    if(depth <0)
                        throw new CustomMetadataReportException($"Error, unexpected '{str}' on line {i+1}",i+1);
                    else
                    {
                        // $end is for a CatalogueItem block so preserve it in the body
                        currentSection.Body.Add(str);
                    }
                }
                else
                {
                    // it's just a regular line of text

                    //if it's the first line of a new block we get a plaintext block
                    if(currentSection == null)
                        currentSection = new CatalogueSection(true,i);

                    currentSection.Body.Add(str);
                }
            }
            
            if(currentSection != null)
                if(currentSection.IsPlainText)
                    yield return currentSection;
                else 
                    throw new CustomMetadataReportException($"Reached end of template without finding an expected {EndLoop}",templateBody.Length);
        }

        private class CatalogueSection
        {
            public int LineNumber { get;set;}
            public bool IsPlainText { get;set;}
            public List<string> Body {get;set; } = new List<string>();

            public CatalogueSection(bool isPlainText, int lineNumber)
            {
                IsPlainText = isPlainText;
                LineNumber = lineNumber;
            }
        }

        private string DoReplacements(string[] strs, Catalogue catalogue, CatalogueSection section)
        {
            StringBuilder sb = new StringBuilder();

            for (var index = 0; index < strs.Length; index++)
            {
                var str = strs[index];
                string copy = str;

                if (str.Trim().Equals(LoopCatalogueItems, StringComparison.CurrentCultureIgnoreCase))
                {
                    index = DoReplacements(strs, index, out copy,catalogue.CatalogueItems,section) + 1;
                }
                else
                {
                    foreach (var r in Replacements)
                        if (copy.Contains(r.Key))
                            copy = copy.Replace(r.Key, ValueToString(r.Value(catalogue)));
                }

                sb.AppendLine(copy.TrimEnd());
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Consumes from $foreach to $end looping <paramref name="catalogueItems"/> to produce output rows of string data
        /// </summary>
        /// <param name="strs">The original input in its entirety</param>
        /// <param name="index">The line in <paramref name="strs"/> in which the foreach was detected</param>
        /// <param name="result">The results of consuming the foreach block</param>
        /// <param name="catalogueItems"></param>
        /// <param name="section"></param>
        /// <returns>The index in <paramref name="strs"/> where the $end was detected</returns>
        private int DoReplacements(string[] strs, int index, out string result, CatalogueItem[] catalogueItems,CatalogueSection section)
        {
            // The foreach template block as extracted from strs
            StringBuilder block = new StringBuilder();

            //the result of printing out the block once for each CatalogueItem item (with replacements)
            StringBuilder sbResult = new StringBuilder();

            int i = index+1;
            bool blockTerminated = false;

            int sectionOffset = section?.LineNumber ??0;

            //starting on the next line after $foreach until the end of the file
            for (; i < strs.Length; i++)
            {
                var current = strs[i];
                int lineNumberHuman = i +1+sectionOffset;

                if (current.Trim().Equals(EndLoop, StringComparison.CurrentCultureIgnoreCase))
                {
                    blockTerminated = true;
                    break;
                }

                if(current == LoopCatalogueItems)
                    throw new CustomMetadataReportException($"Error, encountered '{current}' on line {lineNumberHuman} before the end of current block which started on line {lineNumberHuman}.  Make sure to add {EndLoop} at the end of each loop",lineNumberHuman);

                block.AppendLine(current);
            }

            if(!blockTerminated)
                throw new CustomMetadataReportException($"Expected {EndLoop} to match $foreach which started on line {index+1+sectionOffset}",index+1+sectionOffset);

            foreach (CatalogueItem ci in catalogueItems) 
                sbResult.AppendLine(DoReplacements(block.ToString(), ci));

            result = sbResult.ToString();

            return i;
        }

        /// <summary>
        /// Returns a string representation suitable for adding to a template output based on the input object (which may be null)
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private string ValueToString(object v)
        {
            return ReplaceNewlines(v?.ToString() ?? "");
        }

        public string ReplaceNewlines(string input)
        {
            if(input != null && NewlineSubstitution != null)
                return Regex.Replace(input,"[\r]?\n",NewlineSubstitution);

            return input;
        }

        /// <summary>
        /// Replace all templated strings (e.g. $Name) in the <paramref name="template"/> with the corresponding values in the <paramref name="catalogueItem"/>
        /// </summary>
        /// <param name="template"></param>
        /// <param name="catalogueItem"></param>
        /// <returns></returns>
        private string DoReplacements(string template, CatalogueItem catalogueItem)
        {
            foreach (var r in ReplacementsCatalogueItem)
                if (template.Contains(r.Key))
                    template = template.Replace(r.Key, ValueToString(r.Value(catalogueItem)));

            return template.Trim();
        }
    }
}
