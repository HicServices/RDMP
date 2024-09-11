// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;


namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateVersionOfCohortConfiguration : BasicCommandExecution, IAtomicCommand
{
    readonly CohortIdentificationConfiguration _cic;
    readonly IBasicActivateItems _activator;
    readonly string _name;
    readonly string _description;

    public ExecuteCommandCreateVersionOfCohortConfiguration(IBasicActivateItems activator, CohortIdentificationConfiguration cic, string name = null, string description = null) : base(activator)
    {
        _cic = cic;
        _activator = activator;
        _name = name;
        _description = description;
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
        if (_description is not null)
        {
            var createdItem = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("ClonedFrom_ID", _cic.ID).Where(cic => cic.Name == _name);
            if (createdItem.Any())
            {
                createdItem.First().Description = _description;
                createdItem.First().SaveToDatabase();

                var associations = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ProjectCohortIdentificationConfigurationAssociation>("CohortIdentificationConfiguration_ID", _cic.ID);
                foreach (var association in associations)
                {
                    var link = new ProjectCohortIdentificationConfigurationAssociation(_activator.RepositoryLocator.DataExportRepository, (Project)association.Project, createdItem.First());
                    link.SaveToDatabase();
                }

                Publish(createdItem.First());
            }
        }

    }
}
