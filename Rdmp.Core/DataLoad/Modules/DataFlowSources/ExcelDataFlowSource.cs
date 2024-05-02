// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ExcelNumberFormat;
using FAnsi.Discovery;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Modules.Exceptions;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowSources;

/// <summary>
///     Pipeline component for reading from Microsoft Excel files.  Reads only from a single worksheet (by default the
///     first one in the workbook).  Data read
///     is returned as a DataTable all read at once in one big batch.  This component requires Microsoft Office to be
///     installed since it uses Interop.
/// </summary>
public class ExcelDataFlowSource : IPluginDataFlowSource<DataTable>, IPipelineRequirement<FlatFileToLoad>
{
    public const string WorkSheetName_DemandDescription =
        "Name of the worksheet to load data from (single sheet name only).  If this is empty then the first sheet in the spreadsheet will be loaded instead";

    public const string AddFilenameColumnNamed_DemandDescription =
        "Optional - Set to the name of a column in your RAW database (e.g. Filename).  If set this named column will be populated with the path to the file being read (e.g. c:\\myproj\\Data\\ForLoading\\MyFile.csv)";

    [DemandsInitialization(WorkSheetName_DemandDescription)]
    public string WorkSheetName { get; set; }

    [DemandsInitialization(DelimitedFlatFileDataFlowSource.MakeHeaderNamesSane_DemandDescription,
        DemandType.Unspecified, true)]
    public bool MakeHeaderNamesSane { get; set; }

    [DemandsInitialization(AddFilenameColumnNamed_DemandDescription)]
    public string AddFilenameColumnNamed { get; set; }

    private FlatFileToLoad _fileToLoad;

    private DataTable dataReadFromFile;
    private bool haveDispatchedDataTable;

