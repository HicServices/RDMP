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

namespace Rdmp.Core.Reports;

/// <summary>
/// Create a custom report e.g. markdown, xml etc by taking a template file and replicating it with replacements for each <see cref="Catalogue"/> property
/// </summary>
public partial class CustomMetadataReport
{
    /// <summary>
    /// Substitutions that are used during template value replacement e.g. $Name => Catalogue.Name
    /// </summary>
    private Dictionary<string, Func<Catalogue, object>> Replacements = new();

    /// <summary>
    /// Substitutions that are used during template value replacement when inside a '$foreach CatalogueItem' block e.g. $Name => CatalogueItem.Name
    /// </summary>
    private Dictionary<string, Func<CatalogueItem, object>> ReplacementsCatalogueItem = new();


    /// <summary>
    /// Control line that begins looping Catalogues
    /// </summary>
    public const string LoopCatalogues = "$foreach Catalogue";

    /// <summary>
    /// Control line that begins looping CatalogueItems of a Catalogue
    /// </summary>
    public const string LoopCatalogueItems = "$foreach CatalogueItem";

    public const string Comma = "$Comma";

    /// <summary>
    /// Ends looping
    /// </summary>
    public const string EndLoop = "$end";

    /// <summary>
    /// How the range of data in each <see cref="Catalogue"/> is determined, defaults to <see cref="DatasetTimespanCalculator"/> (using the DQE results)
    /// </summary>
    public IDetermineDatasetTimespan TimespanCalculator { get; set; } = new DatasetTimespanCalculator();

    /// <summary>
    /// Specify a replacement for newlines when found in fields e.g. with space.  Leave as null to leave newlines intact.
    /// </summary>
    public string NewlineSubstitution { get; set; }


    /// <summary>
    /// Specify a replacement for the token element separator replacement <see cref="Comma"/> (note that this option
    /// only affects the token not regular commas in the template).
    /// </summary>
    public string CommaSubstitution { get; set; } = ",";

    /// <summary>
    /// The repository where column completeness metrics will come from.  Note this is usually the same source as <see cref="TimespanCalculator"/>
    /// </summary>
    public DQERepository DQERepository { get; set; }

    /// <summary>
    /// Cache of latest run DQE <see cref="Evaluation"/> by <see cref="Catalogue"/> (populated from <see cref="DQERepository"/>)
    /// </summary>
    public Dictionary<ICatalogue, Evaluation> EvaluationCache { get; set; } = new();

    /// <summary>
    /// Describes whether a $foreach is iterating and which element type is
    /// currently being processed for replacement
    /// </summary>
    private enum ElementIteration
    {
        NotIterating,
        RegularElement,
        LastElement
    }

    public CustomMetadataReport(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        // Catalogue level properties  (works in root or $foreach Catalogue block)

        //add basic properties
        foreach (var prop in typeof(Catalogue).GetProperties())
            Replacements.Add($"${prop.Name}", c => prop.GetValue(c));

        //add basic properties TableInfo
        foreach (var prop in typeof(TableInfo).GetProperties())
            // if it's not already a property of Catalogue
            Replacements.TryAdd($"${prop.Name}", c => GetTable(c) == null ? null : prop.GetValue(GetTable(c)));

        AddDQEReplacements();

        // Catalogue Item level properties (only work in a $foreach CatalogueItem block)

        //add basic properties CatalogueItem
        foreach (var prop in typeof(CatalogueItem).GetProperties())
            ReplacementsCatalogueItem.Add($"${prop.Name}", s => prop.GetValue(s));

        //add basic properties ColumnInfo
        foreach (var prop in typeof(ColumnInfo).GetProperties())
            // if it's not already a property of CatalogueItem
            ReplacementsCatalogueItem.TryAdd($"${prop.Name}",
                s => s.ColumnInfo_ID == null ? null : prop.GetValue(s.ColumnInfo));

        try
        {
            DQERepository = new DQERepository(repositoryLocator.CatalogueRepository);
        }
        catch (NotSupportedException)
        {
            DQERepository = null;
        }
    }

    private static ITableInfo GetTable(Catalogue c)
    {
        return c.GetTableInfosIdeallyJustFromMainTables().MinBy(t => t.IsPrimaryExtractionTable);
    }

