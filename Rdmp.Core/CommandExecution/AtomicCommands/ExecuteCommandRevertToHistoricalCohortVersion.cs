// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.



using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandRevertToHistoricalCohortVersion : BasicCommandExecution, IAtomicCommand
{

    private readonly IBasicActivateItems _activator;
    private CohortIdentificationConfiguration _configuration;
    private readonly CohortIdentificationConfiguration _historicalConfiguration;

    /// <summary>
    /// Set a cohort configuration to match a previously saved version of that cohort
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="cic"></param>
    /// <param name="historicalCic"></param>
    public ExecuteCommandRevertToHistoricalCohortVersion(IBasicActivateItems activator, CohortIdentificationConfiguration cic, CohortIdentificationConfiguration historicalCic): base(activator)
    {
        _activator = activator;
        _configuration = cic;
        _historicalConfiguration = historicalCic;
    }


    public override void Execute()
    {
        if (!_activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("ClonedFrom_ID", _configuration.ID).Where(cic => cic.Version != null && cic.ID == _historicalConfiguration.ID).Any())
        {
            throw new System.Exception("Historical configuration is not derived from this cohort configuration");
        }
        base.Execute();
        var clone = _historicalConfiguration.CloneIntoExistingConfiguration(ThrowImmediatelyCheckNotifier.Quiet, _configuration,false);
        Publish(clone);
        Emphasise(clone);
    }
}