    public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken,
        int rowOffset = 0, int columnOffset = 0, string[] replacementHeadersSplit = null)
    {
        dataReadFromFile ??= GetAllData(listener, cancellationToken, rowOffset, columnOffset, replacementHeadersSplit);

        if (haveDispatchedDataTable)
            return null;

        haveDispatchedDataTable = true;

        return dataReadFromFile;
    }

    private DataTable GetAllData(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken,
        int rowOffset = 0, int columnOffset = 0, string[] replacementHeadersSplit = null)
    {
        var sw = new Stopwatch();
        sw.Start();
        if (_fileToLoad == null)
            throw new Exception("_fileToLoad has not been set yet, possibly component has not been Initialized yet");

        if (!IsAcceptableFileExtension())
            throw new Exception($"FileToLoad ({_fileToLoad.File.FullName}) extension was not XLS or XLSX, dubious");

        using var fs = new FileStream(_fileToLoad.File.FullName, FileMode.Open);
        IWorkbook wb = _fileToLoad.File.Extension == ".xls" ? new HSSFWorkbook(fs) : new XSSFWorkbook(fs);

        DataTable toReturn = null;

        try
        {
            var worksheet =
                //if the user hasn't picked one, use the first
                (string.IsNullOrWhiteSpace(WorkSheetName) ? wb.GetSheetAt(0) : wb.GetSheet(WorkSheetName)) ??
                throw new FlatFileLoadException(
                    $"The Excel sheet '{WorkSheetName}' was not found in workbook '{_fileToLoad.File.Name}'");
            toReturn = GetAllData(worksheet, listener, rowOffset, columnOffset, replacementHeadersSplit);

            //set the table name the file name
            toReturn.TableName =
                QuerySyntaxHelper.MakeHeaderNameSensible(Path.GetFileNameWithoutExtension(_fileToLoad.File.Name));

            if (toReturn.Columns.Count == 0)
                throw new FlatFileLoadException(
                    $"The Excel sheet '{worksheet.SheetName}' in workbook '{_fileToLoad.File.Name}' is empty");

            //if the user wants a column in the DataTable storing the filename loaded add it
            if (!string.IsNullOrWhiteSpace(AddFilenameColumnNamed))
            {
                toReturn.Columns.Add(AddFilenameColumnNamed);
                foreach (DataRow dataRow in toReturn.Rows)
                    dataRow[AddFilenameColumnNamed] = _fileToLoad.File.FullName;
            }
        }
        finally
        {
            wb.Close();
            toReturn?.EndLoadData();
        }

        return toReturn;
    }

    /// <summary>
    ///     Returns all data held in the current <paramref name="worksheet" />.  The first row of data becomes the headers.
    ///     Throws away fully blank columns/rows.
    /// </summary>
    /// <param name="worksheet"></param>
    /// <param name="listener"></param>
    /// <param name="rowOffset"></param>
    /// <param name="columnOffset"></param>
    /// <param name="replacementHeadersSplit"></param>
    /// <returns></returns>
    public DataTable GetAllData(ISheet worksheet, IDataLoadEventListener listener, int rowOffset = 0,
        int columnOffset = 0, string[] replacementHeadersSplit = null)
    {
        var toReturn = new DataTable();
        toReturn.BeginLoadData();

        var rowEnumerator = worksheet.GetRowEnumerator();
        var nColumns = -1;

        var nonBlankColumns = new Dictionary<int, DataColumn>();

        while (rowEnumerator.MoveNext())
        {
            var row = (IRow)rowEnumerator.Current;
            if (rowOffset - 1 > row.RowNum) continue; // .RowNumber is 0 indexed

            //if all the cells in the current row are blank skip it (eliminates top of file whitespace)
            if (row.Cells.All(c => string.IsNullOrWhiteSpace(c.ToString())))
                continue;

            //first row (that has any data in it) - makes headers
            if (nColumns == -1)
            {
                nColumns = row.Cells.Count;
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Excel sheet {worksheet.SheetName} contains {nColumns}"));


                if (replacementHeadersSplit is not null && replacementHeadersSplit.Any() &&
                    replacementHeadersSplit.Length != nColumns)
                    listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Error,
                            $"ForceReplacementHeaders was set but it had {replacementHeadersSplit.Length} column header names while the file had {nColumns} (there must be the same number of replacement headers as headers in the excel file)"));

                var originalHeaders = new string[nColumns];
                for (var i = 0; i < nColumns; i++)
                {
                    //if the cell header is blank
                    var cell = row.Cells[i];
                    if (cell.ColumnIndex < columnOffset) continue;
                    string h;
                    try
                    {
                        h = cell.StringCellValue;
                    }
                    catch (Exception)
                    {
                        h = cell.NumericCellValue.ToString();
                    }

                    if (replacementHeadersSplit is not null && replacementHeadersSplit.Any() &&
                        replacementHeadersSplit.Length == nColumns)
                    {
                        originalHeaders[i] = h;
                        h = replacementHeadersSplit[i];
                    }

                    if (string.IsNullOrWhiteSpace(h))
                        continue;

                    nonBlankColumns.Add(cell.ColumnIndex, toReturn.Columns.Add(h));
                }

                if (replacementHeadersSplit is not null && replacementHeadersSplit.Any())
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                        $"Force headers will make the following header changes:{GenerateASCIIArtOfSubstitutions(originalHeaders, replacementHeadersSplit)}"));

                continue;
            }

            //the rest of the rows
            var r = toReturn.Rows.Add();

            var gotAtLeastOneGoodValue = false;

            foreach (var cell in row.Cells)
            {
                var value = GetCellValue(cell);

                //if the cell is blank skip it
                if (IsNull(value))
                    continue;

                //were we expecting this to be blank?
                if (!nonBlankColumns.ContainsKey(cell.ColumnIndex))
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                        $"Discarded the following data (that was found in unnamed columns):{value}"));
                    continue;
                }

                r[nonBlankColumns[cell.ColumnIndex]] = value.ToString();
                gotAtLeastOneGoodValue = true;
            }

            //if we didn't get any values at all for the row throw it away
            if (!gotAtLeastOneGoodValue)
                toReturn.Rows.Remove(r);
        }

        return toReturn;
    }

    private static string GenerateASCIIArtOfSubstitutions(string[] headers,
        string[] replacements)
    {
        var sb = new StringBuilder("");

        var max = Math.Max(replacements.Length, headers.Length);

        for (var i = 0; i < max; i++)
        {
            var replacement = i >= replacements.Length ? "???" : replacements[i];
            var original = i >= headers.Length ? "???" : headers[i];

            sb.Append($"{Environment.NewLine}[{i}]{original}>>>{replacement}");
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Returns the C# value that best represents the contents of the cell.
    /// </summary>
    /// <param name="cell">The cell whose value you want to retrieve</param>
    /// <param name="treatAs">Leave blank, used in recursion for dealing with Formula cells</param>
    /// <returns></returns>
    private object GetCellValue([CanBeNull] ICell cell, CellType treatAs = CellType.Unknown)
    {
        if (cell == null)
            return null;

        treatAs = treatAs switch
        {
            CellType.Formula => throw new Exception("Cannot treat the cell contents as a Formula"),
            CellType.Unknown => cell.CellType,
            _ => treatAs
        };

        switch (treatAs)
        {
            case CellType.Unknown:
                return cell.ToString();
            case CellType.Numeric:

                //some numerics are actually dates/times
                if (cell.CellStyle.DataFormat == 0) return cell.NumericCellValue;

                var format = cell.CellStyle.GetDataFormatString();

                if (IsDateWithoutTime(format))
                    return cell.DateCellValue.HasValue ? cell.DateCellValue.Value.ToString("yyyy-MM-dd") : null;

                if (IsDateWithTime(format))
                    return cell.DateCellValue.HasValue
                        ? cell.DateCellValue.Value.ToString("yyyy-MM-dd HH:mm:ss")
                        : null;

                if (IsTimeWithoutDate(format))
                    return cell.DateCellValue.HasValue ? cell.DateCellValue.Value.ToString("HH:mm:ss") : null;

                return new NumberFormat(format).Format(
                    IsDateFormat(format) ? cell.DateCellValue : cell.NumericCellValue, CultureInfo.InvariantCulture);

            case CellType.String:

                //if it is blank or 'null' then leave it null
                return string.IsNullOrWhiteSpace(cell.StringCellValue) ||
                       cell.StringCellValue.Trim().Equals("NULL", StringComparison.CurrentCultureIgnoreCase)
                    ? null
                    : cell.StringCellValue;

            case CellType.Formula:
                return GetCellValue(cell, cell.CachedFormulaResultType);
            case CellType.Blank:
                return null;
            case CellType.Boolean:
                return cell.BooleanCellValue;
            case CellType.Error:
                return null;
            default:
                throw new ArgumentOutOfRangeException(nameof(treatAs));
        }
    }

    private static bool IsDateWithTime(string formatString)
    {
        return formatString.Contains('h') && formatString.Contains('y');
    }

    private static bool IsDateWithoutTime(string formatString)
    {
        return formatString.Contains('y') && !formatString.Contains('h');
    }

    private static bool IsTimeWithoutDate(string formatString)
    {
        return !formatString.Contains('y') && formatString.Contains('h');
    }

    private static bool IsDateFormat(string formatString)
    {
        return !string.IsNullOrWhiteSpace(formatString) &&
               (formatString.Contains('/') ||
                formatString.Contains('\\') ||
                formatString.Contains(':'));
    }

    /*

    private string IntToExcelColumnLetter(int colNumberStartingAtOne)
    {
        int dividend = colNumberStartingAtOne;
        string columnName = String.Empty;
        int modulo;

        while (dividend > 0)
        {
            modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo) + columnName;
            dividend = (int)((dividend - modulo) / 26);
        }

        return columnName;
    }

    private object MakeTimeRelatedDecision(double value, Range cells, string type, dynamic cell,int row, int col)
    {
        if (type.Contains("/") || type.Contains("\\") || type.Contains(":"))
        {
            if (cell != null)
                return cell.Text;

            return cells[row, col].Text;
        }

        //timeRelatedDescisions
        return value;
    }
    */
    private readonly string[] acceptedFileExtensions =
    {
        ".xlsx",
        ".xls"
    };

    private bool IsAcceptableFileExtension()
    {
        return acceptedFileExtensions.Contains(_fileToLoad.File.Extension.ToLower());
    }

    private static bool IsNull(object o)
    {
        return o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString());
    }

    public void Check(ICheckNotifier notifier)
    {
        if (_fileToLoad != null)
            if (!IsAcceptableFileExtension())
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"File extension {_fileToLoad.File} has an invalid extension:{_fileToLoad.File.Extension} (this class only accepts:{string.Join(",", acceptedFileExtensions)})",
                        CheckResult.Fail));
            else
                notifier.OnCheckPerformed(
                    new CheckEventArgs($"File extension of file {_fileToLoad.File.Name} is acceptable",
                        CheckResult.Success));
        else
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "FlatFileToLoad (Pipeline Requirement) was not met (we weren't initialized with a file)",
                    CheckResult.Warning));
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public DataTable TryGetPreview()
    {
        var timeoutToken = new CancellationTokenSource();
        timeoutToken.CancelAfter(10000);

        var token = new GracefulCancellationToken(timeoutToken.Token, timeoutToken.Token);

        DataTable dt;
        try
        {
            dt = GetAllData(ThrowImmediatelyDataLoadEventListener.Quiet, token);
        }
        catch (Exception e)
        {
            if (timeoutToken.IsCancellationRequested)
                throw new Exception(
                    "Failed to generate preview in 10 seconds or less, giving up trying to load a preview (this doesn't mean that the source is broken, more likely you just have a big file or something)",
                    e);

            throw;
        }

        return dt;
    }

    public void PreInitialize(FlatFileToLoad value, IDataLoadEventListener listener)
    {
        _fileToLoad = value;
    }

    public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        return GetChunk(listener, cancellationToken, 0);
    }
}