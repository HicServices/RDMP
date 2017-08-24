using System.ServiceProcess;
using RDMPAutomationService;

namespace RDMPAutomationService
{
    public class RDMPAutomationService : ServiceBase
    {
        public RDMPAutomationService()
        {
            ServiceName = Program.ServiceName;
        }

        protected override void OnStart(string[] args)
        {
            Program.Start(args);
        }

        protected override void OnStop()
        {
            Program.Stop();
        }
    }
}