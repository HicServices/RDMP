using System;
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
    public class RDMPAutomationLoop
    {
        /// <summary>
        /// Manages all the OnGoingAutomationTasks, will be null until a lock has been successfully established on an AutomationServiceSlot
        /// </summary>
        public AutomationDestination AutomationDestination { get; private set; }
        
        public bool Stop { get; set; }

        public bool StillRunning
        {
            get
            {
                return
                    t == null //not started yet 
                    || t.IsAlive; //or running
            }
        }
        
        public void Start()
        {
            Stop = false;

            t = new Thread(Run);
            t.Start();
        }

        public event EventHandler<ServiceEventArgs> Failed;
        public event EventHandler<ServiceEventArgs> StartCompleted;
        
        private readonly IRepository _repository;
        private readonly IRDMPPlatformRepositoryServiceLocator _locator;
        private AutomationServiceSlot _serviceSlot;
        private AutomationPipelineEngineCollection _collection;
        private Thread t;
        private readonly Action<EventLogEntryType, string> _log;
        private readonly string mySelf = Environment.UserName + " (" + Environment.MachineName + ")";
        
        /// <summary>
        /// true once an RDMPStartup has completed execution (MEF loaded, repositories located etc)
        /// </summary>
        public bool StartupComplete { get; set; }

        public RDMPAutomationLoop(AutomationServiceOptions options, Action<EventLogEntryType, string> logAction)
        {
            _log = logAction;
            _locator = options.GetRepositoryLocator();
            _repository = _locator.CatalogueRepository;
            _serviceSlot = GetFirstAutomationServiceSlot();

            AutomationDestination = new AutomationDestination();
        }

        public RDMPAutomationLoop(IRDMPPlatformRepositoryServiceLocator locator, AutomationServiceSlot serviceSlot, Action<EventLogEntryType, string> log)
        {
            _repository = locator.CatalogueRepository;
            _locator = locator;
            _serviceSlot = serviceSlot;
            _log = log;

            AutomationDestination = new AutomationDestination();
        }

        private void Run()
        {
            bool lockEstablished = false;
            try
            {
                if (_serviceSlot == null)
                {
                    _log(EventLogEntryType.Warning, "Cannot start automation service without an AutomationServiceSlots, are they all locked?");
                    return;
                }

                //refresh it since it might have changed
                _serviceSlot.RevertToDatabaseState();

                if (_serviceSlot.LockedBecauseRunning && _serviceSlot.LockHeldBy != mySelf)
                {
                    _log(EventLogEntryType.Error, "AutomationServiceSlots seems to be locked by someone else!");
                    return;
                }

                //lock it
                _serviceSlot.Lock();
                _log(EventLogEntryType.Information, "Locked service slot " + _serviceSlot.ID);

                Startup startup = new Startup(_locator);
                startup.DoStartup(new AutomationMEFLoadingCheckNotifier());//who cares if MEF has problems eh?

                StartupComplete = true;
                lockEstablished = true;

                if (_serviceSlot.GlobalTimeoutPeriod.HasValue && _serviceSlot.GlobalTimeoutPeriod.Value > 0)
                    DatabaseCommandHelper.GlobalTimeout = _serviceSlot.GlobalTimeoutPeriod.Value;

                //let people know we are still alive
                _serviceSlot.TickLifeline();
                
                _collection = new AutomationPipelineEngineCollection(_locator, _serviceSlot, AutomationDestination);

                _log(EventLogEntryType.Information, "_____AUTOMATION SERVER NOW RUNNING_____" + _serviceSlot.ID);

                //always keep processing cancellation requests, infact if the user is trying to shut down automation hes probably also spamming cancellation buttons
                CancellationTokenSource poll = new CancellationTokenSource();
                Task pollTask = new Task(()=>
                    {
                        while(!poll.Token.IsCancellationRequested)
                        {
                            Task.Delay(100, poll.Token);
                            AutomationDestination.ProcessCancellationRequests();
                        }
                    });

                pollTask.Start();

                //while it is not the case that we are told to stop and can stop
                while (!(Stop && AutomationDestination.CanStop()))
                {
                    //Check for new tasks, if it takes less than 1 second chill out till 1s has passed
                    if (!Stop)
                        _collection.ExecuteAll(1000);

                    //let people know we are still alive - Even if we were told to stop
                    _serviceSlot.TickLifeline();
                }

                poll.Cancel();
            }
            catch (Exception e)
            {
                new AutomationServiceException((ICatalogueRepository) _repository, e);
                RaiseFailure(e);
            }
            finally
            {
                if(lockEstablished)
                    _serviceSlot.Unlock();
            }
            
        }

        private AutomationServiceSlot GetFirstAutomationServiceSlot()
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
            return null;
        }

        private void RaiseFailure(Exception exception)
        {
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
            var eventArgs = new ServiceEventArgs()
            {
                EntryType = EventLogEntryType.Information,
                Message = "MEF Startup Completed"
            };

            var handler = StartCompleted;
            if (handler != null)
                handler(this, eventArgs);
        }
    }
}
