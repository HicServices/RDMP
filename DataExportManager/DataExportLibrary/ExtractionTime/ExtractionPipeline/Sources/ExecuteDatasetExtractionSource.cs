using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.Repositories;
using DataLoadEngine.DataFlowPipeline.Sources;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using IContainer = CatalogueLibrary.Data.IContainer;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources
{
    /// <summary>
    /// Executes a single Dataset extraction into a flat file and orchestrates the creation of meta data word file
    /// </summary>
    [Description("The default source for data extraction, performs a join between a dataset and a cohort on the same server and substitutes private identifier for release identifier in datasets entering the pipeline")]
    public class ExecuteDatasetExtractionSource : IPluginDataFlowSource<DataTable>, IPipelineRequirement<IExtractCommand>
    {
        //Request is either for one of these
        public  ExtractDatasetCommand Request { get; protected set; }

        //or one of these
        private ExtractCohortCustomTableCommand _customTablePair;
        private bool _customTablePairDataSent = false;

        public const string AuditTaskName = "DataExtraction";

        private readonly List<string> _extractionIdentifiersidx = new List<string>();
        
        private bool _cancel = false;
        
        ICatalogue _catalogue;

        protected const string ValidationColumnName = "RowValidationResult";

        public ExtractionTimeValidator ExtractionTimeValidator { get; protected set; }
        public Exception ValidationFailureException { get; protected set; }

        public HashSet<object> UniqueReleaseIdentifiersEncountered { get; set; }

        public ExtractionTimeTimeCoverageAggregator ExtractionTimeTimeCoverageAggregator { get; set; }

        public CumulativeExtractionResults CumulativeExtractionResults { get; protected set; }
        
        [DemandsInitialization("Determines the systems behaviour when an extraction query returns 0 rows.  Default (false) is that an error is reported.  If set to true (ticked) then instead a DataTable with 0 rows but all the correct headers will be generated usually resulting in a headers only 0 line/empty extract file")]
        public bool AllowEmptyExtractions { get; set; }

        [DemandsInitialization("Batch size, number of records to read from source before releasing it into the extraction pipeline",DemandType.Unspecified,10000)]
        public int BatchSize { get; set; }
        
        /// <summary>
        /// This is a dictionary containing all the CatalogueItems used in the query, the underlying datatype in the origin database and the
        /// actual datatype that was output after the transform operation e.g. a varchar(10) could be converted into a bona fide DateTime which
        /// would be an sql Date.  Finally
        /// a recommended SqlDbType is passed back.
        /// </summary>
        public Dictionary<ExtractableColumn, ExtractTimeTransformationObserved> ExtractTimeTransformationsObserved;
        private DbDataCommandDataFlowSource _hostedSource;

        private bool _initialized = false;
        private void Initialize(ExtractDatasetCommand request)
        {
            Request = request;
            
            if (request == ExtractDatasetCommand.EmptyCommand)
                return;

            _timeSpentValidating = new Stopwatch();
            _timeSpentCalculatingDISTINCT = new Stopwatch();
            _timeSpentBuckettingDates = new Stopwatch();

            Request.ColumnsToExtract.Sort();//ensure they are in the right order so we can record the release identifiers
        
            //if we have a cached builder already
            if(request.QueryBuilder == null)
                request.GenerateQueryBuilder();
            
            foreach (ReleaseIdentifierSubstitution substitution in Request.ReleaseIdentifierSubstitutions)
                _extractionIdentifiersidx.Add(substitution.GetRuntimeName());
            
            UniqueReleaseIdentifiersEncountered = new HashSet<object>();

            _catalogue = request.Catalogue;

            if (!string.IsNullOrWhiteSpace(_catalogue.ValidatorXML))
                ExtractionTimeValidator = new ExtractionTimeValidator(_catalogue, request.ColumnsToExtract);
          
            //if there is a time periodicity ExtractionInformation (AND! it is among the columns the user selected to be extracted)
            if (_catalogue.TimeCoverage_ExtractionInformation_ID != null && request.ColumnsToExtract.Cast<ExtractableColumn>().Any(c => c.CatalogueExtractionInformation_ID == _catalogue.TimeCoverage_ExtractionInformation_ID))
                ExtractionTimeTimeCoverageAggregator = new ExtractionTimeTimeCoverageAggregator(_catalogue, request.ExtractableCohort);
            else
                ExtractionTimeTimeCoverageAggregator = null;
         

            _initialized = true;
        }

        public bool WasCancelled
        {
            get { return _cancel; }
        }

        
        private Stopwatch _timeSpentValidating;
        private int _rowsValidated = 0;

        private Stopwatch _timeSpentCalculatingDISTINCT;
        private Stopwatch _timeSpentBuckettingDates;
        private int _rowsBucketted = 0;
        
        private bool firstChunk = true;
        private int _rowsRead;

        public virtual DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (_customTablePair != null)
                return _customTablePairDataSent ? null : SendCustomTableData(listener);


            if (!_initialized)
                 listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Component has not been initialized before being asked to GetChunk(s)"));

            if(_cancel)
                throw new Exception("User cancelled data extraction");

        
           if (_hostedSource == null)
            {
               //unless we are checking, start auditing
               if(!_testMode)
                    StartAudit(Request.QueryBuilder.SQL);

               if(Request.DatasetBundle.DataSet.DisableExtraction)
                   throw new Exception("Cannot extract " + Request.DatasetBundle.DataSet + " because DisableExtraction is set to true");

                _hostedSource = new DbDataCommandDataFlowSource(GetCommandSQL(listener),
                    "ExecuteDatasetExtraction " + Request.DatasetBundle.DataSet,
                    _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.DataExport, false).Builder, _testMode ? 30 : 50000);

                _hostedSource.AllowEmptyResultSets = AllowEmptyExtractions;
                _hostedSource.BatchSize = BatchSize;
            }

            DataTable chunk=null;

            Thread t = new Thread(() =>
            {
                try
                {
                    chunk = _hostedSource.GetChunk(listener, cancellationToken);
                }
                catch (Exception e)
                {
                    listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Read from source failed",e));
                }
            });
            t.Start();

            bool haveAttemptedCancelling = false;
            while (t.IsAlive)
            {
                if(cancellationToken.IsCancellationRequested && _hostedSource.cmd != null && !haveAttemptedCancelling)
                {
                    _hostedSource.cmd.Cancel();//cancel the database command
                    haveAttemptedCancelling = true;
                }
                Thread.Sleep(100);
            }
            
            if(cancellationToken.IsCancellationRequested)
                throw new Exception("Data read cancelled because our cancellationToken was set, aborting data reading");
            
            //if the first chunk is null
            if (firstChunk && chunk == null)
                throw new Exception("There is no data to load, query returned no rows, query was:" + Environment.NewLine + Request.QueryBuilder.SQL);
            
            //not the first chunk anymore
            firstChunk = false;

            //data exhausted
            if (chunk == null)
            {
                listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Data exhausted after reading " + _rowsRead + " rows of data ("+UniqueReleaseIdentifiersEncountered.Count + " unique release identifiers seen)"));
                return null;
            }

            _rowsRead += chunk.Rows.Count;
            //chunk will have datatypes for all the things in the buffer so we can populate our dictionary of facts about what columns/catalogue items have spontaneously changed name/type etc
            if(ExtractTimeTransformationsObserved == null)
                GenerateExtractionTransformObservations(chunk);


            //see if the SqlDataReader has a column with the same name as the ReleaseIdentifierSQL (if so then we can use it to count the number of distinct subjects written out to the csv)
            bool includesReleaseIdentifier = _extractionIdentifiersidx.Count > 0;


            //first line - lets see what columns we wrote out
            //looks at the buffer and computes any transforms performed on the column
                    

            _timeSpentValidating.Start();
            //build up the validation report (Missing/Wrong/Etc) - this has no mechanical effect on the extracted data just some metadata that goes into a flat file
            if (ExtractionTimeValidator != null && Request.IncludeValidation)
                try
                {
                    chunk.Columns.Add(ValidationColumnName);

                    ExtractionTimeValidator.Validate(chunk, ValidationColumnName);

                    _rowsValidated += chunk.Rows.Count;
                    listener.OnProgress(this,new ProgressEventArgs("Validation",new ProgressMeasurement(_rowsValidated,ProgressType.Records), _timeSpentValidating.Elapsed));
                }
                catch (Exception ex)
                {
                    listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Could not validate data chunk",ex));
                    ValidationFailureException = ex;
                    ExtractionTimeValidator = null;
                }
            _timeSpentValidating.Stop();
            
            _timeSpentBuckettingDates.Start();
            if (ExtractionTimeTimeCoverageAggregator != null)
            {
                _rowsBucketted += chunk.Rows.Count;

                foreach (DataRow row in chunk.Rows)
                    ExtractionTimeTimeCoverageAggregator.ProcessRow(row);
                
                listener.OnProgress(this, new ProgressEventArgs("Bucketting Dates",new ProgressMeasurement(_rowsBucketted,ProgressType.Records),_timeSpentCalculatingDISTINCT.Elapsed ));
            }
            _timeSpentBuckettingDates.Stop();

            _timeSpentCalculatingDISTINCT.Start();
            //record unique release identifiers found
            if (includesReleaseIdentifier)
                foreach (string idx in _extractionIdentifiersidx)
                {
                    foreach (DataRow r in chunk.Rows)
                    {
                        if (r[idx] == DBNull.Value)
                            if (_extractionIdentifiersidx.Count == 1)
                                throw new Exception("Null release identifier found in extract of dataset " + Request.DatasetBundle.DataSet);
                            else
                                continue; //there are multiple extraction identifiers thats fine if one or two are null

                        if (!UniqueReleaseIdentifiersEncountered.Contains(r[idx]))
                            UniqueReleaseIdentifiersEncountered.Add(r[idx]);
                    }

                     listener.OnProgress(this,new ProgressEventArgs("Calculating Distinct Release Identifiers",new ProgressMeasurement(UniqueReleaseIdentifiersEncountered.Count, ProgressType.Records),_timeSpentCalculatingDISTINCT.Elapsed ));
                }
            _timeSpentCalculatingDISTINCT.Stop();


            //if it is test mode reset the host so it is ready to go again if called a second time
            if (_testMode)
                _hostedSource = null;


            return chunk;

        }

        private DataTable SendCustomTableData(IDataLoadEventListener listener)
        {
            _customTablePairDataSent = true;
            try
            {
                var sql = _customTablePair.ExtractableCohort.GetCustomTableExtractionSQL(_customTablePair.TableName);

                using (var con = _customTablePair.ExtractableCohort.GetDatabaseServer().Server.GetConnection())
                {
                    con.Open();
                    var dt = new DataTable();
                    var cmd = DatabaseCommandHelper.GetCommand(sql, con);
                    DatabaseCommandHelper.GetDataAdapter(cmd).Fill(dt);

                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Read "+dt.Rows.Count+" rows from custom table " + _customTablePair.TableName));
                    return dt;
                }
            }
            catch (Exception e)
            {
                listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error,"Failed to extract custom table " + _customTablePair.TableName,e));
                throw;
            }
        }

        private void GenerateExtractionTransformObservations(DataTable chunk)
        {
            ExtractTimeTransformationsObserved = new Dictionary<ExtractableColumn, ExtractTimeTransformationObserved>();

            //create the Types dictionary
            foreach (ExtractableColumn column in Request.ColumnsToExtract)
            {
                ExtractTimeTransformationsObserved.Add(column, new ExtractTimeTransformationObserved());

                //record catalogue information about what it is supposed to be.
                if (!column.HasOriginalExtractionInformationVanished())
                {
                    var extractionInformation = column.CatalogueExtractionInformation;

                    //what the catalogue says it is
                    ExtractTimeTransformationsObserved[column].DataTypeInCatalogue =
                        extractionInformation.ColumnInfo.Data_type;
                    ExtractTimeTransformationsObserved[column].CatalogueItem = extractionInformation.CatalogueItem;

                    //what it actually is
                    if (chunk.Columns.Contains(column.GetRuntimeName()))
                    {
                        ExtractTimeTransformationsObserved[column].FoundAtExtractTime = true;
                        ExtractTimeTransformationsObserved[column].DataTypeObservedInRuntimeBuffer =
                            chunk.Columns[column.GetRuntimeName()].DataType;
                    }
                    else
                        ExtractTimeTransformationsObserved[column].FoundAtExtractTime = false;
                }
            }
        }

        private string GetCommandSQL( IDataLoadEventListener listener)
        {
            string sql = Request.QueryBuilder.SQL;

            sql = HackExtractionSQL(sql,listener);

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "/*Decided on extraction SQL:*/"+Environment.NewLine + sql));
            
            return sql;
        }

        public virtual string HackExtractionSQL(string sql, IDataLoadEventListener listener)
        {
            return sql;

        }
        
        private void StartAudit(string sql)
        {

            var dataExportRepo = ((DataExportRepository) Request.RepositoryLocator.DataExportRepository);

            var previousAudit = dataExportRepo.GetAllCumulativeExtractionResultsFor(Request.Configuration, Request.DatasetBundle.DataSet).ToArray();

            //delete old audit records
            foreach (CumulativeExtractionResults audit in previousAudit)
                audit.DeleteInDatabase();

            CumulativeExtractionResults = new CumulativeExtractionResults(dataExportRepo, Request.Configuration, Request.DatasetBundle.DataSet, sql);

            string filterDescriptions = RecursivelyListAllFilterNames(Request.Configuration.GetFilterContainerFor(Request.DatasetBundle.DataSet));

            CumulativeExtractionResults.FiltersUsed = filterDescriptions.TrimEnd(',');
            CumulativeExtractionResults.SaveToDatabase();
        }

        private string RecursivelyListAllFilterNames(IContainer filterContainer)
        {
            if (filterContainer == null)
                return "";

            string toReturn = "";

            if (filterContainer.GetSubContainers() != null)
                foreach (IContainer subContainer in filterContainer.GetSubContainers())
                    toReturn += RecursivelyListAllFilterNames(subContainer);

            if(filterContainer.GetFilters() != null)
                foreach (IFilter f in filterContainer.GetFilters())
                    toReturn += f.Name +',';

            return toReturn;
        }
        
        public virtual void Dispose(IDataLoadEventListener job, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public virtual DataTable TryGetPreview()
        {
            if(Request == ExtractDatasetCommand.EmptyCommand)
                return new DataTable();

            DataTable toReturn = new DataTable();
            var server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.DataExport,false);

            using (var con = server.GetConnection())
            {
                con.Open();

                var da = server.GetDataAdapter(Request.QueryBuilder.SQL, con);

                //get up to 1000 records
                da.Fill(0, 1000, toReturn);
                
                con.Close();
            }

            return toReturn;
        }

        public void PreInitialize(IExtractCommand value, IDataLoadEventListener listener)
        {
            if (value is ExtractDatasetCommand)
                Initialize(value as ExtractDatasetCommand);
            else
                _customTablePair = value as ExtractCohortCustomTableCommand;
        }
        
        public virtual void Check(ICheckNotifier notifier)
        {
            if (Request == ExtractDatasetCommand.EmptyCommand)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Request is ExtractDatasetCommand.EmptyCommand, checking will not be carried out",CheckResult.Warning));
                return;
            }

            if (Request == null)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("ExtractionRequest has not been set", CheckResult.Fail));
                return;
            }

            notifier.OnCheckPerformed(new CheckEventArgs("ExtractionRequest is set, about to generate test extraction SQL", CheckResult.Success));
                
            if(Request.LimitationSql == null || (!Request.LimitationSql.Trim().StartsWith("TOP ")))
                notifier.OnCheckPerformed(
                    new CheckEventArgs("Request did not have any LimitationSql or it did not start with TOP",
                        CheckResult.Warning));
                
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "Extraction SQL for Checking (may differ from runtime extraction SQL e.g. have TOP 100 in it) is :" +
                    Request.QueryBuilder.SQL, CheckResult.Success));


            notifier.OnCheckPerformed(new CheckEventArgs("About to run GetChunk ",CheckResult.Success));
            try
            {
                _testMode = true;
                DataTable dt = GetChunk(new FromCheckNotifierToDataLoadEventListener(notifier), new GracefulCancellationToken());

                if (dt == null)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs("The above SQL returned no rows",
                            CheckResult.Fail));
                else
                    notifier.OnCheckPerformed(
                        new CheckEventArgs("successfully read " + dt.Rows.Count + " rows using the above SQL",
                            CheckResult.Success));

            }
            catch (Exception e)
            {
                if (e.Message.Contains("Timeout"))
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Above SQL resulted in a timeout, it is likely that your pipeline is intact but targets a slow table that is not worth previewing",
                            CheckResult.Warning, e));
                else
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Could not read data from the source (above SQL failed)",
                            CheckResult.Fail, e));
                }
            }
            finally
            {
                _testMode = false;
            }

            
        }

        private bool _testMode;
        
    }
}
