using System;
using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution
{
    /// <summary>
    /// Pipeline which processes a single job through all stages before accepting another.  Execution involves running each DataLoadComponent with the current 
    /// IDataLoadJob and then disposing them. 
    /// </summary>
    public class SingleJobExecution : IDataLoadExecution
    {
        public List<IDataLoadComponent> Components { get; set; }
   

        public SingleJobExecution(List<IDataLoadComponent> components)
        {
            Components = components;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            job.StartLogging();
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Starting load for " + job.LoadMetadata.Name));

            try
            {
                foreach (var component in Components)
                {
                    cancellationToken.ThrowIfAbortRequested();

                    try
                    {
                        //schedule the component for disposal
                        job.PushForDisposal(component);

                        //run current component
                        var exitCodeType = component.Run(job, cancellationToken);
                        
                        //current component failed so jump out, either because load not nessesary or crash
                        if (exitCodeType == ExitCodeType.OperationNotRequired)
                        {
                            TryDispose(exitCodeType, job);
                            //load not nessesary so abort entire DLE process but also cleanup still
                            return exitCodeType;
                        }

                        if (exitCodeType != ExitCodeType.Success)
                            throw new Exception("Component " + component.Description + " returned result " + exitCodeType);
                    }
                    catch (OperationCanceledException e)
                    {
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, component.Description + " has been cancelled by the user", e));
                        throw;
                    }
                    catch (Exception e)
                    {
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, component.Description + " crashed while running Job ", e));
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Job crashed", e));
                        TryDispose(ExitCodeType.Error, job);
                        return ExitCodeType.Error;
                    }
                }

                TryDispose(ExitCodeType.Success, job);

                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Completed job " + job.JobID));
                
                return ExitCodeType.Success;
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsAbortRequested)
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Job " + job.JobID + "cancelled in pipeline"));

                TryDispose(cancellationToken.IsAbortRequested ? ExitCodeType.Abort : ExitCodeType.Success, job);
                throw;
            }
        }

        private void TryDispose(ExitCodeType exitCode,IDataLoadJob job)
        {
            try
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Disposing disposables..."));
                job.LoadCompletedSoDispose(exitCode, job);
                job.CloseLogging();
            }
            catch (Exception e)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Job " + job.JobID + " crashed again during disposing", e));
                throw;
            }
        }
    }
}
