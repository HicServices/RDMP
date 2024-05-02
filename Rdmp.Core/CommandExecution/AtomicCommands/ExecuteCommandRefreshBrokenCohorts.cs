// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Clears the <see cref="DataExportChildProvider.ForbidListedSources" /> list and triggers a refresh
///     which results in all previously broken cohort sources to be re-evaluated for existence
/// </summary>
public class ExecuteCommandRefreshBrokenCohorts : BasicCommandExecution
{
    private readonly ExternalCohortTable _ect;

    [UseWithObjectConstructor]
    public ExecuteCommandRefreshBrokenCohorts(IBasicActivateItems activator,
        [DemandsInitialization(
            "The specific ExternalCohortTable to attempt to refresh connections to or null to refresh all ExternalCohortTables")]
        ExternalCohortTable ect = null) : base(activator)
    {
        _ect = ect;

        if (activator.CoreChildProvider is not DataExportChildProvider dx)
        {
            SetImpossible($"{nameof(activator.CoreChildProvider)} is not a {nameof(DataExportChildProvider)}");
            return;
        }

        // if we only want to clear one
        if (ect != null)
        {
            if (!dx.ForbidListedSources.Contains(ect)) SetImpossible($"'{ect}' is not broken");
        }
        else
        {
            // we want to clear all of them
            if (!dx.ForbidListedSources.Any())
                SetImpossible("There are no broken ExternalCohortTable to clear status on");
        }
    }

    public override void Execute()
    {
        base.Execute();

        var dx = (DataExportChildProvider)BasicActivator.CoreChildProvider;
        var toPublish = _ect ?? dx.ForbidListedSources.FirstOrDefault();

        // there's nothing to clear now anyway
        if (toPublish == null)
            return;

        if (_ect != null)
            dx.ForbidListedSources.Remove(_ect);
        else
            dx.ForbidListedSources.Clear();


        Publish(toPublish);
    }
}