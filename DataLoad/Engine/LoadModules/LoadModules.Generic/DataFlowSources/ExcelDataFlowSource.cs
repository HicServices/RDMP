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
using System.Text.RegularExpressions;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using ExcelNumberFormat;
using FAnsi.Discovery;
using LoadModules.Generic.Checks;
using LoadModules.Generic.Exceptions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataFlowSources
{
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
        private int successfullyRead = 0;

        DataTable dataReadFromFile;
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

        private int sketchyDoublesEvaluated = 0;

        private DataTable GetAllData(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if(_fileToLoad == null)
                throw new Exception("_fileToLoad has not been set yet, possibly component has not been Initialized yet");

            DataTable toReturn = new DataTable();
            
            //set the table name to:
            toReturn.TableName =  
                //sane version of
                QuerySyntaxHelper.MakeHeaderNameSane(
                    //the filename
                    Path.GetFileNameWithoutExtension(_fileToLoad.File.Name));
            
            successfullyRead = 0;
            if (!IsAcceptableFileExtension())
                throw new Exception("FileToLoad (" + _fileToLoad.File.FullName + ") extension was not XLS or XLSX, dubious");
            
            Stopwatch s = new Stopwatch();
            s.Start();

            var wb = new XSSFWorkbook(_fileToLoad.File.FullName);
            
            ISheet worksheet;
                     
            //if the user hasn't picked one, use the first
            if (string.IsNullOrWhiteSpace(WorkSheetName))
                worksheet = wb.GetSheetAt(0);
            else
                worksheet = wb.GetSheet(WorkSheetName);//else use the named sheet

            var rowEnumerator = worksheet.GetRowEnumerator();
            int nColumns = -1;

            Dictionary<int,DataColumn> nonBlankColumns = new Dictionary<int, DataColumn>();

            while (rowEnumerator.MoveNext())
            {
                var row = (IRow)rowEnumerator.Current;
                
                //if all the cells in the current row are blank skip it (eliminates top of file whitespace)
                if(row.Cells.All(c=>String.IsNullOrWhiteSpace(c.StringCellValue)))        
                    continue;

                //first row (that has any data in it) - makes headers
                if(nColumns == -1)
                {
                    nColumns = row.Cells.Count;
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Excel sheet " + worksheet.SheetName + " contains " + nColumns));

                    for (int i = 0; i < nColumns; i++)
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

                foreach (var cell in row.Cells)
                {
                    var value = GetCellValue(cell);

                    //if the cell is blank skip it
                    if (IsNull(value))
                        continue;
                    
                    //were we expecting this to be blank?
                    if (!nonBlankColumns.ContainsKey(cell.ColumnIndex))
                    {
                        listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning, "Discarded the following data (that was found in unamed columns):" + value));
                        continue;
                    }
                    
                    r[nonBlankColumns[cell.ColumnIndex]] = value.ToString();
                }
            }
                     
            /*
            //list of column headers in Excel file (improves performance), note that we add a blank at the start of the list because excel columns start at 1
            List<string> headersByColumnIndex = new List<string>();
            headersByColumnIndex.Add("");
                    
            var cells = usedRange.Cells;
                    
            Dictionary<int, string> knownColumnFormats = new Dictionary<int, string>();


            int columnHeadersRowIs = -1;

            //one indexed ofcourse because Excel
            for (int i = 1; i <= data.GetLength(0); i++)
            {
                bool foundValidData = false;

                for (int col = 1; col <= data.GetLength(1); col++)
                    if (!IsNull(data[i, col]))
                        foundValidData = true;

                if (foundValidData)
                {
                    columnHeadersRowIs = i;
                    break;
                }

            }
                    
            if (columnHeadersRowIs == -1)
                throw new FlatFileLoadException("The Excel sheet '" +  worksheet.Name + "' in workbook '" + wb.Name +"' is empty");
                    

            for (int i = 1; i <= nColumns; i++)
            {
                        
                    dynamic format = usedRange.get_Range(IntToExcelColumnLetter(i) + (columnHeadersRowIs+1)  +":"+ IntToExcelColumnLetter(i) + "" +  nRows).NumberFormat;

                if (format is string)
                    knownColumnFormats.Add(i, format.ToString());
                else
                    listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Warning,
                            "Column " + i + " has mixed NumberFormats in it, it will be very slow to read"));
            }
                    
                    
            //if there is at least 1 column
            if (nColumns > 0)
            {
                Range rHeaders = (Range)usedRange.Rows[columnHeadersRowIs];

                List<int> skippedColumns = new List<int>();
                List<string> discardedData = new List<string>(); //any time we find data that is in an unnamed column we put it in here (usually totals etc)
                const int maxBitsOfDiscardedDataToCollect = 10; 

                Dictionary<int, int> legitColumns = new Dictionary<int, int>();
                            
                List<string> duplicateHeaders = new List<string>();

                //for each column in Excel add a new (untyped) column - we will strongly type later
                for (int c = 1; c <= nColumns; c++)
                {
                    string header = rHeaders.Columns[c].Text.ToString();
                             
                    if(string.IsNullOrWhiteSpace(header))
                        skippedColumns.Add(c);
                    else
                    {
                        legitColumns.Add(c,toReturn.Columns.Count);//index of the column in our data table

                        if (MakeHeaderNamesSane)
                            header = QuerySyntaxHelper.MakeHeaderNameSane(header);

                        //watch for duplicate columns
                        if(toReturn.Columns.Contains(header))
                            if (MakeHeaderNamesSane)
                                header = FlatFileColumnCollection.MakeHeaderUnique(header,toReturn.Columns, listener,this);
                            else
                            {
                                duplicateHeaders.Add(header);
                                continue;
                            }

                        toReturn.Columns.Add(header);
                    }
                }

                if(duplicateHeaders.Any())
                    throw new FlatFileLoadException("Found the following duplicate headers in file '" +_fileToLoad.File + "':" + string.Join(",",duplicateHeaders));

                for (int r = columnHeadersRowIs+1; r <= nRows; r++) //row 1 (Excel starts at row 1 always when it comes to used area)
                {
                    if(cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException("Stopped evaluating Excel file because cancellation token was triggered");

                    DataRow row = toReturn.Rows.Add();
                    bool rowIsEmpty = true;

                    for (int col = 1; col <= nColumns; col++) //for each cell in Excel data read
                    {
                        var value = data[r, col];

                        //if it is a skipped column (unamed)
                        if (skippedColumns.Contains(col))
                        {
                            if (!IsNull(value))
                                if (discardedData.Count < maxBitsOfDiscardedDataToCollect)
                                    discardedData.Add(value.ToString());
                        }
                        else//it is not a skipped column
                        {
                            if (!IsNull(value)) 
                                rowIsEmpty = false;//it is a good value in a good column

                            //see if it is a secret date!
                            if(value is double)
                            {
                                string numberFormat;
                                dynamic cell = null;

                                if (knownColumnFormats.ContainsKey(col))
                                    numberFormat = knownColumnFormats[col];
                                else
                                { 
                                    //very slow, if we have to use the value here
                                    cell = cells[r, col];
                                    numberFormat = cell.NumberFormat.ToString();
                                }

                                value = MakeTimeRelatedDecision((double)value, cells,numberFormat,cell,r,col);
                            }
                                    
                            //consult the dictionary and map the col index from excel to the good columns only indexes in the DataTable (e.g. excel col 1 maps to data table col 0 normally but if there is a blank col at start then excel column 2 will map to data table 0 instead)
                            row[legitColumns[col]] = value;

                            sketchyDoublesEvaluated++;

                            //don't spam them with events!
                            if (sketchyDoublesEvaluated%10000 == 0)
                                listener.OnProgress(this, new ProgressEventArgs("Evaluating Excel System.Double data types to see if they are dates or not", new ProgressMeasurement(sketchyDoublesEvaluated, ProgressType.Records), sw.Elapsed));
                        }
                    }

                    if (rowIsEmpty)
                        toReturn.Rows.Remove(row);
                    else
                        successfullyRead++;
                }

                //give them a final update about the number of sketchy doubles evaluated
                if (sketchyDoublesEvaluated>0)
                    listener.OnProgress(this, new ProgressEventArgs("Evaluating Excel System.Double data types to see if they are dates or not", new ProgressMeasurement(sketchyDoublesEvaluated, ProgressType.Records), sw.Elapsed));

                if(discardedData.Count>0)
                    listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning, "Discarded the following data (that was found in unamed columns):"+string.Join(",",discardedData)));

                foreach (int skippedIndex in skippedColumns)
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Column "+ skippedIndex + " did not have a header and so was not loaded"));
            }
            else
                throw new Exception("There were no columns in the file");
                * */

            if (toReturn.Columns.Count == 0)
                throw new FlatFileLoadException(string.Format("The Excel sheet '{0}' in workbook '{1}' is empty",worksheet.SheetName,_fileToLoad.File.Name));

            s.Stop();

            //if the user wants a column in the DataTable storing the filename loaded add it
            if (!string.IsNullOrWhiteSpace(AddFilenameColumnNamed))
            {
                toReturn.Columns.Add(AddFilenameColumnNamed);
                foreach (DataRow dataRow in toReturn.Rows)
                    dataRow[AddFilenameColumnNamed] = _fileToLoad.File.FullName;
            }

            
            return toReturn;
        }

        
        private object GetCellValue(ICell cell)
        {
            if (cell == null)
                return null;
            
            switch (cell.CellType)
            {
                case CellType.Unknown:
                    return cell.ToString();
                case CellType.Numeric:

                    //some numerics are actually dates/times
                    if (cell.CellStyle.DataFormat != 0)
                    {
                        string format = cell.CellStyle.GetDataFormatString();
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
                    return cell.StringCellValue;
                case CellType.Formula:
                    throw new Exception("Forumals are not supported by Excel Data Flow Source");
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
                        new CheckEventArgs("File extension " + _fileToLoad.File +
                                           " has an invalid extension:" + _fileToLoad.File.Extension +
                                           " (this class only accepts:" + string.Join(",", acceptedFileExtensions) + ")",
                            CheckResult.Fail));
                else
                    notifier.OnCheckPerformed(
                        new CheckEventArgs("File extension of file " + _fileToLoad.File.Name + " is acceptable",
                            CheckResult.Success));
            else
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "FlatFileToLoad (Pipeline Requirement) was not met (we weren't initialized with a file)",
                        CheckResult.Warning));

            var excelChecks = new ExcelInstalledChecker();
            excelChecks.Check(notifier);
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
}
