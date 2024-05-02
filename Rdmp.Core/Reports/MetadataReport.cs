// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NPOI.XWPF.UserModel;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Reports;

/// <summary>
///     Describes a method that generates images for a <seealso cref="Catalogue" /> e.g. aggregate graphs
/// </summary>
/// <param name="catalogue"></param>
/// <returns></returns>
public delegate BitmapWithDescription[] RequestCatalogueImagesHandler(Catalogue catalogue);

/// <summary>
///     Generates a high level summary Microsoft Word DocX file of one or more Catalogues.  This includes the rowcount,
///     distinct patient count, description and descriptions
///     of extractable columns as well as an Appendix of Lookups.  In addition any IsExtractable AggregateConfiguration
///     graphs will be run and screen captured and added to
///     the report (including heatmap if a dynamic pivot is included in the graph).
/// </summary>
public class MetadataReport : DocXHelper
{
    private readonly ICatalogueRepository _repository;
    private readonly MetadataReportArgs _args;
    private readonly HashSet<TableInfo> _lookupsEncounteredToAppearInAppendix = new();

    public float PageWidthInPixels { get; private set; }

    public event RequestCatalogueImagesHandler RequestCatalogueImages;

    private const int TextFontSize = 7;


    public MetadataReport(ICatalogueRepository repository, MetadataReportArgs args)
    {
        _repository = repository;
        _args = args;
    }

    private Thread thread;

    public void GenerateWordFileAsync(IDataLoadEventListener listener, bool showFile)
    {
        thread = new Thread(() => GenerateWordFile(listener, showFile));
        thread.Start();
    }

