using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using RDMPAutomationService;

namespace RDMPAutomationService
{
    /// <summary>
    /// Wrapper for AutoRDMP when running as a Windows Service (inherits from ServiceBase).  Handles all the Windows Service based functionality (logging service 
    /// events, starting stoping etc) passing on calls to OnStart / OnStop to AutoRDMP etc (OnStart / OnStop occur when a user tries to shutdown/start a windows
    /// service).
    /// </summary>
    public class RDMPAutomationService : ServiceBase
    {
        private EventLog eventLogger;
        private AutoRDMP autoRDMP;

        public RDMPAutomationService()
        {
            InitializeComponent();
            InitLogging();
        }

        protected override void OnStart(string[] args)
        {
            eventLogger.WriteEntry("Starting up RDMP AutomationService", EventLogEntryType.Information);
            autoRDMP = new AutoRDMP();
            autoRDMP.LogEvent += (o,e) => eventLogger.WriteEntry(e.Message, e.EntryType);

            Task.Run(() => autoRDMP.Start());
        }

        protected override void OnStop()
        {
            eventLogger.WriteEntry("Stopping RDMP AutomationService", EventLogEntryType.Information);
            autoRDMP.Stop();
        }

        private void InitLogging()
        {
            if (!EventLog.SourceExists(this.ServiceName))
            {
                EventLog.CreateEventSource(ServiceName, "Application");
            }
            eventLogger.Source = ServiceName;
            eventLogger.Log = "Application";
        }

        private void InitializeComponent()
        {
            this.eventLogger = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.eventLogger)).BeginInit();
            // 
            // RDMPAutomationService
            // 
            this.ServiceName = "RDMPAutomationService";
            ((System.ComponentModel.ISupportInitialize)(this.eventLogger)).EndInit();
        }
    }
}