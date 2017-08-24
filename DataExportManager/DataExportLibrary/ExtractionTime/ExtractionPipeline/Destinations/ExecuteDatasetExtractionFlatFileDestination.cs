using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.ExtractionTime.UserPicks;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.FileOutputFormats;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataLoadEngine.DataFlowPipeline;
using HIC.Logging;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations
{

    public enum ExecuteExtractionToFlatFileType
    {
        Access,
        CSV
    }

    [Description("The Extraction target for DataExportManager into a Flat file (e.g. CSV), this should only be used by ExtractionPipelineHost as it is the only class that knows how to correctly call PreInitialize ")]
    public class ExecuteDatasetExtractionFlatFileDestination : IExecuteDatasetExtractionDestination
    {
        private FileOutputFormat _output;
        public TableLoadInfo TableLoadInfo { get; private set; }
        
        public DirectoryInfo DirectoryPopulated { get; private set; }
        
        public int SeparatorsStrippedOut { get; set; }
        public string OutputFile { get; private set; }
        public int LinesWritten { get; private set; }
        Stopwatch stopwatch = new Stopwatch();


        [DemandsInitialization("The date format to output all datetime fields in e.g. dd/MM/yyyy for uk format yyyy-MM-dd for something more machine processable, see https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx", DemandType.Unspecified, "yyyy-MM-dd", Mandatory = true)]
        public string DateFormat { get; set; }


        [DemandsInitialization("The kind of flat file to generate for the extraction", DemandType.Unspecified, ExecuteExtractionToFlatFileType.CSV)]
        public ExecuteExtractionToFlatFileType FlatFileType { get; set; }

        public ExtractionTimeValidator ExtractionTimeValidator { get; set; }
        
        private bool haveOpened = false;
        private bool haveWrittenBundleContents = false;

        public DataTable ProcessPipelineData( DataTable toProcess, IDataLoadEventListener job, GracefulCancellationToken cancellationToken)
        {
            if (!haveWrittenBundleContents && _request is ExtractDatasetCommand)
                WriteBundleContents(((ExtractDatasetCommand)_request).DatasetBundle, job, cancellationToken);

            stopwatch.Start();
            if (!haveOpened)
            {
                haveOpened = true;
                _output.Open();
                _output.WriteHeaders(toProcess);
                LinesWritten = 0;

                //create an audit object
                TableLoadInfo = new TableLoadInfo(_dataLoadInfo, "", OutputFile, new DataSource[] { new DataSource(_request.DescribeExtractionImplementation(), DateTime.Now) }, -1);
            }

            foreach (DataRow row in toProcess.Rows)
            {
                _output.Append(row);

                LinesWritten++;

                if(TableLoadInfo.IsClosed)
                    throw new Exception("TableLoadInfo was closed so could not write number of rows (" + LinesWritten +") to audit object - most likely the extraction crashed?");
                else
                    TableLoadInfo.Inserts = LinesWritten;

                job.OnProgress(this,new ProgressEventArgs("Write to file " + OutputFile,new ProgressMeasurement(LinesWritten,ProgressType.Records), stopwatch.Elapsed));
            }
            
            stopwatch.Stop();
            _output.Flush();

            return null;
        }

        private void WriteBundleContents(IExtractableDatasetBundle datasetBundle, IDataLoadEventListener job, GracefulCancellationToken cancellationToken)
        {
            var rootDir = _request.GetExtractionDirectory();
            var supportingSQLFolder = new DirectoryInfo(Path.Combine(rootDir.FullName, SupportingSQLTable.ExtractionFolderName));
            var lookupDir = rootDir.CreateSubdirectory("Lookups");
                    
            //extract the documents
            foreach (SupportingDocument doc in datasetBundle.Documents)
                datasetBundle.States[doc] = TryExtractSupportingDocument(rootDir, doc, job)
                    ? ExtractCommandState.Completed
                    : ExtractCommandState.Crashed;

            //extract supporting SQL
            foreach (SupportingSQLTable sql in datasetBundle.SupportingSQL)
                datasetBundle.States[sql] = TryExtractSupportingSQLTable(supportingSQLFolder, _request.Configuration, sql, job, _dataLoadInfo)
                    ? ExtractCommandState.Completed
                    : ExtractCommandState.Crashed;

            //extract lookups
            foreach (BundledLookupTable lookup in datasetBundle.LookupTables)
            {
                try
                {
                    job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "About to extract lookup " + lookup));

                    var server = DataAccessPortal.GetInstance().ExpectServer(lookup.TableInfo, DataAccessContext.DataExport);
                
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    //extracts all of them
                    ExtractTableVerbatim extractTableVerbatim = new ExtractTableVerbatim(server, new []{lookup.TableInfo.Name}, lookupDir, _request.Configuration.Separator,DateFormat);
                    int linesWritten = extractTableVerbatim.DoExtraction();
                    sw.Stop();
                    job.OnProgress(this,new ProgressEventArgs("Lookup "+ lookup,new ProgressMeasurement(linesWritten,ProgressType.Records),sw.Elapsed));

                    datasetBundle.States[lookup] = ExtractCommandState.Completed;
                }
                catch (Exception e)
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Error occurred trying to extract lookup " + lookup + " on server "  + lookup.TableInfo.Server,e));
                    
                    datasetBundle.States[lookup] = ExtractCommandState.Crashed;
                }
            }

            haveWrittenBundleContents = true;
        }



        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            CloseFile(listener);
        }

        public void Abort(IDataLoadEventListener listener)
        {
            CloseFile(listener);
        }

        private bool _fileAlreadyClosed = false;

        private void CloseFile(IDataLoadEventListener listener)
        {
            //we never even started or have already closed
            if (!haveOpened || _fileAlreadyClosed )
                return;

            _fileAlreadyClosed = true;

            try
            {
                //whatever happens in the writing block, make sure to at least attempt to close off the file
                _output.Close();
                GC.Collect(); //prevents file locks from sticking around

                //close audit object - unless it was prematurely closed e.g. by a failure somewhere
                if (!TableLoadInfo.IsClosed)
                    TableLoadInfo.CloseAndArchive();
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Error when trying to close csv file", e));
            }
        }

        private IExtractCommand _request ;
        private DataLoadInfo _dataLoadInfo;

        public void PreInitialize(IExtractCommand request, IDataLoadEventListener listener)
        {
            _request = request;

            LinesWritten = 0;

            DirectoryPopulated = request.GetExtractionDirectory();

            switch (FlatFileType)
            {
                case ExecuteExtractionToFlatFileType.Access:
                    OutputFile = Path.Combine(DirectoryPopulated.FullName, request + ".accdb");
                      _output = new MicrosoftAccessDatabaseFormat(OutputFile);
                    break;
                case ExecuteExtractionToFlatFileType.CSV:
                    OutputFile = Path.Combine(DirectoryPopulated.FullName, request + ".csv");

                    bool includeValidation = request is ExtractDatasetCommand && ((ExtractDatasetCommand)request).IncludeValidation;

                    _output = new CSVOutputFormat(OutputFile, request.Configuration.Separator, DateFormat, includeValidation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Setup data extraction destination as " + OutputFile + " (will not exist yet)"));
        }

        public void PreInitialize(DataLoadInfo value, IDataLoadEventListener listener)
        {
            _dataLoadInfo = value;
        }

        public string GetDestinationDescription()
        {
            return OutputFile;
        }

        public void ExtractGlobals(Project project, ExtractionConfiguration configuration, GlobalsBundle globals, IDataLoadEventListener listener, DataLoadInfo dataLoadInfo)
        {
            ExtractionDirectory targetDirectory = new ExtractionDirectory(project.ExtractionDirectory, configuration);
            DirectoryInfo globalsDirectory = targetDirectory.GetGlobalsDirectory();
            
            foreach (var doc in globals.Documents)
                globals.States[doc] = TryExtractSupportingDocument(globalsDirectory, doc, listener)
                    ? ExtractCommandState.Completed
                    : ExtractCommandState.Crashed;
            
            foreach (var sql in globals.SupportingSQL)
                globals.States[sql] = TryExtractSupportingSQLTable(globalsDirectory, configuration, sql, listener, dataLoadInfo)
                    ? ExtractCommandState.Completed
                    : ExtractCommandState.Crashed;
        }

        
        private bool TryExtractSupportingDocument(DirectoryInfo directory, SupportingDocument doc, IDataLoadEventListener listener)
        {
            SupportingDocumentsFetcher fetcher = new SupportingDocumentsFetcher(null);

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Preparing to copy "+doc+" to directory " + directory.FullName));
            try
            {
                fetcher.ExtractToDirectory(directory, doc);
                return true;
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Failed to copy file " + doc + " to directory " + directory.FullName, e));
                return false;
            }
        }

        private bool TryExtractSupportingSQLTable(DirectoryInfo directory, IExtractionConfiguration configuration, SupportingSQLTable sql, IDataLoadEventListener listener, DataLoadInfo dataLoadInfo)
        {
            try
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Preparing to extract Supporting SQL " + sql + " to directory " + directory.FullName));

                Stopwatch sw = new Stopwatch();
                sw.Start();

                //start auditing it as a table load
                string target = Path.Combine(directory.FullName, sql.Name + ".csv");
                var tableLoadInfo = dataLoadInfo.CreateTableLoadInfo("", target, new[] { new DataSource(sql.SQL, DateTime.Now) }, -1);

                int sqlLinesWritten = new ExtractTableVerbatim(sql.GetServer(),
              sql.SQL, sql.Name,
              directory,
              configuration
                  .Separator,
                  DateFormat)
              .DoExtraction();

                sw.Stop();

                //end auditing it
                tableLoadInfo.Inserts = sqlLinesWritten;
                tableLoadInfo.CloseAndArchive();

                listener.OnProgress(this, new ProgressEventArgs("Extract " + sql, new ProgressMeasurement(sqlLinesWritten, ProgressType.Records), sw.Elapsed));
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Extracted " + sqlLinesWritten + " records from SupportingSQL " + sql + " into directory " + directory.FullName));
                return true;
            }
            catch (Exception e)
            {
                if (e is SqlException)
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Failed to run extraction SQL (make sure to fully specify all database/table/column objects completely):" + Environment.NewLine + sql.SQL, e));
                else
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Failed to extract " + sql + " into directory " + directory.FullName, e));

                return false;
            }
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                string result = DateTime.Now.ToString(DateFormat);
                notifier.OnCheckPerformed(new CheckEventArgs("DateFormat '" + DateFormat + "' is valid, dates will look like:" + result, CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("DateFormat '" + DateFormat + "' was invalid",CheckResult.Fail, e));
            }
        }
    }
}