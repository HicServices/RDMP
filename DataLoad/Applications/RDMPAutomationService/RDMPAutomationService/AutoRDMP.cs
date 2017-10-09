using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data.Automation;
using CommandLine;
using Timer = System.Timers.Timer;

namespace RDMPAutomationService
{
    public class AutoRDMP
    {
        public event EventHandler<ServiceEventArgs> LogEvent;

        private RDMPAutomationLoop host;

        private readonly Action<EventLogEntryType, string> logAction;
        private readonly Timer timer;

        public AutoRDMP()
        {
            logAction = (et, msg) => OnLogEvent(new ServiceEventArgs() { EntryType = et, Message = msg });
            timer = new Timer(600000);
            timer.Elapsed += (sender, args) => Start();
            timer.Start();

            InitialiseAutomationLoop();
        }

        private void InitialiseAutomationLoop()
        {
            host = new RDMPAutomationLoop(new AutomationServiceOptions(), logAction);
            host.Failed += HostServiceFailure;
            host.StartCompleted += HostStarted;
        }

        private void HostServiceFailure(object sender, ServiceEventArgs e)
        {
            OnLogEvent(new ServiceEventArgs()
            {
                EntryType = e.EntryType,
                Message = "RDMP Automation Loop did not start: \r\n\r\n" + e.Message +
                          "\r\n\r\nWill try again in about " + timer.Interval / 60000 + " minute(s)."
            });
            Stop();
            Environment.FailFast("Unrecoverable error from Automation Loop", e.Exception);
        }

        private void HostStarted(object sender, ServiceEventArgs e)
        {
            OnLogEvent(new ServiceEventArgs() { EntryType = e.EntryType, Message = e.Message });
        }

        public void Start()
        {
            OnLogEvent(new ServiceEventArgs()
            {
                EntryType = EventLogEntryType.Information,
                Message = "Starting Host Container..."
            });

            host.Start();
            
            if (Environment.UserInteractive)
            {
                // running as console app
                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
                Stop();
            }
        }
        
        public void Stop()
        {
            host.Stop = true;
        }

        protected virtual void OnLogEvent(ServiceEventArgs e)
        {
            var handler = LogEvent;
            if (handler != null)
            {
                handler(this, e);
            }
            else
            {
                Console.WriteLine("{0}: {1}", e.EntryType.ToString().ToUpper(), e.Message);
            }
        }
    }
}