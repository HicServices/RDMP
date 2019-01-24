using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Reports;
using FAnsi.Discovery;
using LoadModules.Generic.Checks;
using LoadModules.Generic.DataFlowSources.SubComponents;
using LoadModules.Generic.Exceptions;
using Microsoft.Office.Interop.Excel;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using DataTable = System.Data.DataTable;
using Excel = Microsoft.Office.Interop.Excel; 

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
            
            Excel.Application excelApp = new Excel.Application();
            excelApp.Visible = false;
            excelApp.Interactive = false;
            excelApp.ScreenUpdating = false;

            try
            {
                Workbook wb = excelApp.Workbooks.Open(_fileToLoad.File.FullName);
                try
                {
                    Worksheet worksheet = null;

                    //if the user hasn't picked one, use the first
                    if(string.IsNullOrWhiteSpace(WorkSheetName))
                        worksheet = wb.Worksheets.Cast<Worksheet>().First();
                    else
                        worksheet = (Worksheet)wb.Worksheets[WorkSheetName];//else use the named sheet

                    var usedRange = worksheet.UsedRange;
                    int nColumns = usedRange.Columns.Count;
                    int nRows = usedRange.Rows.Count;

                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Excel sheet " + worksheet.Name + " contains " + nColumns + " columns and " + nRows + " rows"));

                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to fetch all values from Excel file, this might take a while"));

                    object[,] data = usedRange.Value;
                    
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "successfully fetched " + string.Format("{0:n0}",data.Length)+ " array elements"));
                    
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
                }
                finally
                {
                    wb.Close(false);
                }
            }
            finally
            {
                // restore interactivity, GUI updates and calculation
                excelApp.Interactive = true;
                excelApp.ScreenUpdating = true;
                excelApp.Quit();

                // release COM object
                Marshal.FinalReleaseComObject(excelApp);
                GC.Collect();
            }
            
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