    private void AddDQEReplacements()
    {
        // Catalogue level shortcuts
        Replacements.Add("$DQE_StartDate",
            c => GetStartDate(c)?.ToString());
        Replacements.Add("$DQE_EndDate",
            c => GetEndDate(c)?.ToString());
        Replacements.Add("$DQE_DateRange",
            c => TimespanCalculator?.GetHumanReadableTimespanIfKnownOf(c, true, out _));

        Replacements.Add("$DQE_StartYear",
            c => GetStartDate(c)?.ToString("yyyy"));
        Replacements.Add("$DQE_StartMonth",
            c => GetStartDate(c)?.ToString("MM"));
        Replacements.Add("$DQE_StartDay",
            c => GetStartDate(c)?.ToString("dd"));

        Replacements.Add("$DQE_EndYear",
            c => GetEndDate(c)?.ToString("yyyy"));
        Replacements.Add("$DQE_EndMonth",
            c => GetEndDate(c)?.ToString("MM"));
        Replacements.Add("$DQE_EndDay",
            c => GetEndDate(c)?.ToString("dd"));

        Replacements.Add("$DQE_DateOfEvaluation",
            c => GetFromEvaluation(c, static e => e.DateOfEvaluation));
        Replacements.Add("$DQE_CountTotal",
            c => GetFromEvaluation(c, static e => e.GetRecordCount()));

        ReplacementsCatalogueItem.Add("$DQE_PercentNull",
            GetPercentNull);

        ReplacementsCatalogueItem.Add("$DQE_CountCorrect",
            ci => GetFromColumnState(ci, static s => s.CountCorrect));
        ReplacementsCatalogueItem.Add("$DQE_CountInvalidatesRow",
            ci => GetFromColumnState(ci, static s => s.CountInvalidatesRow));
        ReplacementsCatalogueItem.Add("$DQE_CountMissing",
            ci => GetFromColumnState(ci, static s => s.CountMissing));
        ReplacementsCatalogueItem.Add("$DQE_CountWrong",
            ci => GetFromColumnState(ci, static s => s.CountWrong));
        ReplacementsCatalogueItem.Add("$DQE_CountTotal",
            ci => GetFromColumnState(ci, static s => s.CountCorrect + s.CountMissing + s.CountWrong + s.CountInvalidatesRow));

        ReplacementsCatalogueItem.Add("$DQE_CountDBNull",
            ci => GetFromColumnState(ci, static s => s.CountDBNull));
    }

    private object GetFromEvaluation(Catalogue c, Func<Evaluation, object> func)
    {
        var eval = GetEvaluation(c);
        return eval != null ? func(eval) : null;
    }

    private object GetFromColumnState(CatalogueItem ci, Func<ColumnState, object> func)
    {
        var state = GetColumnState(ci);
        return state != null ? func(state) : null;
    }

    private ColumnState GetColumnState(CatalogueItem ci)
    {
        return GetEvaluation(ci)?.ColumnStates.FirstOrDefault(c => string.Equals(c.TargetProperty, ci.Name));
    }

    private string GetPercentNull(CatalogueItem ci)
    {
        var columnStats = GetColumnState(ci);

        if (columnStats == null) return null;

        var total = columnStats.CountCorrect + columnStats.CountInvalidatesRow + columnStats.CountMissing +
                    columnStats.CountWrong;

        return total == 0 ? null : $"{(int)(columnStats.CountDBNull / (double)total * 100)}%";
    }

    private Evaluation GetEvaluation(CatalogueItem ci) => GetEvaluation(ci.Catalogue);

    private Evaluation GetEvaluation(Catalogue c)
    {
        if (!EvaluationCache.TryGetValue(c, out var evaluation))
        {
            evaluation = DQERepository?.GetMostRecentEvaluationFor(c);
            EvaluationCache.Add(c, evaluation);
        }

        return evaluation;
    }

    private DateTime? GetStartDate(Catalogue c) =>
        TimespanCalculator?.GetMachineReadableTimespanIfKnownOf(c, true, out _)?.Item1;

    private DateTime? GetEndDate(Catalogue c) =>
        TimespanCalculator?.GetMachineReadableTimespanIfKnownOf(c, true, out _)?.Item2;

