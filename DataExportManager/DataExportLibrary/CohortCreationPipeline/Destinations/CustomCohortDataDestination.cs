using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using DataLoadEngine.DataFlowPipeline.Destinations;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.CohortCreationPipeline.Destinations
{
    public class CustomCohortDataDestination: IDataFlowDestination<DataTable>, IPipelineRequirement<ExtractableCohort>
    {
      
        private IExtractableCohort _cohort;
        IExternalCohortTable _externalCohortTable;
        private DiscoveredDatabase _database;

        public bool ThrowExceptionIfAnyPrivateIdentifiersAreMissingFromCohort { get; set; }
        public CustomCohortDataDestination()
        {
            ThrowExceptionIfAnyPrivateIdentifiersAreMissingFromCohort = false;
        }

        private int totalReadSoFar = 0;
        private Stopwatch swWritting = null;

        private bool _haveSentFirstBatch = false;
            
        public DataTable ProcessPipelineData( DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            //if there is no data in the batch
            if (toProcess == null)
                if (!_haveSentFirstBatch)
                    throw new Exception("First batch passed to this destination component was null! there is no data to submit apparently");
                else
                    return null;
            
            _database = _cohort.GetDatabaseServer();

            //if destination is not already full of data or otherwise unprepared
            if (!CheckDataTable(toProcess, listener))
                throw new Exception("Aborting load because CheckDataTable returned false");

            //start performance counter
            if (swWritting == null)
            {
                swWritting = new Stopwatch();
                swWritting.Start();
            }

            //send it to the db
            if (!_haveSentFirstBatch)
            {
                SetupDestination(toProcess, listener);
                _haveSentFirstBatch = true;
            }
            
            //send it to the destination
            _destination.ProcessPipelineData(toProcess, listener,cancellationToken);

            _tableName = _destination.TargetTableName;
            //report on performance to whoever is listening (e.g. UI)
            totalReadSoFar += toProcess.Rows.Count;
            
            listener.OnProgress(this, new ProgressEventArgs("Comitting rows to cohort " + _cohort + toProcess.TableName, new ProgressMeasurement(totalReadSoFar, ProgressType.Records), swWritting.Elapsed));
            
            return null;
        }

        private DataTableUploadDestination _destination;

        private void SetupDestination(DataTable toProcess, IDataLoadEventListener listener)
        {
            _database.Server.EnableAsync();

            _destination = new DataTableUploadDestination();

            //make sure to use explicit datatypes that match the cohort tables e.g. if client treates 01999 as varchar(50) then we should do the same in the custom table being created
            _destination.AddExplicitWriteType(SqlSyntaxHelper.GetRuntimeName(_cohort.GetPrivateIdentifier()),_cohort.GetPrivateIdentifierDataType());

            _destination.AllowResizingColumnsAtUploadTime = true;
            _destination.PreInitialize(_database,listener);
            
        }

        private string _privateIdentifier;
        private string _releaseIdentifier;
        private string _tableName;

        //private Dictionary<string, string> releaseToPrivateKeyMap;//for turning project identifiers on files back to private identifiers for integration with the cohort database (during data release these will be reanonymised no worries!)

        public bool CheckDataTable(DataTable toProcess, IDataLoadEventListener listener)
        {
            var syntaxHelper = _database.Server.Helper.GetQuerySyntaxHelper();

            try
            {
                //Ensure that private identifier field EXISTS in DataTable
                if(_privateIdentifier == null)
                    _privateIdentifier = syntaxHelper.GetRuntimeName(_cohort.GetPrivateIdentifier());

                //Ensure that the release identifier field DOES NOT EXIST in DataTable
                if (_releaseIdentifier == null)
                    _releaseIdentifier = syntaxHelper.GetRuntimeName(_cohort.GetReleaseIdentifier());

                bool filecontainsReleaseIdentifier = toProcess.Columns.Cast<DataColumn>().Any(dc => dc.ColumnName.Equals(_releaseIdentifier, StringComparison.InvariantCulture));
                bool fileContainsPrivateIdentifier = toProcess.Columns.Cast<DataColumn>().Any(dc => dc.ColumnName.Equals(_privateIdentifier, StringComparison.InvariantCulture));

                //special case, we have release identifier but not private identifier!- let's reverse the anonymisation so that we can add it and still link on private id like all the other datasets do
                if (filecontainsReleaseIdentifier && !fileContainsPrivateIdentifier)
                {
                    listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Warning,
                            "File contains Release Identifier (" + _releaseIdentifier + ")but not Private Identifier (" +
                            _privateIdentifier +
                            "), attempting to substitute release identifiers for private identifiers."));

                    
                    _cohort.ReverseAnonymiseDataTable(toProcess,listener,true);
                    
                }
                else
                {

                    //did we find private?
                    listener.OnNotify(this,
                        fileContainsPrivateIdentifier
                            ? new NotifyEventArgs(ProgressEventType.Information,
                                "Cohort Private Identifier " + _privateIdentifier + " found in DataTable")
                            : new NotifyEventArgs(ProgressEventType.Error,
                                "Cohort Private Identifier " + _privateIdentifier + " not found in DataTable"));
                    
                    if (!fileContainsPrivateIdentifier)
                        return false;
                    

                    //did we find release identifier?
                    listener.OnNotify(this,
                        filecontainsReleaseIdentifier
                            ? new NotifyEventArgs(ProgressEventType.Error,
                                "Cohort Release Identifier " + _releaseIdentifier + " found in DataTable")
                            : new NotifyEventArgs(ProgressEventType.Information,
                                "Cohort Release Identifier " + _releaseIdentifier + " not found in DataTable"));

                    if (filecontainsReleaseIdentifier)
                        return false;
                }

                //it obviously worked before so we are half way through a load I guess, no need to check for custom cohort data (infact theres probably a transaction lock preventing us from doing that anyway)
                if (_haveSentFirstBatch)
                    return true;

                //ensure filename is unique for this cohort
                string prospectiveTableName = toProcess.TableName;
                
                bool duplicateName = _cohort.GetDatabaseServer().DiscoverTables(true).Any(t => t.GetRuntimeName().Equals(prospectiveTableName,StringComparison.CurrentCultureIgnoreCase));

                listener.OnNotify(this,duplicateName
                    ? new NotifyEventArgs(ProgressEventType.Error,
                        "A Custom Table already exists called " + prospectiveTableName)
                    : new NotifyEventArgs(ProgressEventType.Information, 
                        "Custom Table " + prospectiveTableName + " is unique for cohort " + _cohort +
                        " and can be used as a custom table name"));

                if (duplicateName)
                    return false;
                
                foreach (DataColumn column in toProcess.Columns)
                {
                    try
                    {
                        syntaxHelper.GetRuntimeName(column.ColumnName);
                    }
                    catch (Exception e)
                    {
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, 
                            "SqlSyntaxHelper does not like the column name '" + column.ColumnName +"', double click to find out why", e));
                    }

                    try
                    {
                        syntaxHelper.TypeTranslater.GetSQLDBTypeForCSharpType(new DatabaseTypeRequest(column.DataType));
                    }
                    catch (Exception e)
                    {
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "As a dry run we asked the destination database provider what datatype to use for the csharp column datatype '"+column.DataType+"' but it threw an Exception", e));
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Last minute checks (just before committing to the database) failed on DataTable called "+toProcess.TableName+" ,check Exception for details",e));
                return false;
            }
        }

        private string[] listPrivateIdentifierMismatches(DiscoveredDatabase db, string tableName,DbConnection con)
        {
            string privateIdentifier = SqlSyntaxHelper.GetRuntimeName(_cohort.GetPrivateIdentifier());

            DbCommand cmd = db.Server.GetCommand(
                string.Format("Select {0} from " + tableName + " WHERE {0} not in (SELECT {0} from {1} WHERE {2}) AND {0} is not null", privateIdentifier
                , _externalCohortTable.TableName
                , _cohort.WhereSQL())
            ,con);

            DbDataReader r = cmd.ExecuteReader();

            var toReturn = new List<string>();

            while (r.Read())
                toReturn.Add(r[privateIdentifier].ToString());

            r.Close();

            return toReturn.ToArray();
        }

     

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            if (pipelineFailureExceptionIfAny == null)
            {
                _destination.Dispose(listener, null);

                //this will hold all the identifiers that are found in the file that do not match anyone in the cohort
                string[] missingIdentifiers;

                listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information,
                        "Upload of CustomData succeeded into table " + _tableName +
                        ", now preparing to check for missing identifiers and record the location of the table in the CustomData table of the Cohort"));

                using (var con = _database.Server.GetConnection())
                {
                    con.Open();

                    missingIdentifiers = listPrivateIdentifierMismatches(_database, _tableName, con);

                    _cohort.RecordNewCustomTable(_database.Server, _tableName, con, null);


                    //if there were missing identifiers we should complain about them
                    if (missingIdentifiers.Any())
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Warning,
                                missingIdentifiers.Length + " Missing Identifiers were Found:(" +
                                string.Join(",", missingIdentifiers) + ")"));
                    else
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Information, "Did not find any missing identifiers"));
                }
            }
            else
            {
                if(_destination != null)
                    _destination.Dispose(listener, pipelineFailureExceptionIfAny);
            }
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public void PreInitialize(ExtractableCohort value, IDataLoadEventListener listener)
        {
            _cohort = value;
            _externalCohortTable = _cohort.ExternalCohortTable;
        }
    }
}
