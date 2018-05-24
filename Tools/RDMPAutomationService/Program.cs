using System;
using System.Configuration.Install;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using CatalogueLibrary.Data.DataLoad;
using CommandLine;
using RDMPAutomationService.Logic.DLE;
using RDMPAutomationService.Options;
using RDMPStartup;

namespace RDMPAutomationService
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (Environment.UserInteractive)
            {

                return
                    Parser.Default.ParseArguments<ServiceOptions, RunOptions, DleOptions>(args)
                        .MapResult(
                            (ServiceOptions opts) => RunServiceAndReturnExitCode(opts),
                            (RunOptions opts) => RunRunOptionsAndReturnExitCode(opts),
                            (DleOptions opts) => RunDleOptionsAndReturnExitCode(opts),
                    errs => 1);
            }
            
            var servicesToRun = new ServiceBase[] { new RDMPAutomationService() };
            ServiceBase.Run(servicesToRun);
            return 0;
        }

        private static int RunDleOptionsAndReturnExitCode(DleOptions opts)
        {
            var locator = opts.DoStartup();

            if (opts.Command == DLECommands.list)
            {

                Console.WriteLine(string.Format("[ID] - Name"));
                foreach (LoadMetadata lmd in locator.CatalogueRepository.GetAllObjects<LoadMetadata>())
                    Console.WriteLine("[{0}] - {1}", lmd.ID, lmd.Name);
            }

            if (opts.Command == DLECommands.run)
            {
                var lmd = locator.CatalogueRepository.GetObjectByID<LoadMetadata>(opts.LoadMetadata);
                var load = new AutomatedDLELoad(lmd);
                load.RunTask(locator);
            }

            
            return 0;
        }

        private static int RunRunOptionsAndReturnExitCode(RunOptions runOptions)
        {
            var autoRDMP = new AutoRDMP(runOptions);
            autoRDMP.Start();
            return 0;
        }

        private static int RunServiceAndReturnExitCode(ServiceOptions serviceOptions)
        {
            if (serviceOptions.Install)
            {
                if (ServiceController.GetServices().Any(s => s.ServiceName == "RDMPAutomationService"))
                {
                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                }
                ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                return 0;
            }

            if (serviceOptions.Uninstall)
            {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                return 0;
            }

            return -1;
        }
    }
}
