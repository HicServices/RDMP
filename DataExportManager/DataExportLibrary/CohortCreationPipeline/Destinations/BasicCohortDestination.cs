using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary.CohortCreationPipeline.Destinations.IdentifierAllocation;
using DataExportLibrary.Interfaces.Pipeline;

using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Xceed.Words.NET;
using DataTable = System.Data.DataTable;

namespace DataExportLibrary.CohortCreationPipeline.Destinations
{
    /// <summary>
    /// Destination component for Cohort Creation Pipelines, responsible for bulk inserting patient identifiers into the cohort database specified in the
    /// ICohortCreationRequest.  This 
    /// </summary>
    public class BasicCohortDestination : IPluginCohortDestination
    {
        private string _privateIdentifier;
        private string _releaseIdentifier;
        
        public ICohortCreationRequest Request { get; set; }
        
        private string _fk;
        
        [DemandsInitialization("Set one of these if you plan to upload lists of patients and want RDMP to automatically allocate an anonymous ReleaseIdentifier", TypeOf = typeof(IAllocateReleaseIdentifiers),DefaultValue=typeof(GuidReleaseIdentifierAllocator))]
        public Type ReleaseIdentifierAllocator { get; set; }

        private IAllocateReleaseIdentifiers _allocator = null;
        
        readonly Dictionary<object, object> _cohortDictionary = new Dictionary<object, object>();

        public virtual DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            //if user has picked an allocator get an instance
            if (ReleaseIdentifierAllocator != null && _allocator == null)
                _allocator = (IAllocateReleaseIdentifiers) new ObjectConstructor().Construct(ReleaseIdentifierAllocator);
            
            if(!toProcess.Columns.Contains(_privateIdentifier))
                throw new Exception("Could not find column called " + _privateIdentifier + " in chunk, columns were:" + string.Join(",", toProcess.Columns.OfType<DataColumn>().Select(c => c.ColumnName)));
                
            //we don't have a release identifier column
            if (!toProcess.Columns.Contains(_releaseIdentifier))
                foreach (DataRow row in toProcess.Rows)
                {
                    //so we have to allocate all of them with the allocator

                    //get the private cohort id
                    var priv = row[_privateIdentifier];
                        
                    //already handled these folks?
                    if (_cohortDictionary.ContainsKey(priv) || IsNull(priv)) 
                        continue;
                       
                    //no, allocate them an ID (or null if there is no allocator)
                    _cohortDictionary.Add(priv,_allocator == null? DBNull.Value : _allocator.AllocateReleaseIdentifier(priv));
                }
            else
            {
                bool foundUserSpecifiedReleaseIds = false;

                foreach (DataRow row in toProcess.Rows)
                {
                    //get the private cohort id
                    var priv = row[_privateIdentifier];
                        
                    //already handled these folks?
                    if (_cohortDictionary.ContainsKey(priv) || IsNull(priv))
                        continue;
                        
                    //and the release id specified in the input table
                    var release = row[_releaseIdentifier];
                        
                    //if it was blank
                    if (IsNull(release))
                    {
                        if (_allocator != null)
                        {
                            if (foundUserSpecifiedReleaseIds && _allocator != null)
                                throw new Exception("Input data table had a column '" + _releaseIdentifier + "' which contained some values but also null values.  There is a configured ReleaseIdentifierAllocator, we cannot cannot continue since it would result in a mixed release identifier list of some provided by you and some provided by the ReleaseIdentifierAllocator");

                                release = _allocator.AllocateReleaseIdentifier(priv);
                        }
                    }
                    else
                        foundUserSpecifiedReleaseIds = true;

                    //no, allocate them an ID (or null if there is no allocator)
                    _cohortDictionary.Add(priv,release);
                }
            }

            return null;
        }

        
        private bool IsNull(object o)
        {
            if (o == null || o == DBNull.Value)
                return true;

            return string.IsNullOrWhiteSpace(o.ToString());
        }

        public virtual void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            //it exceptioned
            if (pipelineFailureExceptionIfAny != null)
                return;

            var server = DataAccessPortal.GetInstance().ExpectServer(Request.NewCohortDefinition.LocationOfCohort, DataAccessContext.DataLoad);
            
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Preparing upload"));
            
            using (var connection = server.BeginNewTransactedConnection())
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Started Transaction"));
                Request.PushToServer(connection);

                if(Request.NewCohortDefinition.ID == null)
                    throw new Exception("We pushed the new cohort from the request object to the server (within transaction) but it's ID was not populated");

                var tbl = server.GetCurrentDatabase().ExpectTable(Request.NewCohortDefinition.LocationOfCohort.TableName);

                var bulkCopy = tbl.BeginBulkInsert(connection.ManagedTransaction);

                var dt = new DataTable();
                dt.Columns.Add(_privateIdentifier);
                dt.Columns.Add(_releaseIdentifier);

                //add the ID as another column 
                dt.Columns.Add(_fk);

                foreach (KeyValuePair<object, object> kvp in _cohortDictionary)
                    dt.Rows.Add(kvp.Key, kvp.Value,Request.NewCohortDefinition.ID);

                bulkCopy.Upload(dt);
            }

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Succesfully uploaded " + _cohortDictionary.Count + " records"));
            
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

            var syntax = target.GetQuerySyntaxHelper();
            _privateIdentifier = syntax.GetRuntimeName(target.PrivateIdentifierField);
            _releaseIdentifier = syntax.GetRuntimeName(target.ReleaseIdentifierField);

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

            if (ReleaseIdentifierAllocator == null)
                notifier.OnCheckPerformed(new CheckEventArgs("No ReleaseIdentifierAllocator has been set, this means that Release Identifiers must be provided in the cohort uploaded or populated afer committing manually",CheckResult.Warning));
            
            notifier.OnCheckPerformed(new CheckEventArgs("Cohort identifier columns are '"+ _privateIdentifier + "' (private) and '" + _releaseIdentifier + "' (release)", CheckResult.Success));
            
            Request.Check(notifier);
        }
    }
}
