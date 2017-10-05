using System;
using System.Diagnostics;
using System.ServiceProcess;
using RDMPAutomationService;

namespace RDMPAutomationService
{
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
            autoRDMP.Start();
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