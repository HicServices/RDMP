using System;
using System.Configuration.Install;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using CommandLine;
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
                    Parser.Default.ParseArguments<ServiceOptions>(args).MapResult(
                    RunServiceAndReturnExitCode, errs => 1);

            }
            
            var servicesToRun = new ServiceBase[] { new RDMPAutomationService() };
            ServiceBase.Run(servicesToRun);
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
