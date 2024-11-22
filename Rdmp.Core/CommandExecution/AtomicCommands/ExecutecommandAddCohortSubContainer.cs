// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandAddCohortSubContainer : BasicCommandExecution
{
    private readonly CohortAggregateContainer _container;

    public ExecuteCommandAddCohortSubContainer(IBasicActivateItems activator, CohortAggregateContainer container) :
        base(activator)
    {
        Weight = 0.12f;
        _container = container;

        if (container.ShouldBeReadOnly(out var reason)) SetImpossible(reason);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.CohortAggregateContainer, OverlayKind.Add);

    public override void Execute()
    {
        base.Execute();

        var newContainer =
            new CohortAggregateContainer(BasicActivator.RepositoryLocator.CatalogueRepository, SetOperation.UNION);
        _container.AddChild(newContainer);
        Publish(_container);
        Emphasise(newContainer);
    }
}