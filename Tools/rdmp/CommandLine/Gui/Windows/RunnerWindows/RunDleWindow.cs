using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using System.Linq;

namespace Rdmp.Core.CommandLine.Gui.Windows.RunnerWindows
{

    class RunDleWindow : RunEngineWindow<DleOptions>
    {
        private readonly LoadMetadata lmd;

        public RunDleWindow(IBasicActivateItems activator, LoadMetadata lmd)
            : base(activator, () => GetCommand(lmd))
        {
            this.lmd = lmd;
        }

        private static DleOptions GetCommand(LoadMetadata lmd)
        {
            return new DleOptions()
            {
                LoadMetadata = lmd.ID,
                Iterative = false,
            };
        }

        protected override void AdjustCommand(DleOptions opts, CommandLineActivity activity)
        {
            base.AdjustCommand(opts, activity);

            if (lmd.LoadProgresses.Any() && activity == CommandLineActivity.run)
            {
                var lp = (LoadProgress)BasicActivator.SelectOne("Load Progres", lmd.LoadProgresses, null, true);
                if (lp == null)
                    return;

                opts.LoadProgress = lp.ID;

                if (BasicActivator.SelectValueType("Days to Load", typeof(int), lp.DefaultNumberOfDaysToLoadEachTime, out object chosen))
                    opts.DaysToLoad = (int)chosen;
                else
                    return;
            }

        }
    }
}
