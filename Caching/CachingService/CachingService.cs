using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using CatalogueLibrary.Repositories;
using IContainer = System.ComponentModel.IContainer;

namespace CachingService
{
    public partial class CachingService : ServiceBase
    {
        private readonly CachingServiceProvider _cachingServiceProvider;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr hdle, ref ServiceStatus serviceStatus);

        public CachingService(CachingServiceProvider cachingServiceProvider)
        {
            AutoLog = false;

            InitializeComponent();

            _cachingServiceProvider = cachingServiceProvider;

            if (!System.Diagnostics.EventLog.SourceExists("CachingServiceEventSource"))
                System.Diagnostics.EventLog.CreateEventSource("CachingServiceEventSource", "RDMPCachingEngine");

            eventLog.Source = "CachingServiceEventSource";
            eventLog.Log = "RDMPCachingEngine";
            
            /*
            eventLog = new EventLog
            {
                Source = "RDMPCachingService",
                Log = "RDMPCache"
            };

            if (!EventLog.SourceExists(eventLog.Source))
                EventLog.CreateEventSource(eventLog.Source, eventLog.Log);
             */
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                eventLog.WriteEntry("Starting RDMP Caching Service");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            var listener = new EventLogListener(eventLog);
            _cachingServiceProvider.Start(args, listener);
        }

        protected override void OnStop()
        {
            var serviceStatus = new ServiceStatus {dwWaitHint = 10000};

            if (_cachingServiceProvider != null)
                StopCaching(ref serviceStatus);
        }

        private void StopCaching(ref ServiceStatus serviceStatus)
        {
            // Might take a while to stop, so update the service status to 'STOP_PENDING'
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            SetServiceStatus(ServiceHandle, ref serviceStatus);

            eventLog.WriteEntry("Stopping caching host...");

            // blocks until the task is completed
            _cachingServiceProvider.Stop(new EventLogListener(eventLog));

            eventLog.WriteEntry("Caching host stopped");

            // Update service state to Stopped
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        public CachingService(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