    /// <summary>
    /// Reads the contents of <paramref name="template"/> and generates one or more files (see <paramref name="oneFile"/>) by substituting tokens (e.g. $Name) for the values in the provided <paramref name="catalogues"/>
    /// </summary>
    /// <param name="catalogues">All catalogues that you want to produce metadata for</param>
    /// <param name="outputDirectory">The directory to write output file(s) into</param>
    /// <param name="template">Template file with free text and substitutions (e.g. $Name).  Also supports looping e.g. $foreach CatalogueItem</param>
    /// <param name="fileNaming">Determines how output file(s) will be named in the <paramref name="outputDirectory"/>.  Supports substitution e.g. $Name.md</param>
    /// <param name="oneFile">True to concatenate the results together and output in a single file.  If true then <paramref name="fileNaming"/> should not contain substitutions.  If false then <paramref name="fileNaming"/> should contain substitutions (e.g. $Name.doc) to prevent duplicate file names</param>
    public void GenerateReport(Catalogue[] catalogues, DirectoryInfo outputDirectory, FileInfo template,
        string fileNaming, bool oneFile)
    {
        if (catalogues == null || !catalogues.Any())
            return;

        var templateBody = File.ReadAllLines(template.FullName);

        var outname = DoReplacements(new[] { fileNaming }, catalogues.First(), null, ElementIteration.NotIterating)
            .Trim();

        StreamWriter outFile = null;

        try
        {
            if (oneFile)
                outFile = new StreamWriter(File.Create(Path.Combine(outputDirectory.FullName, outname)));

            if (templateBody.Contains(LoopCatalogues))
            {
                if (oneFile)
                    foreach (var section in SplitCatalogueLoops(templateBody))
                        if (section.IsPlainText)
                            outFile.WriteLine(string.Join(Environment.NewLine, section.Body));
                        else
                            for (var i = 0; i < catalogues.Length; i++)
                            {
                                var element =
                                    i == catalogues.Length - 1
                                        ? ElementIteration.LastElement
                                        : ElementIteration.RegularElement;

                                var newContents = DoReplacements(section.Body.ToArray(), catalogues[i], section,
                                    element);
                                outFile.WriteLine(newContents);
                            }
                else
                    throw new Exception(
                        $"'{LoopCatalogues}' is on valid when extracting in oneFile mode (a single document for all Catalogues' metadata)");
            }
            else
            {
                foreach (var catalogue in catalogues)
                {
                    var newContents = DoReplacements(templateBody, catalogue, null, ElementIteration.NotIterating);

                    if (oneFile)
                    {
                        outFile.WriteLine(newContents);
                    }
                    else
                    {
                        var filename = DoReplacements(new[] { fileNaming }, catalogue, null,
                            ElementIteration.NotIterating).Trim();

                        using var sw = new StreamWriter(Path.Combine(outputDirectory.FullName, filename));
                        sw.Write(newContents);
                        sw.Flush();
                        sw.Close();
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

    private static IEnumerable<CatalogueSection> SplitCatalogueLoops(string[] templateBody)
    {
        if (templateBody.Length == 0)
            yield break;

        CatalogueSection currentSection = null;
        var depth = 0;

        for (var i = 0; i < templateBody.Length; i++)
        {
            var str = templateBody[i];

            // is it trying to loop catalogue items
            if (str.Trim().Equals(LoopCatalogueItems))
            {
                if (currentSection == null || currentSection.IsPlainText)
                    throw new CustomMetadataReportException(
                        $"Error, Unexpected '{str}' on line {i + 1}.  Current section is plain text, '{LoopCatalogueItems}' can only appear within a '{LoopCatalogues}' block (you cannot mix and match top level loop elements)",
                        i + 1);

                // ignore dives into CatalogueItems
                depth++;

                // but preserve it in the body because it will be needed later
                currentSection.Body.Add(str);
            }
            else
            // is it a loop Catalogues
            if (str.Trim().Equals(LoopCatalogues))
            {
                if (currentSection != null)
                    yield return currentSection.IsPlainText
                        ? currentSection
                        : throw new CustomMetadataReportException(
                            $"Unexpected '{str}' before the end of the last one on line {i + 1}", i + 1);

                // start new section looping Catalogues
                currentSection = new CatalogueSection(false, i);
                depth = 1;
            }
            else
            // is it an end loop
            if (str.Trim().Equals(EndLoop))
            {
                if (currentSection == null || currentSection.IsPlainText)
                    throw new CustomMetadataReportException(
                        $"Error, encountered '{str}' on line {i + 1} while not in a {LoopCatalogues} block", i + 1);

                depth--;

                // does end loop correspond to ending a $foreach Catalogue
                if (depth == 0)
                {
                    yield return currentSection;
                    currentSection = null;
                }
                else if (depth < 0)
                {
                    throw new CustomMetadataReportException($"Error, unexpected '{str}' on line {i + 1}", i + 1);
                }
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
                currentSection ??= new CatalogueSection(true, i);

                currentSection.Body.Add(str);
            }
        }

        if (currentSection != null)
            yield return currentSection.IsPlainText
                ? currentSection
                : throw new CustomMetadataReportException(
                    $"Reached end of template without finding an expected {EndLoop}", templateBody.Length);
    }

    private class CatalogueSection
    {
        public int LineNumber { get; set; }
        public bool IsPlainText { get; set; }
        public List<string> Body { get; set; } = new();

        public CatalogueSection(bool isPlainText, int lineNumber)
        {
            IsPlainText = isPlainText;
            LineNumber = lineNumber;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="strs"></param>
    /// <param name="catalogue"></param>
    /// <param name="section"></param>
    /// <param name="iteration">Indicates if looping through Catalogues and we are not at the last element in the collection yet.</param>
    /// <returns></returns>
    private string DoReplacements(string[] strs, Catalogue catalogue, CatalogueSection section,
        ElementIteration iteration)
    {
        var sb = new StringBuilder();

        for (var index = 0; index < strs.Length; index++)
        {
            var str = strs[index];
            var copy = str;

            if (str.Trim().Equals(LoopCatalogueItems, StringComparison.CurrentCultureIgnoreCase))
            {
                index = DoReplacements(strs, index, out copy, catalogue.CatalogueItems, section);
            }
            else
            {
                foreach (var r in Replacements)
                    if (copy.Contains(r.Key))
                        copy = copy.Replace(r.Key, ValueToString(r.Value(catalogue)));

                // when iterating we need to respect iteration symbols (e.g. $Comma).
                if (iteration == ElementIteration.NotIterating)
                    ThrowIfContainsIterationElements(copy);
                else
                    copy = copy.Replace(Comma, iteration == ElementIteration.RegularElement ? CommaSubstitution : "");
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
    private int DoReplacements(string[] strs, int index, out string result, CatalogueItem[] catalogueItems,
        CatalogueSection section)
    {
        // The foreach template block as extracted from strs
        var block = new StringBuilder();

        //the result of printing out the block once for each CatalogueItem item (with replacements)
        var sbResult = new StringBuilder();

        var i = index + 1;
        var blockTerminated = false;

        var sectionOffset = section?.LineNumber ?? 0;

        //starting on the next line after $foreach until the end of the file
        for (; i < strs.Length; i++)
        {
            var current = strs[i];
            var lineNumberHuman = i + 1 + sectionOffset;

            if (current.Trim().Equals(EndLoop, StringComparison.CurrentCultureIgnoreCase))
            {
                blockTerminated = true;
                break;
            }

            if (current == LoopCatalogueItems)
                throw new CustomMetadataReportException(
                    $"Error, encountered '{current}' on line {lineNumberHuman} before the end of current block which started on line {lineNumberHuman}.  Make sure to add {EndLoop} at the end of each loop",
                    lineNumberHuman);

            block.AppendLine(current);
        }

        if (!blockTerminated)
            throw new CustomMetadataReportException(
                $"Expected {EndLoop} to match $foreach which started on line {index + 1 + sectionOffset}",
                index + 1 + sectionOffset);

        for (var j = 0; j < catalogueItems.Length; j++)
            sbResult.AppendLine(DoReplacements(block.ToString(), catalogueItems[j],
                j < catalogueItems.Length - 1 ? ElementIteration.RegularElement : ElementIteration.LastElement));


        result = sbResult.ToString();

        return i;
    }

    /// <summary>
    /// Returns a string representation suitable for adding to a template output based on the input object (which may be null)
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    private string ValueToString(object v) =>
        v is ExtractionInformation ei ? ei.GetRuntimeName() : ReplaceNewlines(v?.ToString() ?? "");

    public string ReplaceNewlines(string input) => input != null && NewlineSubstitution != null
        ? Newline().Replace(input, NewlineSubstitution)
        : input;

    /// <summary>
    /// Replace all templated strings (e.g. $Name) in the <paramref name="template"/> with the corresponding values in the <paramref name="catalogueItem"/>
    /// </summary>
    /// <param name="template"></param>
    /// <param name="catalogueItem"></param>
    /// <param name="iteration">Indicates if looping through CatalogueItems and we are not at the last element in the collection yet.</param>
    /// <returns></returns>
    private string DoReplacements(string template, CatalogueItem catalogueItem, ElementIteration iteration)
    {
        foreach (var r in ReplacementsCatalogueItem)
            if (template.Contains(r.Key))
                template = template.Replace(r.Key, ValueToString(r.Value(catalogueItem)));

        if (iteration == ElementIteration.NotIterating)
            ThrowIfContainsIterationElements(template);
        else
            template = template.Replace(Comma, iteration == ElementIteration.RegularElement ? CommaSubstitution : "");


        return template.TrimEnd();
    }

    private static void ThrowIfContainsIterationElements(string template)
    {
        if (template.Contains(Comma))
            throw new CustomMetadataReportException(
                $"Unexpected use of {Comma} outside of an iteration ($foreach) block", -1);
    }

    [GeneratedRegex("[\r]?\n")]
    private static partial Regex Newline();
}