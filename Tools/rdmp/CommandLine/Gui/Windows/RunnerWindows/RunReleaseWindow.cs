using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.DataExport.Data;
using System;

namespace Rdmp.Core.CommandLine.Gui.Windows.RunnerWindows
{
    class RunReleaseWindow : RunEngineWindow<ReleaseOptions>
    {
        public RunReleaseWindow(IBasicActivateItems activator, IProject project):base(activator,()=>GetCommand(project))
        {

        }

        private static ReleaseOptions GetCommand(IProject project)
        {

			return new ReleaseOptions()
			{
				Pipeline = _pipelineSelectionUI1.Pipeline == null ? 0 : _pipelineSelectionUI1.Pipeline.ID,
				Configurations = _configurations.Where(c => tlvReleasePotentials.IsChecked(c) || tlvReleasePotentials.IsCheckedIndeterminate(c)).Select(ec => ec.ID).ToArray(),
				SelectedDataSets = _selectedDataSets.All(tlvReleasePotentials.IsChecked) ? new int[0] : tlvReleasePotentials.CheckedObjects.OfType<ISelectedDataSets>().Select(sds => sds.ID).ToArray(),
				Command = activityRequested,
				ReleaseGlobals = tlvReleasePotentials.IsChecked(_globalsNode),
			};

			yield return new ExecuteCommandRunConsoleGuiView(_activator,
				() => new RunRunEngineWindow(_activator, () => opts))
			{ OverrideCommandName = "Run Load" };
		}

        protected override void AdjustCommand(ReleaseOptions opts, CommandLineActivity activity)
        {
            base.AdjustCommand(opts, activity);


        }
    }
}
