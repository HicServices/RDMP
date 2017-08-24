using System;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceProcess;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using RDMPStartup;

namespace RDMPAutomationService
{
    public class Program
    {
        public const string ServiceName = "RDMPAutomationService";
        private static RDMPAutomationLoop _host;
        public static void Main(string[] args)
        {
           Start(args);
        }

        private static AutomationServiceSlot GetFirstAutomationServiceSlot(IRDMPPlatformRepositoryServiceLocator repositoryFinder)
        {
            //get first unlocked slot
            return repositoryFinder.CatalogueRepository.GetAllObjects<AutomationServiceSlot>().FirstOrDefault(a => !a.LockedBecauseRunning);
        }

        public static void Start(string[] args)
        {
            IRDMPPlatformRepositoryServiceLocator serviceLocator;

            var options = new AutomationServiceOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
                serviceLocator = options.GetRepositoryLocator();
            else
                return;

            _host = new RDMPAutomationLoop(serviceLocator, GetFirstAutomationServiceSlot(serviceLocator));

            if (!Environment.UserInteractive)
                // running as service
                using (var service = new RDMPAutomationService())
                    ServiceBase.Run(service);
            else
            {
                // running as console app
                _host.Start();

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);

                Stop();
            }
        }

        public static void Stop()
        {
            _host.Stop = true;
        }
    }
}
