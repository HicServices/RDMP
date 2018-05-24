using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using RDMPAutomationService.Options;

namespace RDMPAutomationService.OptionRunners
{
    class ServiceOptionsRunner
    {
        public int Run(ServiceOptions opts)
        {
            switch (opts.Command)
            {
                case ServiceCommands.run:
                    opts.LoadFromAppConfig();
                    var autoRDMP = new AutoRDMP(opts);
                    autoRDMP.Start();
                    return 0;
                case ServiceCommands.install:
                    if (ServiceController.GetServices().Any(s => s.ServiceName == "RDMPAutomationService"))
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });

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