using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using RDMPAutomationService.Pipeline;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService
{
    /// <summary>
    /// The main Automation loop class.  Manages running all Automation Pipelines, picking an available AutomationServiceSlot, shutting down cleanly etc.
    /// </summary>
    public class RDMPAutomationLoop
    {
        /// <summary>
        /// Manages all the OnGoingAutomationTasks, will be null until a lock has been successfully established on an AutomationServiceSlot
        /// </summary>
        public AutomationDestination AutomationDestination { get; private set; }
        
        public bool Stop { get; set; }

        public event EventHandler<ServiceEventArgs> Failed;
        public event EventHandler<ServiceEventArgs> StartCompleted;
        
        private readonly IRepository _repository;
        private readonly IRDMPPlatformRepositoryServiceLocator _locator;
        private AutomationServiceSlot _serviceSlot;
        private AutomationPipelineEngineCollection _collection;
        private AutomationServiceOptions _options;
        
        private Thread t;
        private readonly Action<EventLogEntryType, string> _log;
        private readonly string mySelf = Environment.UserName + " (" + Environment.MachineName + ")";
        
        private bool lockEstablished;
        
        public RDMPAutomationLoop(AutomationServiceOptions options, Action<EventLogEntryType, string> logAction)
        {
            _log = logAction;
            _options = options;
            _locator = options.GetRepositoryLocator();
            _repository = _locator.CatalogueRepository;

            AutomationDestination = new AutomationDestination();
        }
        
        public void Start()
        {
            Stop = false;
            lockEstablished = false;
            _serviceSlot = GetFirstAutomationServiceSlot(_options.ForceSlot);
            try
            {
                if (_serviceSlot == null)
                {
                    _log(EventLogEntryType.Error,
                        "Cannot start automation service without an AutomationServiceSlot, are they all locked?");
                    return;
                }

                //refresh it since it might have changed
                _serviceSlot.RevertToDatabaseState();

                if (_serviceSlot.LockedBecauseRunning && _serviceSlot.LockHeldBy != mySelf)
                {
                    _log(EventLogEntryType.Error,
                        "AutomationServiceSlots seems to be locked by someone else: " + _serviceSlot.LockHeldBy);
                    return;
                }

                //lock it
                _serviceSlot.Lock();

                Startup startup = new Startup(_locator);
                startup.DoStartup(new AutomationMEFLoadingCheckNotifier()); //who cares if MEF has problems eh?

                OnStartCompleted();
            }
            catch (Exception e)
            {
                OnFailed(e);
                if (lockEstablished)
                    _serviceSlot.Unlock();
            }
            t = new Thread(Run);
            t.Start();
        }

        private void Run()
        {
            try
            {
                if (_serviceSlot.GlobalTimeoutPeriod.HasValue && _serviceSlot.GlobalTimeoutPeriod.Value > 0)
                    DatabaseCommandHelper.GlobalTimeout = _serviceSlot.GlobalTimeoutPeriod.Value;

                //let people know we are still alive
                _serviceSlot.TickLifeline();
                
                _collection = new AutomationPipelineEngineCollection(_locator, _serviceSlot, AutomationDestination);

                _log(EventLogEntryType.Information, String.Format("_____AUTOMATION SERVER NOW RUNNING ON SLOT {0}_____", _serviceSlot.ID));

                //always keep processing cancellation requests, infact if the user is trying to shut down automation hes probably also spamming cancellation buttons
                var poll = new CancellationTokenSource();
                var pollTask = new Task(()=>
                    {
                        while(!poll.Token.IsCancellationRequested)
                        {
                            Task.Delay(100, poll.Token);
                            AutomationDestination.ProcessCancellationRequests();
                        }
                    });

                pollTask.Start();

                var waitTime = 1000;

                //while it is not the case that we are told to stop and can stop
                while (!(Stop && AutomationDestination.CanStop()))
                {
                    //Check for new tasks, if nothing fails throttle up and run the next batch at increased intervals
                    if (!Stop)
                    {
                        try
                        {
                            _collection.ExecuteAll(waitTime);
                            if (waitTime <= 300000) // max 10 minutes yo!
                                waitTime *= 2;
                        }
                        catch (Exception e)
                        {
                            var messages = new List<string>();
                            if (e is AggregateException)
                            {
                                foreach (var exception in ((AggregateException)e).InnerExceptions)
                                {
                                    messages.Add(ExceptionHelper.ExceptionToListOfInnerMessages(exception));
                                }
                            }
                            else
                            {
                                messages = new List<string>() { ExceptionHelper.ExceptionToListOfInnerMessages(e) };
                            }
                            _log(EventLogEntryType.Error,
                                "Pipeline execution failed: \r\n" + 
                                String.Join("\r\n----\r\n", messages) +
                                "\r\n\r\nWill retry...");
                            waitTime = 1000;
                        }
                    }

                    //let people know we are still alive - Even if we were told to stop
                    _serviceSlot.TickLifeline();
                }

                poll.Cancel();
            }
            catch (Exception e)
            {
                OnFailed(e);
            }
            finally
            {
                if(lockEstablished)
                    _serviceSlot.Unlock();
            }
            
        }

        private AutomationServiceSlot GetFirstAutomationServiceSlot(int forceSlot)
        {
            if (forceSlot != 0)
            {
                var existingSlot = _locator.CatalogueRepository.GetAllObjects<AutomationServiceSlot>().FirstOrDefault(ass => ass.ID == forceSlot);
                if (existingSlot == null)
                {
                    _log(EventLogEntryType.Warning,
                        String.Format("Could not find the requested slot {0}.", forceSlot));
                    return null;
                }
                if (existingSlot.LockedBecauseRunning)
                {
                    if (existingSlot.LockHeldBy == mySelf)
                    {
                        existingSlot.Unlock();
                        return existingSlot;
                    }
                    else
                    {
                        _log(EventLogEntryType.Warning,
                             String.Format("Found slot {0}, but it was locked by {1}.", forceSlot, existingSlot.LockHeldBy));
                        return null;
                    }
                }

                return existingSlot;
            }
            else
            {
                //get all slots, locked first so if one of the locked is ours we will re-grab it!
                var slots = _locator.CatalogueRepository.GetAllObjects<AutomationServiceSlot>().OrderByDescending(ass => ass.LockedBecauseRunning);
                foreach (var automationServiceSlot in slots)
                {
                    if (automationServiceSlot.LockedBecauseRunning)
                    {
                        if (automationServiceSlot.LockHeldBy == mySelf)
                        {
                            automationServiceSlot.Unlock();
                            return automationServiceSlot;
                        }
                    }
                    else
                    {
                        return automationServiceSlot;
                    }
                }
            }
            return null;
        }
        
        private void OnFailed(Exception exception)
        {
            new AutomationServiceException((ICatalogueRepository) _repository, exception);
            var eventArgs = new ServiceEventArgs()
            {
                EntryType = EventLogEntryType.Error,
                Exception = exception,
                Message = exception.Message
            };
            
            var handler = Failed;
            if (handler != null) 
                handler(this, eventArgs);
        }

        private void OnStartCompleted()
        {
            lockEstablished = true;

            var eventArgs = new ServiceEventArgs()
            {
                EntryType = EventLogEntryType.Information,
                Message = String.Format("MEF Startup Completed, Lock on SLOT {0} established.", _serviceSlot.ID)
            };

            var handler = StartCompleted;
            if (handler != null)
                handler(this, eventArgs);
        }
    }
}
