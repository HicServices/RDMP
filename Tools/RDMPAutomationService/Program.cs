using System;
using System.Configuration.Install;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using RDMPStartup;

namespace RDMPAutomationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "-install":
                            {
                                if (ServiceController.GetServices().Any(s => s.ServiceName == "RDMPAutomationService"))
                                {
                                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                                }
                                ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                                break;
                            }
                        case "-uninstall":
                            {
                                ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                                break;
                            }
                        default:
                        {
                            Console.WriteLine("Use -install to install or -uninstall to uninstall" +
                                              "\r\n" +
                                              "No command line options will run in console.");
                            break;
                        }
                    }
                }
                else
                {
                    var autoRDMP = new AutoRDMP();
                    autoRDMP.Start();
                }
            }
            else
            {
                var servicesToRun = new ServiceBase[] { new RDMPAutomationService() };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
