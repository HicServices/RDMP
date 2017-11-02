using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using DataExportLibrary.Interfaces.Pipeline;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using DataTable = System.Data.DataTable;

namespace DataExportLibrary.CohortCreationPipeline.Destinations
{
    public class BasicCohortDestination : IPluginCohortDestination
    {
        private string _privateIdentifier;
        private string _releaseIdentifier;
        private SqlConnection _con;
        private SqlTransaction _transaction;

        public ICohortCreationRequest Request { get; set; }
        SqlBulkCopy bulkCopy;
        private string _fk;

        Stopwatch sw = new Stopwatch();

        private int rowsSubmitted = 0;
        private bool _hasReleaseIdentifierColumn = false;

        [DemandsInitialization("The default behaviour of this system requires that both the private and release identifier be defined either by coming out of a source file or being allocated by an identifier allocation component further up the pipeline.  But if you set this to true then you allow the component to insert the private ID only (this will work only if you have an identity or a computed column for your release identifier e.g. a Hash of the private ID)",DemandType.Unspecified,true)]
        public bool AllowNullReleaseIdentifiers { get; set; }

        public virtual DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            try
            {
                sw.Start();
                if(_con == null)
                {
                    _con = (SqlConnection) DataAccessPortal.GetInstance().ExpectServer(Request.NewCohortDefinition.LocationOfCohort, DataAccessContext.DataLoad).GetConnection();
                    _con.Open();
                    _transaction = _con.BeginTransaction();

                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Started Transaction"));

                    Request.PushToServer(_con, _transaction);
                
                    if(Request.NewCohortDefinition.ID == null)
                        throw new Exception("We pushed the new cohort from the request object to the server (within transaction) but it's ID was not populated");

                    bulkCopy = new SqlBulkCopy(_con, SqlBulkCopyOptions.Default, _transaction);
                    bulkCopy.DestinationTableName = Request.NewCohortDefinition.LocationOfCohort.TableName;

                    if (toProcess.Columns.Contains(_privateIdentifier))
                        bulkCopy.ColumnMappings.Add(_privateIdentifier, _privateIdentifier);
                    else
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Could not find column called " + _privateIdentifier + " in chunk, columns were:" + string.Join(",", toProcess.Columns.OfType<DataColumn>().Select(c => c.ColumnName))));

                    if (toProcess.Columns.Contains(_releaseIdentifier))
                    {
                        bulkCopy.ColumnMappings.Add(_releaseIdentifier, _releaseIdentifier);
                        _hasReleaseIdentifierColumn = true;
                    }
                    else
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Could not find column called " + _releaseIdentifier + " in chunk, columns were:" + string.Join(",", toProcess.Columns.OfType<DataColumn>().Select(c => c.ColumnName))));
                    
                    bulkCopy.ColumnMappings.Add(_fk, _fk);
                }

                
                //add the ID as another column 
                toProcess.Columns.Add(_fk);
                foreach (DataRow dr in toProcess.Rows)
                    dr[_fk] = Request.NewCohortDefinition.ID;
                
                listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "About to calculate distinct identifiers.... Please Wait"));

                //get DISTINCT values
                DataTable dtToUpload;
                if (_hasReleaseIdentifierColumn)
                    dtToUpload = new DataView(toProcess).ToTable(true, _privateIdentifier, _releaseIdentifier, _fk);
                else
                    if (AllowNullReleaseIdentifiers)
                        dtToUpload = new DataView(toProcess).ToTable(true, _privateIdentifier,  _fk);//the chunk has no release identifier column, maybe it has a default or is an identity or something?
                    else
                        throw new MissingFieldException("There is no release identifier in the pipeline and AllowNullReleaseIdentifiers is false, does your database logic support inserting null release identifiers? if so then set AllowNullReleaseIdentifiers=true in the configuration of this pipeline component.");

                //warn user if the distinct count isn't the same as the non distinct
                if(dtToUpload.Rows.Count != toProcess.Rows.Count)
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "There were " + toProcess.Rows.Count +" identifiers in the current batch but only " + dtToUpload.Rows.Count + " DISTINCT pairs of private/release identifier"));

                UsefulStuff.BulkInsertWithBetterErrorMessages(bulkCopy, dtToUpload, null);
                rowsSubmitted += dtToUpload.Rows.Count;

                listener.OnProgress(this, new ProgressEventArgs("Uploading cohort " + Request.NewCohortDefinition, new ProgressMeasurement(rowsSubmitted,ProgressType.Records), sw.Elapsed));

                sw.Stop();
                return null;
            }
            catch (Exception )
            {
                //if something goes wrong don't leave the transaction hanging!
                _transaction.Dispose();
                throw;
            }
        }

        public virtual void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            //it exceptioned
            if (pipelineFailureExceptionIfAny != null)
                return;

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Attempting to commit transaction"));
            _transaction.Commit();
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Transaction committed"));
            _con.Close();
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Connection closed"));

            int id = Request.ImportAsExtractableCohort();

            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Cohort successfully comitted to destination and imported as an RDMP ExtractableCohort (ID="+id+" <- this is the ID of the reference pointer, the cohortDefinitionID of the actual cohort remains as you specified:"+Request.NewCohortDefinition.ID+")"));

        }

        public virtual void Abort(IDataLoadEventListener listener)
        {
            
        }

        public virtual void PreInitialize(ICohortCreationRequest value, IDataLoadEventListener listener)
        {
            Request = value;

            if(value == CohortCreationRequest.Empty)
                return;

            var target = Request.NewCohortDefinition.LocationOfCohort;
            _privateIdentifier = SqlSyntaxHelper.GetRuntimeName(target.PrivateIdentifierField);
            _releaseIdentifier = SqlSyntaxHelper.GetRuntimeName(target.ReleaseIdentifierField);
            _fk = Request.NewCohortDefinition.LocationOfCohort.DefinitionTableForeignKeyField;

            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "CohortCreationRequest spotted, we will look for columns " + _privateIdentifier + " and " + _releaseIdentifier + " (both of which must be in the pipeline before we will allow the cohort to be submitted)"));
            listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "id column in table " + Request.NewCohortDefinition.LocationOfCohort.TableName + " is " + Request.NewCohortDefinition.LocationOfCohort.DefinitionTableForeignKeyField));
        }


        public virtual void Check(ICheckNotifier notifier)
        {
            if (Request == CohortCreationRequest.Empty)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs("Cannot check because CohortCreationRequest is CohortCreationRequest.Empty",
                        CheckResult.Warning));
                return;
            }

            notifier.OnCheckPerformed(new CheckEventArgs("Cohort identifier columns are '"+ _privateIdentifier + "' (private) and '" + _releaseIdentifier + "' (release)", CheckResult.Success));
            
            Request.Check(notifier);
        }
    }
}
