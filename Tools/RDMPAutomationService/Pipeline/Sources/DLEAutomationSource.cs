using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using RDMPAutomationService.Interfaces;
using RDMPAutomationService.Logic.DLE;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Pipeline.Sources
{
    /// <summary>
    /// Identifies when a LoadMetadata can be executed (according to DLERunFinder), packages it as an AutomatedDLELoad/AutomatedDLELoadFromCache and releases it
    /// into the automation pipeline for scheduling/execution.
    /// </summary>
    public class DLEAutomationSource : IAutomationSource
    {
        private AutomationServiceSlot _serviceSlot;
        private DLERunFinder _dleRunFinder;
        private IDataLoadEventListener _listener;

        public OnGoingAutomationTask GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (_serviceSlot.IsAcceptingNewJobs(AutomationJobType.DLE))
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Slot is accepting new DLE jobs... let's find one."));
                
                LoadPeriodically periodicallyWeCanRun = _dleRunFinder.SuggestLoad();

                //if a run was suggested,set it up
                if (periodicallyWeCanRun != null)
                {
                    string msg = "Started Loading Periodic ID=" + periodicallyWeCanRun.ID + " (LoadMetadata ID=" +periodicallyWeCanRun.LoadMetadata_ID + ")";
                    listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, msg));
                    
                    var toReturn = new AutomatedDLELoad(_serviceSlot, periodicallyWeCanRun).GetTask();

                    msg = "Automation Job ID is " + toReturn.Job.ID;
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, msg));
                    
                    return toReturn;
                }
                else
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "No Load periodic jobs ready to run, trying with Cache Based LoadProgress"));
                    //no periodical loads are available, how about a cache loading DLE job?
                    var cacheBasedLoadWeCanRun = _dleRunFinder.SuggestLoadBecauseCacheAvailable();

                    if (cacheBasedLoadWeCanRun != null)
                    {
                        string msg = "Started cache based load ID = " + cacheBasedLoadWeCanRun.ID;
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, msg));

                        var toReturn = new AutomatedDLELoadFromCache(_serviceSlot, cacheBasedLoadWeCanRun).GetTask();

                        msg = "Automation Job ID is " + toReturn.Job.ID;
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, msg));

                        return toReturn;
                    }

                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "No Cache Based LoadProgress found."));
                    return null;
                }
            }

            _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Slot is not accepting any new DLE jobs..."));
            return null;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
        }

        public void Abort(IDataLoadEventListener listener)
        {
        }

        public OnGoingAutomationTask TryGetPreview()
        {
            throw new NotSupportedException();
        }

        public void PreInitialize(AutomationServiceSlot value, IDataLoadEventListener listener)
        {
            _serviceSlot = value;
            _listener = listener;
            _dleRunFinder = new DLERunFinder((ICatalogueRepository) _serviceSlot.Repository, _listener);

        }
    }
}
