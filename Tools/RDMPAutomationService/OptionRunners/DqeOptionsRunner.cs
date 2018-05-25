using RDMPAutomationService.Logic.DQE;
using RDMPAutomationService.Options;

namespace RDMPAutomationService.OptionRunners
{
    class DqeOptionsRunner
    {
        public int Run(DqeOptions opts)
        {
            opts.LoadFromAppConfig();
            opts.DoStartup();
            var run = new AutomatedDQERun(opts);
            run.RunTask();
            return 0;
        }
    }
}