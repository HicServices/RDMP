// Copyright (c) The University of Dundee 2018-2019
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
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowSources;

/// <summary>
/// Pipeline component for reading from Microsoft Excel files.  Reads only from a single worksheet (by default the first one in the workbook).  Data read
/// is returned as a DataTable all read at once in one big batch.  This component requires Microsoft Office to be installed since it uses Interop.
/// </summary>
public class ExcelDataFlowSource : IPluginDataFlowSource<DataTable>, IPipelineRequirement<FlatFileToLoad>
{
    public const string WorkSheetName_DemandDescription =
        "Name of the worksheet to load data from (single sheet name only).  If this is empty then the first sheet in the spreadsheet will be loaded instead";

    public const string AddFilenameColumnNamed_DemandDescription = "Optional - Set to the name of a column in your RAW database (e.g. Filename).  If set this named column will be populated with the path to the file being read (e.g. c:\\myproj\\Data\\ForLoading\\MyFile.csv)";

    [DemandsInitialization(WorkSheetName_DemandDescription)]
    public string WorkSheetName { get; set; }
        
    [DemandsInitialization(DelimitedFlatFileDataFlowSource.MakeHeaderNamesSane_DemandDescription,DemandType.Unspecified,true)]
    public bool MakeHeaderNamesSane { get; set; }

    [DemandsInitialization(AddFilenameColumnNamed_DemandDescription)]
    public string AddFilenameColumnNamed { get; set; }

    private FlatFileToLoad _fileToLoad;
    private DataTable dataReadFromFile;
    private bool haveDispatchedDataTable = false;
        
    public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        if (dataReadFromFile == null)
            dataReadFromFile = GetAllData(listener, cancellationToken);

        if (haveDispatchedDataTable)
            return null;

        haveDispatchedDataTable = true;
            
