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
    public class DLEAutomationSource : IAutomationSource
    {
        private AutomationServiceSlot _serviceSlot;
        private DLERunFinder _dleRunFinder;

        public OnGoingAutomationTask GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (_serviceSlot.IsAcceptingNewJobs(AutomationJobType.DLE))
            {
                LoadPeriodically periodicallyWeCanRun = _dleRunFinder.SuggestLoad();

                //if a run was suggested,set it up
                if (periodicallyWeCanRun != null)
                {
                    string msg = "Started Loading Periodic ID=" + periodicallyWeCanRun.ID + " (LoadMetadata ID=" +periodicallyWeCanRun.LoadMetadata_ID + ")";
                    listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, msg));
                    Console.WriteLine(msg);

                    var toReturn = new AutomatedDLELoad(_serviceSlot, periodicallyWeCanRun).GetTask();

                    msg = "Automation Job ID is " + toReturn.Job.ID;
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, msg));
                    Console.WriteLine(msg);

                    return toReturn;
                }
                else
                {
                    //no periodical loads are available, how about a cache loading DLE job?
                    var cacheBasedLoadWeCanRun = _dleRunFinder.SuggestLoadBecauseCacheAvailable();

                    if (cacheBasedLoadWeCanRun == null)
                        return null;

                    string msg = "Started cache based load ID = " + cacheBasedLoadWeCanRun.ID;
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, msg));
                    Console.WriteLine(msg);


                    var toReturn = new AutomatedDLELoadFromCache(_serviceSlot, cacheBasedLoadWeCanRun).GetTask();

                    msg = "Automation Job ID is " + toReturn.Job.ID;
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, msg));
                    Console.WriteLine(msg);

                    return toReturn;
                }

            }

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
            _dleRunFinder = new DLERunFinder((ICatalogueRepository) _serviceSlot.Repository);

        }
    }
}
