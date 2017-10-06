using System;
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
        public bool StillRunning
        {
            get
            {
                return 
                    t == null //not started yet 
                    || t.IsAlive; //or running
            }
        }

        /// <summary>
        /// Manages all the OnGoingAutomationTasks, will be null until a lock has been successfully established on an AutomationServiceSlot
        /// </summary>
        public AutomationDestination AutomationDestination { get; private set; }
        private Thread t;

        public bool Stop { get; set; }

        public void Start()
        {
            Stop = false;

            t = new Thread(Run);
            t.Start();
        }

        private readonly IRepository _repository;
        private readonly IRDMPPlatformRepositoryServiceLocator _locator;
        private AutomationServiceSlot _serviceSlot;
        private AutomationPipelineEngineCollection _collection;
        
        /// <summary>
        /// true once an RDMPStartup has completed execution (MEF loaded, repositories located etc)
        /// </summary>
        public bool StartupComplete { get; set; }

        public RDMPAutomationLoop(IRDMPPlatformRepositoryServiceLocator locator, AutomationServiceSlot serviceSlot)
        {
            _repository = locator.CatalogueRepository;
            _locator = locator;
            _serviceSlot = serviceSlot;

            AutomationDestination = new AutomationDestination();
        }

        private void Run()
        {
            bool lockEstablished = false;
            try
            {
                if (_serviceSlot == null)
                    throw new Exception("Cannot start automation service because there are no free AutomationServiceSlots, they must all be locked?");

                //refresh it since it might have changed
                _serviceSlot.RevertToDatabaseState();

                if (_serviceSlot.LockedBecauseRunning)
                    throw new NotSupportedException("AutomationServiceSlot");

                //lock it
                _serviceSlot.Lock();

                Startup startup = new Startup(_locator);
                startup.DoStartup(new AutomationMEFLoadingCheckNotifier());//who cares if MEF has problems eh?

                StartupComplete = true;
                lockEstablished = true;

                if (_serviceSlot.GlobalTimeoutPeriod.HasValue && _serviceSlot.GlobalTimeoutPeriod.Value > 0)
                    DatabaseCommandHelper.GlobalTimeout = _serviceSlot.GlobalTimeoutPeriod.Value;

                //let people know we are still alive
                _serviceSlot.TickLifeline();
                
                _collection = new AutomationPipelineEngineCollection(_locator, _serviceSlot, AutomationDestination);

                Console.WriteLine("_____AUTOMATION SERVER NOW RUNNING_____");

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
                Console.WriteLine(e);
                new AutomationServiceException((ICatalogueRepository) _repository, e);
            }
            finally
            {
                if(lockEstablished)
                    _serviceSlot.Unlock();
            }
            
        }

        
    }
}