        return dataReadFromFile;
    }
        
    private DataTable GetAllData(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        var sw = new Stopwatch();
        sw.Start();
        if(_fileToLoad == null)
            throw new Exception("_fileToLoad has not been set yet, possibly component has not been Initialized yet");

        if (!IsAcceptableFileExtension())
            throw new Exception($"FileToLoad ({_fileToLoad.File.FullName}) extension was not XLS or XLSX, dubious");

        using (var fs = new FileStream(_fileToLoad.File.FullName, FileMode.Open))
        {
            IWorkbook wb;
            if (_fileToLoad.File.Extension == ".xls")
                wb = new HSSFWorkbook(fs);
            else
                wb = new XSSFWorkbook(fs);

            DataTable toReturn;

            try
            {
                ISheet worksheet;

                //if the user hasn't picked one, use the first
                worksheet = string.IsNullOrWhiteSpace(WorkSheetName) ? wb.GetSheetAt(0) : wb.GetSheet(WorkSheetName);

                if (worksheet == null)
                    throw new FlatFileLoadException(string.Format("The Excel sheet '{0}' was not found in workbook '{1}'", WorkSheetName, _fileToLoad.File.Name));

                toReturn = GetAllData(worksheet, listener);

                //set the table name the file name
                toReturn.TableName = QuerySyntaxHelper.MakeHeaderNameSensible(Path.GetFileNameWithoutExtension(_fileToLoad.File.Name));

                if (toReturn.Columns.Count == 0)
                    throw new FlatFileLoadException(string.Format("The Excel sheet '{0}' in workbook '{1}' is empty", worksheet.SheetName, _fileToLoad.File.Name));

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
            }

            return toReturn;
        }
    }
        
    /// <summary>
    /// Returns all data held in the current <paramref name="worksheet"/>.  The first row of data becomes the headers.  Throws away fully blank columns/rows.
    /// </summary>
    /// <param name="worksheet"></param>
    /// <param name="listener"></param>
    /// <returns></returns>
    public DataTable GetAllData(ISheet worksheet, IDataLoadEventListener listener)
    {
        var toReturn = new DataTable();

        var rowEnumerator = worksheet.GetRowEnumerator();
        var nColumns = -1;

        var nonBlankColumns = new Dictionary<int, DataColumn>();

        while (rowEnumerator.MoveNext())
        {
            var row = (IRow)rowEnumerator.Current;

            //if all the cells in the current row are blank skip it (eliminates top of file whitespace)
            if (row.Cells.All(c => string.IsNullOrWhiteSpace(c.ToString())))
                continue;

            //first row (that has any data in it) - makes headers
            if (nColumns == -1)
            {
                nColumns = row.Cells.Count;
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Excel sheet {worksheet.SheetName} contains {nColumns}"));

                for (var i = 0; i < nColumns; i++)
                {
                    //if the cell header is blank
                    var cell = row.Cells[i];
                    var h = cell.StringCellValue;
                    if (string.IsNullOrWhiteSpace(h))
                        continue;

                    nonBlankColumns.Add(cell.ColumnIndex, toReturn.Columns.Add(h));
                }

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
                        $"Discarded the following data (that was found in unamed columns):{value}"));
                    continue;
                }

                r[nonBlankColumns[cell.ColumnIndex]] = value.ToString();
                gotAtLeastOneGoodValue = true;
            }

            //if we didn't get any values at all for the row throw it away
            if(!gotAtLeastOneGoodValue)
                toReturn.Rows.Remove(r);
        }
            
        return toReturn;
    }

    /// <summary>
    /// Retruns the C# value that best represents the contents of the cell.
    /// </summary>
    /// <param name="cell">The cell whose value you want to retrieve</param>
    /// <param name="treatAs">Leave blank, used in recursion for dealing with Formula cells</param>
    /// <returns></returns>
    private object GetCellValue(ICell cell, CellType treatAs = CellType.Unknown)
    {
        if (cell == null)
            return null;

        if (treatAs == CellType.Formula)
            throw new Exception("Cannot treat the cell contents as a Formula");

        if (treatAs == CellType.Unknown)
            treatAs = cell.CellType;
            
        switch (treatAs)
        {
            case CellType.Unknown:
                return cell.ToString();
            case CellType.Numeric:

                //some numerics are actually dates/times
                if (cell.CellStyle.DataFormat != 0)
                {
                    var format = cell.CellStyle.GetDataFormatString();
                    var f = new NumberFormat(format);

                    if (IsDateWithoutTime(format))
                        return cell.DateCellValue.ToString("yyyy-MM-dd");
                        
                    if(IsDateWithTime(format))
                        return cell.DateCellValue.ToString("yyyy-MM-dd HH:mm:ss");

                    if (IsTimeWithoutDate(format))
                        return cell.DateCellValue.ToString("HH:mm:ss");

                    return IsDateFormat(format)
                        ? f.Format(cell.DateCellValue, CultureInfo.InvariantCulture)
                        : f.Format(cell.NumericCellValue, CultureInfo.InvariantCulture);
                }

                return cell.NumericCellValue;
            case CellType.String:
                    
                var v = cell.StringCellValue;

                //if it is blank or 'null' then leave it null
                if (string.IsNullOrWhiteSpace(v) || v.Trim().Equals("NULL", StringComparison.CurrentCultureIgnoreCase))
                    return null;

                return cell.StringCellValue;
            case CellType.Formula:
                return GetCellValue(cell, cell.CachedFormulaResultType);
            case CellType.Blank:
                return null;
            case CellType.Boolean:
                return cell.BooleanCellValue;
            case CellType.Error:
                return null;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool IsDateWithTime(string formatString)
    {
        return formatString.Contains("h") && formatString.Contains("y");
    }
    private bool IsDateWithoutTime(string formatString)
    {
        return formatString.Contains("y") && !formatString.Contains("h");
    }

    private bool IsTimeWithoutDate(string formatString)
    {
        return !formatString.Contains("y") && formatString.Contains("h");
    }

    private bool IsDateFormat(string formatString)
    {
        if (string.IsNullOrWhiteSpace(formatString))
            return false;

        return formatString.Contains("/") || formatString.Contains("\\") || formatString.Contains(":");
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
    private string[] acceptedFileExtensions =
    {
        ".xlsx",
        ".xls"
    };

    private bool IsAcceptableFileExtension()
    {
        return acceptedFileExtensions.Contains(_fileToLoad.File.Extension.ToLower());
    }

    private bool IsNull(object o)
    {
        if (o == null || o == DBNull.Value)
            return true;

        return string.IsNullOrWhiteSpace(o.ToString());
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

        var token = new GracefulCancellationToken(timeoutToken.Token,timeoutToken.Token );

        DataTable dt;
        try
        {
            dt = GetAllData(new ThrowImmediatelyDataLoadEventListener(),token);
        }
        catch (Exception e)
        {
            if(timeoutToken.IsCancellationRequested)
                throw new Exception("Failed to generate preview in 10 seconds or less, giving up trying to load a preview (this doesn't mean that the source is broken, more likely you just have a big file or something)",e);

            throw;
        }

        return dt;
    }

    public void PreInitialize(FlatFileToLoad value, IDataLoadEventListener listener)
    {
        _fileToLoad = value;
    }
}