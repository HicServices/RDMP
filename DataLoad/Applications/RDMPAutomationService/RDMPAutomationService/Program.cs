using System;
using System.Data.SqlClient;
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
                var autoRDMP = new AutoRDMP();
                autoRDMP.Start();
            }
            else
            {
                var servicesToRun = new ServiceBase[] { new RDMPAutomationService() };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
