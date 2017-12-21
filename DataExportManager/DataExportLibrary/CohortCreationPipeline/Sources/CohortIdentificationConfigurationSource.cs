using System;
using System.Data;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CohortManagerLibrary.Execution;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.CohortCreationPipeline.Sources
{
    /// <summary>
    /// Executes a Cohort Identification Configuration query and releases the identifiers read into the pipeline as a single column DataTable.
    /// </summary>
    public class CohortIdentificationConfigurationSource : IPluginDataFlowSource<DataTable>, IPipelineRequirement<CohortIdentificationConfiguration>
    {
        private CohortIdentificationConfiguration _cohortIdentificationConfiguration;

        [DemandsInitialization("The length of time (in seconds) to wait before timing out the SQL command to execute the CohortIdentificationConfiguration, if you find it is taking exceptionally long for a CohortIdentificationConfiguration to execute then consider caching some of the subqueries",DemandType.Unspecified,10000)]
        public int Timeout { get; set; }

        [DemandsInitialization("If ticked, will Freeze the CohortIdentificationConfiguration if the import pipeline terminates successfully")]
        public bool FreezeAfterSuccessfulImport { get; set; }

        private bool haveSentData = false;

        public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {

            if(haveSentData)
                return null;

            haveSentData = true;

            return GetDataTable(Timeout, listener);
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            //if it didn't crash
            if(pipelineFailureExceptionIfAny == null)
                if(FreezeAfterSuccessfulImport)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Freezing CohortIdentificationConfiguration"));
                    _cohortIdentificationConfiguration.Freeze();
                }
        }


        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public DataTable TryGetPreview()
        {
            return GetDataTable(10,null);
        }

        private DataTable GetDataTable(int timeout, IDataLoadEventListener listener)
        {
            if(listener != null)
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to lookup which server to interrogate for CohortIdentificationConfiguration " + _cohortIdentificationConfiguration));

            if(_cohortIdentificationConfiguration.RootCohortAggregateContainer_ID == null)
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "CohortIdentificationConfiguration '" + _cohortIdentificationConfiguration + "' has no RootCohortAggregateContainer_ID, is it empty?"));

            var cohortCompiler = new CohortCompiler(_cohortIdentificationConfiguration);
            cohortCompiler.AddTask(_cohortIdentificationConfiguration.RootCohortAggregateContainer,_cohortIdentificationConfiguration.GetAllParameters());

            var task = cohortCompiler.Tasks.Single();

            cohortCompiler.LaunchSingleTask(task.Key,timeout);

            //timeout is in seconds
            int countDown = timeout * 1000;

            while ( 
                //hasn't timed out
                countDown > 0 && 
                (
                    //state isn't a final state
                    task.Key.State == CompilationState.Executing || task.Key.State == CompilationState.NotScheduled || task.Key.State == CompilationState.Scheduled)
                )
            {
                Thread.Sleep(100);
                countDown -= 100;
            }


            if(countDown <= 0)
                try
                {
                    throw new Exception("Cohort failed to reach a final state (Finished/Crashed) after " + Timeout + " seconds. Current state is " + task.Key.State + ".  The task will be cancelled");
                }
                finally
                {
                    cohortCompiler.CancelAllTasks(true);
                }

            if(task.Key.State != CompilationState.Finished)
                throw new Exception("CohortIdentificationCriteria execution resulted in state '" + task.Key.State +"'",task.Key.CrashMessage);

            if(task.Value.Identifiers == null || task.Value.Identifiers.Rows.Count  == 0)
                throw new Exception("CohortIdentificationCriteria execution resulted in an empty dataset (there were no cohorts matched by the query?)");

            var dt = task.Value.Identifiers;

            foreach (DataColumn column in dt.Columns)
                column.ReadOnly = false;
            
            return dt;
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                if (_cohortIdentificationConfiguration.Frozen)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "CohortIdentificationConfiguration " + _cohortIdentificationConfiguration +
                            " is Frozen (By " + _cohortIdentificationConfiguration.FrozenBy + " on " +
                            _cohortIdentificationConfiguration.FrozenDate + ").  It might have already been imported once before.", CheckResult.Warning));

                
                var result = TryGetPreview();
                
                if(result.Rows.Count == 0)
                    throw new Exception("No Identifiers were returned by the cohort query");
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not build extraction SQL for " + _cohortIdentificationConfiguration, CheckResult.Fail,e));
            }
            
        }


        public void PreInitialize(CohortIdentificationConfiguration value, IDataLoadEventListener listener)
        {
            _cohortIdentificationConfiguration = value;
        }
    }
}
