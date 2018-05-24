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
using ReusableLibraryCode;

namespace RDMPAutomationService
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                try
                {
                    return
                        Parser.Default.ParseArguments<ServiceOptions, DleOptions>(args)
                            .MapResult(
                                (ServiceOptions opts) => RunServiceAndReturnExitCode(opts),
                                (DleOptions opts) => RunDleOptionsAndReturnExitCode(opts),
                                errs => 1);
                }
                catch (Exception e)
                {
                    Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(e));
                    return -1;
                }
            }
            
            var servicesToRun = new ServiceBase[] { new RDMPAutomationService() };
            ServiceBase.Run(servicesToRun);
            return 0;
        }

        private static int RunDleOptionsAndReturnExitCode(DleOptions opts)
        {
            var locator = opts.DoStartup();

            switch (opts.Command)
            {
                case DLECommands.run:
                    
                    var lmd = locator.CatalogueRepository.GetObjectByID<LoadMetadata>(opts.LoadMetadata);
                    var load = new AutomatedDLELoad(lmd);
                    load.RunTask(locator);

                    return 0;
                case DLECommands.list:
                    
                    Console.WriteLine(string.Format("[ID] - Name"));
                    foreach (LoadMetadata l in locator.CatalogueRepository.GetAllObjects<LoadMetadata>())
                        Console.WriteLine("[{0}] - {1}", l.ID, l.Name);

                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private static int RunServiceAndReturnExitCode(ServiceOptions serviceOptions)
        {
            switch (serviceOptions.Command)
            {
                case ServiceCommands.run:
                    var autoRDMP = new AutoRDMP(serviceOptions);
                    autoRDMP.Start();
                    return 0;
                case ServiceCommands.install:
                    if (ServiceController.GetServices().Any(s => s.ServiceName == "RDMPAutomationService"))
                        ManagedInstallerClass.InstallHelper(new string[] {"/u", Assembly.GetExecutingAssembly().Location});

                    ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                return 0;
                case ServiceCommands.uninstall:
                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
