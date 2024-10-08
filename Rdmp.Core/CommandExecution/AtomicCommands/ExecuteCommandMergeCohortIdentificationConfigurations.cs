﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandMergeCohortIdentificationConfigurations : BasicCommandExecution
{
    private CohortIdentificationConfiguration[] ToMerge { get; }

    public ExecuteCommandMergeCohortIdentificationConfigurations(IBasicActivateItems activator,
        CohortIdentificationConfiguration[] toMerge) : base(activator)
    {
        ToMerge = toMerge;
    }

    public override void Execute()
    {
        base.Execute();

        var toMerge = ToMerge;

        if (toMerge is not { Length: > 1 } &&
            !SelectMany(
                BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>(),
                out toMerge)) return;

        if (toMerge is not { Length: > 1 })
        {
            BasicActivator.Show("You must select at least 2 configurations to merge");
            return;
        }

        var merger =
            new CohortIdentificationConfigurationMerger(
                (CatalogueRepository)BasicActivator.RepositoryLocator.CatalogueRepository);
        var result = merger.Merge(toMerge, SetOperation.UNION);

        if (result == null) return;

        BasicActivator.Show($"Successfully created '{result}'");
        Publish(result);
        Emphasise(result);
    }
}