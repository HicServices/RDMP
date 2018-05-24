using System;
using System.Diagnostics;
using RDMPAutomationService.Options;

namespace RDMPAutomationService
{
    /// <summary>
    /// Host container for an RDMPAutomationLoop which handles ressurecting it if it crashes and recording starting/stopping events to the console / windows
    /// logs (when running as a windows service).
    /// </summary>
    class AutoRDMP
    {
        public event EventHandler<ServiceEventArgs> LogEvent;
        public event EventHandler<ServiceEventArgs> HostStarted;

        private RDMPAutomationLoop host;

        private readonly Action<EventLogEntryType, string> logAction;
        private bool hostStarted;
        private ServiceOptions _options;

        internal AutoRDMP(ServiceOptions options = null)
        {
            _options = options ?? new ServiceOptions();
            logAction = (et, msg) => OnLogEvent(new ServiceEventArgs() { EntryType = et, Message = msg });
            InitialiseAutomationLoop();
        }

        private void InitialiseAutomationLoop()
        {
            host = new RDMPAutomationLoop(_options, logAction);
            host.Failed += OnHostServiceFailure;
            host.StartCompleted += OnHostStarted;
        }

        private void OnHostServiceFailure(object sender, ServiceEventArgs e)
        {
            var serviceEventArgs = new ServiceEventArgs()
            {
                EntryType = e.EntryType,
                Message = "RDMP Automation Loop failed catastrophically: \r\n\r\n" + e.Message
            };
            if (e.Exception != null)
            {
                serviceEventArgs.Message += "\r\n\r\n" + e.Exception.Message +
                                            "\r\n" + e.Exception.StackTrace;
            }

            OnLogEvent(serviceEventArgs);
            Stop();
            Environment.Exit(666);
        }

        private void OnHostStarted(object sender, ServiceEventArgs e)
        {
            OnLogEvent(new ServiceEventArgs() { EntryType = e.EntryType, Message = e.Message });
            hostStarted = true;
        }

        public void Start()
        {
            host.Start();

            OnLogEvent(new ServiceEventArgs()
            {
                EntryType = EventLogEntryType.Information,
                Message = "Starting Host Container..."
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