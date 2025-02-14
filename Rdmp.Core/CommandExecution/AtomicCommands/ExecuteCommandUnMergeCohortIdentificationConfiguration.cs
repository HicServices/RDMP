// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandUnMergeCohortIdentificationConfiguration : BasicCommandExecution
{
    private readonly CohortAggregateContainer _target;

    [UseWithObjectConstructor]
    public ExecuteCommandUnMergeCohortIdentificationConfiguration(IBasicActivateItems activator,
        CohortIdentificationConfiguration cic) :
        this(activator, cic?.RootCohortAggregateContainer)
    {
    }

    public ExecuteCommandUnMergeCohortIdentificationConfiguration(IBasicActivateItems activator,
        CohortAggregateContainer container) : base(activator)
    {
        _target = container;
        Weight = 0.3f;

        if (_target == null)
        {
            SetImpossible("No root container");
            return;
        }

        if (!_target.IsRootContainer())
        {
            SetImpossible("Only root containers can be unmerged");
            return;
        }

        if (_target.GetAggregateConfigurations().Length != 0)
        {
            SetImpossible("Container must contain only subcontainers (i.e. no aggregate sets)");
            return;
        }

        if (_target.GetSubContainers().Length <= 1)
        {
            SetImpossible("Container must have 2 or more immediate subcontainers for unmerging");
        }
    }

    public override void Execute()
    {
        base.Execute();

        if (!BasicActivator.Confirm("Generate new Cohort Identification Configurations for each container?",
                "Confirm UnMerge")) return;

        var merger =
            new CohortIdentificationConfigurationMerger(
                (CatalogueRepository)BasicActivator.RepositoryLocator.CatalogueRepository);
        var results = merger.UnMerge(_target);

        if (results?.Any() == true)
        {
            BasicActivator.Show(
                $"Created {results.Length} new configurations:{Environment.NewLine} {string.Join(Environment.NewLine, results.Select(r => r.Name))}");
            Publish(results.First());
            Emphasise(results.First());
        }
    }
}