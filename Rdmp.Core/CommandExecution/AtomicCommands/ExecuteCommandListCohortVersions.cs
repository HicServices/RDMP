// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using Rdmp.Core.Curation.Data.Cohort;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandListCohortVersions: BasicCommandExecution, IAtomicCommand
{
    readonly CohortIdentificationConfiguration _cic;
    readonly IBasicActivateItems _activator;

    public ExecuteCommandListCohortVersions(IBasicActivateItems activator, CohortIdentificationConfiguration cic):base(activator)
    {
        _cic = cic;
        _activator = activator;
    }

    public override void Execute()
    {
        base.Execute();
        var versions = _cic.GetVersions();
        var outputDictionary = new Dictionary<string, string>();
        foreach ( var version in versions )
        {
            outputDictionary.Add(version.Name, version.ID.ToString());
        }

        var output = string.Join(Environment.NewLine,
            outputDictionary.Select(kvp => $"{kvp.Value}:{kvp.Key}")
                .OrderBy(s => s));
        BasicActivator.Show(output);

    }

}
