// Surfaces the cohort build health board breakdown in the RDMP desktop GUI (right-click a cohort
// identification configuration). The same command class is auto-discovered for the CLI
// (`rdmp cmd ExportCohortBuildHealthBoardBreakdown`), so one definition serves both.

using System.Collections.Generic;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;

namespace RdmpCohortBuildHealthBoardBreakdown;

public class CohortBuildHealthBoardBreakdownPluginUserInterface : PluginUserInterface
{
    public CohortBuildHealthBoardBreakdownPluginUserInterface(IBasicActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override IEnumerable<IAtomicCommand> GetAdditionalRightClickMenuItems(object o)
    {
        if (o is CohortIdentificationConfiguration cic)
            yield return new ExecuteCommandExportCohortBuildHealthBoardBreakdown(BasicActivator, cic);
    }
}
