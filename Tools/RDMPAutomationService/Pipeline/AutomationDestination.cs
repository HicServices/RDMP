using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Pipeline
{
    /// <summary>
    /// Starts OnGoingAutomationTasks identified by IAutomationSources.  There is a single instance of AutomationDestination regardless of how many 
    /// IAutomationSources / Automation pipelines there are running.  Handles fatal crashes of IAutomateables, pruning completed tasks and determining
    /// if it is safe to stop the AutomationService (all running IAutomateables have completed/crashed/cancelled).
    /// </summary>
    public class AutomationDestination : IDataFlowDestination<OnGoingAutomationTask>
    {
        public List<OnGoingAutomationTask> OnGoingTasks = new List<OnGoingAutomationTask>();

        private object oOngoingTasksLock = new object();

        public bool CanStop()
        {
            lock (oOngoingTasksLock)
            {

                //if there are no tasks
                if (!OnGoingTasks.Any())
                    return true;

                //or they are all completed or faulted
                return OnGoingTasks.All(t => t.Task.IsCompleted || t.Task.IsCanceled || t.Task.IsFaulted);
            }
        }

        public void ProcessCancellationRequests()
        {
            lock (oOngoingTasksLock)
            {
                //get rid of expired jobs
                PruneTaskList();

                foreach (OnGoingAutomationTask task in OnGoingTasks)
                {
                    try
                    {
                        task.Job.RevertToDatabaseState();

                        //if database has cancel requested propagate it to token source
                        if (task.Job.CancelRequested && !task.CancellationTokenSource.IsCancellationRequested)
                            task.CancellationTokenSource.Cancel();
                    }
                    catch (KeyNotFoundException e)
                    {
                        if (e.Message.Contains("Could not find " + typeof (AutomationJob).Name + " with ID "))
                            continue;
                                //task has been finished! between the time it took to PruneTaskList and get to here

                        throw;
                    }
                }
            }
        }

        public void WaitTillStoppable(int timeout = -1)
        {
            if (timeout != -1)
                Task.WaitAll(OnGoingTasks.Select(t => t.Task).ToArray(), timeout);
        }

        private void PruneTaskList()
        {
            const int maxJobs = 100;

            if (OnGoingTasks.Count(j => j.JobType == AutomationJobType.UserCustomPipeline) > maxJobs)
                throw new Exception("There are more than " + maxJobs + " UserCustomPipeline jobs currently executing");

            //go back to the database and any that have been .Deleted can be cleared
            OnGoingTasks = OnGoingTasks.Where(j => j.Job.Exists()).ToList();
        }

        public OnGoingAutomationTask ProcessPipelineData(OnGoingAutomationTask toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            lock (oOngoingTasksLock)
            {
                OnGoingTasks.Add(toProcess);
            }
            toProcess.Task.Start();
            
            return null;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }
    }
}
