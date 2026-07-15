// Surfaces the build breakdown-by-groups command in the RDMP desktop GUI (right-click a cohort
// identification configuration). Two entries: the SHARE preset (inputs resolved by name via
// SharePreset, prompting only for anything missing) and a choose-inputs variant that always prompts.
// The same command class is auto-discovered for the CLI (`rdmp cmd ExportCohortBuildBreakDownByGroups`).

using System.Collections.Generic;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;

namespace RdmpCohortBuildBreakdownByGroups;

public class CohortBuildBreakdownByGroupsPluginUserInterface : PluginUserInterface
{
    public CohortBuildBreakdownByGroupsPluginUserInterface(IBasicActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override IEnumerable<IAtomicCommand> GetAdditionalRightClickMenuItems(object o)
    {
        if (o is not CohortIdentificationConfiguration cic)
            yield break;

        SharePreset.TryResolve(BasicActivator.RepositoryLocator.CatalogueRepository,
            out var groupColumn, out var lookupKey, out var lookupLabel, out var lookupGrouping);

        yield return new ExecuteCommandExportCohortBuildBreakDownByGroups(BasicActivator, cic,
            groupColumn, lookupKey, lookupLabel, lookupGroupingColumn: lookupGrouping)
        {
            OverrideCommandName = "Export Build Breakdown By Groups (SHARE preset)"
        };

        yield return new ExecuteCommandExportCohortBuildBreakDownByGroups(BasicActivator, cic)
        {
            OverrideCommandName = "Export Build Breakdown By Groups (choose inputs)"
        };
    }
}
