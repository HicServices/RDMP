using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;

namespace CachingService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            EventLogInstaller installer = FindInstaller(Installers);
            if (installer != null)
                installer.Log = "RDMPCachingEngine";
        }

        private EventLogInstaller FindInstaller(InstallerCollection installers)
        {
            foreach (Installer installer in installers)
            {
                if (installer is EventLogInstaller)
                    return (EventLogInstaller) installer;

                var eventLogInstaller = FindInstaller(installer.Installers);
                if (eventLogInstaller != null)
                    return eventLogInstaller;
            }

            return null;
        }

        protected override void OnCommitted(IDictionary savedState)
        {
            base.OnCommitted(savedState);

            // todo: should be configuration, but hard-coding for now (as it can easily be changed through post-install)
            int exitCode;
            using (var process = new Process())
            {
                var startInfo = process.StartInfo;
                startInfo.FileName = "sc";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                // tell Windows that the service should restart if it fails
                // Something like this when I find out if there is a mail/notification solution in the new infrastructure
                startInfo.Arguments = "failure \"RDMPCachingService\" reset= 600 command= \"NotifyCachingServiceFailure.cmd\" actions= restart/60000/restart/300000/run/30000";
                //startInfo.Arguments = "failure \"RDMPCachingService\" reset= 600 actions= restart/60000/restart/300000";

                process.Start();
                process.WaitForExit();

                exitCode = process.ExitCode;
            }

            if (exitCode != 0)
                throw new InvalidOperationException();
        }
    }
}
