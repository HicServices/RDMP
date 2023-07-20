// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Governance;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandAddCatalogueToGovernancePeriod:BasicCommandExecution
{
    private readonly GovernancePeriod _governancePeriod;
    private readonly ICatalogue[] _catalogues;

    public ExecuteCommandAddCatalogueToGovernancePeriod(IBasicActivateItems activator, GovernancePeriod governancePeriod, ICatalogue c) : this(activator, governancePeriod, new[]{c})
    {
    }

    public ExecuteCommandAddCatalogueToGovernancePeriod(IBasicActivateItems activator, GovernancePeriod governancePeriod, IEnumerable<ICatalogue> catalogues):base(activator)
    {
        _governancePeriod = governancePeriod;
        _catalogues = catalogues.Except(_governancePeriod.GovernedCatalogues).ToArray();

        if(!_catalogues.Any())
            SetImpossible("All Catalogues are already in the Governance Period");
    }

    public override void Execute()
    {
        base.Execute();

        foreach(var catalogue in _catalogues)
            BasicActivator.RepositoryLocator.CatalogueRepository.GovernanceManager.Link(_governancePeriod,catalogue);

        Publish(_governancePeriod);
    }
}