    public FileInfo GenerateWordFile(IDataLoadEventListener listener, bool showFile)
    {
        try
        {
            //if there's only one catalogue call it 'prescribing.docx' etc
            var filename = _args.Catalogues.Length == 1 ? _args.Catalogues[0].Name : "MetadataReport";

            using var document = GetNewDocFile(filename);
            PageWidthInPixels = GetPageWidth();

            var sw = Stopwatch.StartNew();

            try
            {
                var completed = 0;


                foreach (var c in _args.Catalogues.OrderBy(c => c.Name))
                {
                    listener.OnProgress(this,
                        new ProgressEventArgs("Extracting",
                            new ProgressMeasurement(completed++, ProgressType.Records, _args.Catalogues.Length),
                            sw.Elapsed));

                    var recordCount = -1;
                    var distinctRecordCount = -1;
                    string identifierName = null;

                    var gotRecordCount = false;
                    try
                    {
                        if (_args.IncludeRowCounts)
                        {
                            GetRecordCount(c, out recordCount, out distinctRecordCount, out identifierName);
                            gotRecordCount = true;
                        }
                    }
                    catch (Exception e)
                    {
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                            $"Error processing record count for Catalogue {c.Name}", e));
                    }

                    InsertHeader(document, c.Name);

                    //assume we don't know the age of the dataset
                    DateTime? accurateAsOf = null;

                    //get the age of the dataset if known and output it
                    if (_args.TimespanCalculator != null)
                    {
                        var timespan =
                            _args.TimespanCalculator.GetHumanReadableTimespanIfKnownOf(c, true, out accurateAsOf);
                        if (!string.IsNullOrWhiteSpace(timespan) && !timespan.Equals("Unknown"))
                            InsertParagraph(document, timespan + (accurateAsOf.HasValue ? "*" : ""), TextFontSize);
                    }

                    InsertParagraph(document, c.Description, TextFontSize);

                    if (accurateAsOf.HasValue)
                        InsertParagraph(document, $"* Based on DQE run on {accurateAsOf.Value}", TextFontSize - 2);

                    if (gotRecordCount)
                    {
                        InsertHeader(document, "Record Count", 3);
                        CreateCountTable(document, recordCount, distinctRecordCount, identifierName);
                    }

                    if (!_args.SkipImages && RequestCatalogueImages != null)
                    {
                        var onRequestCatalogueImages = RequestCatalogueImages(c);

                        if (onRequestCatalogueImages.Any())
                        {
                            InsertHeader(document, "Aggregates", 2);
                            AddImages(document, onRequestCatalogueImages);
                        }
                    }

                    CreateDescriptionsTable(document, c);

                    if (_args.IncludeNonExtractableItems)
                        CreateNonExtractableColumnsTable(document, c);

                    //if this is not the last Catalogue create a new page
                    if (completed != _args.Catalogues.Length)
                        InsertSectionPageBreak(document);

                    listener.OnProgress(this,
                        new ProgressEventArgs("Extracting",
                            new ProgressMeasurement(completed, ProgressType.Records, _args.Catalogues.Length),
                            sw.Elapsed));
                }

                if (_lookupsEncounteredToAppearInAppendix.Any())
                    CreateLookupAppendix(document, listener);

                if (showFile)
                    ShowFile(document);

                SetMargins(document, 20);

                AddFooter(document, $"Created on {DateTime.Now}", TextFontSize);

                return document.FileInfo;
            }
            catch (ThreadInterruptedException)
            {
                //user hit abort
            }
        }
        catch (Exception e)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Error, "Entire process failed, see Exception for details", e));
        }

        return null;
    }

    private void CreateLookupAppendix(XWPFDocument document, IDataLoadEventListener listener)
    {
        InsertSectionPageBreak(document);
        InsertHeader(document, "Appendix 1 - Lookup Tables");

        //foreach lookup
        foreach (var lookupTable in _lookupsEncounteredToAppearInAppendix)
        {
            DataTable dt = null;

            try
            {
                dt = GetLookupTableInfoContentsFromDatabase(lookupTable);
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                    $"Failed to get the contents of loookup {lookupTable.Name}", e));
            }

            if (dt == null)
                continue;

            //if it has too many columns
            if (dt.Columns.Count > 5)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"Lookup table {lookupTable.Name} has more than 5 columns so will not be processed"));
                continue;
            }

            //write name of lookup
            InsertHeader(document, lookupTable.Name);

            var table = InsertTable(document, Math.Min(dt.Rows.Count + 1, _args.MaxLookupRows + 2), dt.Columns.Count);

            var tableLine = 0;

            //write the headers to the table
            for (var i = 0; i < dt.Columns.Count; i++)
                SetTableCell(table, tableLine, i, dt.Columns[i].ColumnName, TextFontSize);

            //move to next line
            tableLine++;

            var maxLineCountDowner = _args.MaxLookupRows + 1; //1 for the headers and 1 for the ... row

            //see if it has any lookups
            foreach (DataRow row in dt.Rows)
            {
                for (var i = 0; i < dt.Columns.Count; i++)
                    SetTableCell(table, tableLine, i, Convert.ToString(row[i]));

                //move to next line
                tableLine++;
                maxLineCountDowner--;

                if (maxLineCountDowner == 1)
                {
                    for (var i = 0; i < dt.Columns.Count; i++)
                        SetTableCell(table, tableLine, i, "...");
                    break;
                }
            }

            AutoFit(table);

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Wrote out lookup table {lookupTable.Name} successfully"));
        }
    }


    private static DataTable GetLookupTableInfoContentsFromDatabase(TableInfo lookupTable)
    {
        //get the contents of the lookup
        using var con = DataAccessPortal.ExpectServer(lookupTable, DataAccessContext.InternalDataProcessing)
            .GetConnection();
        con.Open();

        using var cmd = DatabaseCommandHelper.GetCommand($"Select * from {lookupTable.Name}", con);
        using var da = DatabaseCommandHelper.GetDataAdapter(cmd);
        var dt = new DataTable();
        dt.BeginLoadData();
        da.Fill(dt);
        dt.EndLoadData();

        return dt;
    }

    private static void AddImages(XWPFDocument document, BitmapWithDescription[] onRequestCatalogueImages)
    {
        foreach (var image in onRequestCatalogueImages)
        {
            if (!string.IsNullOrWhiteSpace(image.Header))
                InsertHeader(document, image.Header, 3);

            if (!string.IsNullOrWhiteSpace(image.Description))
                InsertParagraph(document, image.Description);

            GetPicture(document, image.Bitmap);
        }
    }

    private void CreateDescriptionsTable(XWPFDocument document, Catalogue c)
    {
        var extractionInformations = c.GetAllExtractionInformation(ExtractionCategory.Any).Where(Include).ToList();
        extractionInformations.Sort(IsExtractionIdentifiersFirstOrder);

        if (!extractionInformations.Any())
            return;

        InsertHeader(document, "Extractable Columns", 2);

        var table = InsertTable(document, extractionInformations.Count + 1, 4);

        var tableLine = 0;

        SetTableCell(table, tableLine, 0, "Column", TextFontSize);
        SetTableCell(table, tableLine, 1, "Datatype", TextFontSize);
        SetTableCell(table, tableLine, 2, "Description", TextFontSize);
        SetTableCell(table, tableLine, 3, "Category", TextFontSize);

        tableLine++;


        foreach (var information in extractionInformations)
        {
            SetTableCell(table, tableLine, 0, information.GetRuntimeName(), TextFontSize);
            SetTableCell(table, tableLine, 1,
                information.ColumnInfo == null ? "ORPHAN" : information.ColumnInfo.Data_type, TextFontSize);
            var description = information.CatalogueItem.Description;

            //a field should only ever be a foreign key to one Lookup table
            var lookups = information.ColumnInfo?.GetAllLookupForColumnInfoWhereItIsA(LookupType.ForeignKey);

            //if it has any lookups
            if (lookups != null && lookups.Any())
            {
                var pkTableId = lookups.Select(l => l.PrimaryKey.TableInfo_ID).Distinct().SingleOrDefault();

                var lookupTable = _repository.GetObjectByID<TableInfo>(pkTableId);

                _lookupsEncounteredToAppearInAppendix.Add(lookupTable);

                description += $"References Lookup Table {lookupTable.GetRuntimeName()}";
            }

            SetTableCell(table, tableLine, 2, description, TextFontSize);
            SetTableCell(table, tableLine, 3, information.ExtractionCategory.ToString(), TextFontSize);

            tableLine++;
        }

        AutoFit(table);
    }

    private static void CreateNonExtractableColumnsTable(XWPFDocument document, Catalogue c)
    {
        var nonExtractableCatalogueItems = c.CatalogueItems.Where(ci => ci.ExtractionInformation == null).ToList();

        if (!nonExtractableCatalogueItems.Any())
            return;

        InsertHeader(document, "Other Columns (Not Extractable)", 2);

        var table = InsertTable(document, nonExtractableCatalogueItems.Count + 1, 3);

        var tableLine = 0;

        SetTableCell(table, tableLine, 0, "Column", TextFontSize);
        SetTableCell(table, tableLine, 1, "Datatype", TextFontSize);
        SetTableCell(table, tableLine, 2, "Description", TextFontSize);

        tableLine++;


        foreach (var ci in nonExtractableCatalogueItems.OrderBy(ci => ci.Name))
        {
            SetTableCell(table, tableLine, 0, ci.Name, TextFontSize);
            SetTableCell(table, tableLine, 1, ci.ColumnInfo?.Data_type ?? @"N\A", TextFontSize);
            SetTableCell(table, tableLine, 2, ci.Description, TextFontSize);
            tableLine++;
        }

        AutoFit(table);
    }

    private int IsExtractionIdentifiersFirstOrder(ExtractionInformation x, ExtractionInformation y)
    {
        if (x.IsExtractionIdentifier && !y.IsExtractionIdentifier)
            return -1;

        return y.IsExtractionIdentifier is true and true ? 1 : x.Order - y.Order;
    }

    private bool Include(ExtractionInformation arg)
    {
        return arg.ExtractionCategory switch
        {
            ExtractionCategory.Core => true,
            ExtractionCategory.Supplemental => true,
            ExtractionCategory.SpecialApprovalRequired => true,
            ExtractionCategory.Internal => _args.IncludeInternalItems,
            ExtractionCategory.Deprecated => _args.IncludeDeprecatedItems,
            ExtractionCategory.ProjectSpecific => true,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void CreateCountTable(XWPFDocument document, int recordCount, int distinctCount, string identifierName)
    {
        var table = InsertTable(document, 2, identifierName != null && _args.IncludeDistinctIdentifierCounts ? 2 : 1);

        var tableLine = 0;

        SetTableCell(table, tableLine, 0, "Records", TextFontSize);

        //only add column values if there is an IsExtractionIdentifier returned
        if (identifierName != null && _args.IncludeDistinctIdentifierCounts)
            SetTableCell(table, tableLine, 1, $"Distinct {identifierName}", TextFontSize);

        tableLine++;


        SetTableCell(table, tableLine, 0, recordCount.ToString("N0"), TextFontSize);

        //only add column values if there is an IsExtractionIdentifier returned
        if (identifierName != null && _args.IncludeDistinctIdentifierCounts)
            SetTableCell(table, tableLine, 1, distinctCount.ToString("N0"), TextFontSize);
    }


    private void GetRecordCount(Catalogue c, out int count, out int distinct, out string identifierName)
    {
        //one of the fields will be marked IsExtractionIdentifier (e.g. CHI column)
        var bestExtractionInformation = c.GetAllExtractionInformation(ExtractionCategory.Any)
            .Where(e => e.IsExtractionIdentifier).ToArray();

        TableInfo tableToQuery = null;

        //there is no extraction identifier or we are not doing distincts
        if (!bestExtractionInformation.Any())
        {
            //there is no extraction identifier, let's see what tables there are that we can query
            var tableInfos =
                c.GetAllExtractionInformation(ExtractionCategory.Any)
                    .Select(ei => ei.ColumnInfo.TableInfo_ID)
                    .Distinct()
                    .Select(_repository.GetObjectByID<TableInfo>)
                    .ToArray();

            //there is only one table that we can query
            if (tableInfos.Length == 1)
                tableToQuery = tableInfos.Single(); //query that one
            else if
                (tableInfos.Count(t => t.IsPrimaryExtractionTable) ==
                 1) //there are multiple tables but there is only one IsPrimaryExtractionTable
                tableToQuery = tableInfos.Single(t => t.IsPrimaryExtractionTable);
            else
                throw new Exception(
                    $"Did not know which table to query out of {string.Join(",", tableInfos.Select(t => t.GetRuntimeName()))} you can resolve this by marking one (AND ONLY ONE) of these tables as IsPrimaryExtractionTable=true"); //there are multiple tables and multiple or no IsPrimaryExtractionTable
        }
        else
        {
            tableToQuery =
                bestExtractionInformation[0].ColumnInfo
                    .TableInfo; //there is an extraction identifier so use its table to query
        }

        var hasExtractionIdentifier = bestExtractionInformation.Any();

        var server = c.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, true);
        using var con = server.GetConnection();
        con.Open();

        if (tableToQuery.Name.Contains('@'))
            throw new Exception(
                $"Table '{tableToQuery.Name}' looks like a table valued function so cannot be processed");

        var sql = $"SELECT {Environment.NewLine}";
        sql += "count(*) as recordCount";

        //if it has extraction information and we want a distinct count
        if (hasExtractionIdentifier && _args.IncludeDistinctIdentifierCounts)
            sql +=
                $",\r\ncount(distinct {bestExtractionInformation[0].SelectSQL}) as recordCountDistinct{Environment.NewLine}";

        sql += $" from {Environment.NewLine}";
        sql += tableToQuery.Name;

        identifierName = hasExtractionIdentifier ? bestExtractionInformation[0].GetRuntimeName() : null;

        using (var cmd = server.GetCommand(sql, con))
        {
            cmd.CommandTimeout = _args.Timeout;

            using var r = cmd.ExecuteReader();
            r.Read();
            count = Convert.ToInt32(r["recordCount"]);
            distinct = hasExtractionIdentifier && _args.IncludeDistinctIdentifierCounts
                ? Convert.ToInt32(r["recordCountDistinct"])
                : -1;
        }

        con.Close();
    }

    public void Abort()
    {
        thread?.Interrupt();
    }
}