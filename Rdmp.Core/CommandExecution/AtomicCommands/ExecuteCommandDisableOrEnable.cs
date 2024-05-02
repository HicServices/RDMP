// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandDisableOrEnable : BasicCommandExecution, IAtomicCommand
{
    private readonly IDisableable[] _targets;

    public ExecuteCommandDisableOrEnable(IBasicActivateItems itemActivator, IDisableable target) : base(itemActivator)
    {
        UpdateViabilityForTarget(target);
        _targets = new[] { target };

        Weight = 50.1f;
    }

    public ExecuteCommandDisableOrEnable(IBasicActivateItems activator, IDisableable[] disableables) : base(activator)
    {
        _targets = disableables;

        if (!disableables.Any())
        {
            SetImpossible("No objects selected");
            return;
        }

        if (disableables.All(d => d.IsDisabled) || disableables.All(d => !d.IsDisabled))
            foreach (var d in _targets)
                UpdateViabilityForTarget(d);
        else
            SetImpossible("All objects must be in the same disabled/enabled state");

        Weight = 50.1f;
    }

    private void UpdateViabilityForTarget(IDisableable target)
    {
        switch (target)
        {
            //don't let them disable the root container
            case CohortAggregateContainer container when container.IsRootContainer() && !container.IsDisabled:
                SetImpossible("You cannot disable the root container of a cic");
                break;
            case AggregateConfiguration { IsCohortIdentificationAggregate: false }:
                SetImpossible("Only cohort identification aggregates can be disabled");
                break;
            case AggregateConfiguration aggregateConfiguration:
            {
                if (aggregateConfiguration.IsJoinablePatientIndexTable() && !aggregateConfiguration.IsDisabled)
                    SetImpossible("Joinable Patient Index Tables cannot be disabled");
                break;
            }
        }

        if (target is IMightBeReadOnly ro && ro.ShouldBeReadOnly(out var reason))
            SetImpossible(reason);
    }

    public override void Execute()
    {
        base.Execute();

        foreach (var d in _targets)
        {
            d.IsDisabled = !d.IsDisabled;
            d.SaveToDatabase();
        }

        var toRefresh = _targets.FirstOrDefault();

        if (toRefresh != null)
            Publish((DatabaseEntity)toRefresh);
    }

    public override string GetCommandName()
    {
        if (_targets.Length == 1)
            return _targets[0].IsDisabled ? "Enable" : "Disable";

        return _targets.Length > 1 ? _targets.All(d => d.IsDisabled) ? "Enable All" : "Disable All" : "Enable All";
    }
}