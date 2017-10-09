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
        private readonly RDMPAutomationLoop host;

        private readonly Action<EventLogEntryType, string> logAction;
        private readonly Timer timer;

        public AutoRDMP()
        {
            logAction = (et, msg) => OnLogEvent(new ServiceEventArgs() { EntryType = et, Message = msg });
            timer = new Timer(60000);
            timer.Elapsed += (sender, args) => Start();
            timer.Start();

            host = new RDMPAutomationLoop(new AutomationServiceOptions(), logAction);
        }

        public void Start()
        {
            OnLogEvent(new ServiceEventArgs()
            {
                EntryType = EventLogEntryType.Information,
                Message = "Starting Host Container..."
            });

            try
            {
                host.Start();
            }
            catch (Exception e)
            {
                OnLogEvent(new ServiceEventArgs() { EntryType = EventLogEntryType.Error,
                                                    Message = "RDMP Automation Loop did not start: \r\n\r\n" + e.Message + 
                                                              "\r\n\r\nWill try again in about " + timer.Interval / 60000 + " minute(s)." });
                return;
            }
            
            while (!host.StartupComplete)
            {
                Thread.Sleep(1000);
            }

            OnLogEvent(new ServiceEventArgs()
            {
                EntryType = EventLogEntryType.Information,
                Message = "RDMP Automation Loop Started..."
            });

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

        public event EventHandler<ServiceEventArgs> LogEvent;

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