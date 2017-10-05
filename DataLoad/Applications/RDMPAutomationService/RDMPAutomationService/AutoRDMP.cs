using System;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using CommandLine;

namespace RDMPAutomationService
{
    public class AutoRDMP
    {
        private RDMPAutomationLoop host;
        private IRDMPPlatformRepositoryServiceLocator serviceLocator;

        private AutomationServiceSlot GetFirstAutomationServiceSlot(IRDMPPlatformRepositoryServiceLocator repositoryFinder)
        {
            //get first unlocked slot
            return repositoryFinder.CatalogueRepository.GetAllObjects<AutomationServiceSlot>().FirstOrDefault(a => !a.LockedBecauseRunning);
        }

        public void Start()
        {
            var options = new AutomationServiceOptions();
            serviceLocator = options.GetRepositoryLocator();

            host = new RDMPAutomationLoop(serviceLocator, GetFirstAutomationServiceSlot(serviceLocator));

            OnLogEvent(new ServiceEventArgs()
            {
                EntryType = EventLogEntryType.Information,
                Message = "Starting Host Container..."
            });

            host.Start();

            if (Environment.UserInteractive)
            {
                // running as console app
                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
                Stop();
            }
        }

        public void Stop()
        {
            host.Stop = true;
        }

        public event EventHandler<ServiceEventArgs> LogEvent;

        protected virtual void OnLogEvent(ServiceEventArgs e)
        {
            var handler = LogEvent;
            if (handler != null)
            {
                handler(this, e);
            }
            else
            {
                Console.WriteLine("{0}: {1}", e.EntryType.ToString().ToUpper(), e.Message);
            }
        }
    }

    public class ServiceEventArgs
    {
        public string Message { get; set; }
        public EventLogEntryType EntryType { get; set; }
    }
}