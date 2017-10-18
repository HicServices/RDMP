using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace RDMPAutomationService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            this.AfterInstall += ServiceInstaller_AfterInstall;
        }

        void ServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            //int exitCode;
            //using (var process = new Process())
            //{
            //    var startInfo = process.StartInfo;
            //    startInfo.FileName = "sc";
            //    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            //    // tell Windows that the service should restart if it fails
            //    startInfo.Arguments = string.Format("failure \"{0}\" reset= 0 actions= restart/60000/restart/60000//", serviceInstaller1.ServiceName);

            //    process.Start();
            //    process.WaitForExit();

            //    exitCode = process.ExitCode;
            //}

            //if (exitCode != 0)
            //    throw new InvalidOperationException();

            //using (ServiceController sc = new ServiceController(serviceInstaller1.ServiceName))
            //{
            //    sc.Start();
            //}
        }
    }
}
