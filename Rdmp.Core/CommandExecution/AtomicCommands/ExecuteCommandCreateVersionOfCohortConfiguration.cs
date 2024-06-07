using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using Spectre.Console;
using System.Linq;


namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateVersionOfCohortConfiguration : BasicCommandExecution, IAtomicCommand
{
    readonly CohortIdentificationConfiguration _cic;
    readonly IBasicActivateItems _activator;
    readonly string _name;

    public ExecuteCommandCreateVersionOfCohortConfiguration(IBasicActivateItems activator, CohortIdentificationConfiguration cic, string name = null) : base(activator)
    {
        _cic = cic;
        _activator = activator;
        _name = name;
    }


    public override void Execute()
    {
        base.Execute();
        int? version = 1;

        var previousClones = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("ClonedFrom_ID", _cic.ID).Where(cic => cic.Version != null);
        if (previousClones.Any())
        {
            version = previousClones.Select(pc => pc.Version).Where(v => v != null).Max() + 1;
        }
        var cmd = new ExecuteCommandCloneCohortIdentificationConfiguration(_activator, _cic, _name, version, true);
        cmd.Execute();
        //todo freese the version
    }
}
