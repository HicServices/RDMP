// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandAddNewFilterContainer : BasicCommandExecution
{
    private readonly IRootFilterContainerHost _host;
    private readonly IContainer _container;
    internal const string FiltersCannotBeAddedToApiCalls = "Filters cannot be added to API calls";
    private const float DefaultWeight = 1.1f;

    public ExecuteCommandAddNewFilterContainer(IBasicActivateItems activator, IRootFilterContainerHost host) :
        base(activator)
    {
        Weight = DefaultWeight;
        _host = host;

        if (host.RootFilterContainer_ID != null)
            SetImpossible("There is already a root filter container on this object");

        if (host is AggregateConfiguration ac)
        {
            if (ac.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null)
                SetImpossible("Aggregate is set to use another's filter container tree");

            if (ac.Catalogue.IsApiCall())
                SetImpossible(FiltersCannotBeAddedToApiCalls);
        }

        SetImpossibleIfReadonly(host);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.FilterContainer, OverlayKind.Add);
    }

    public ExecuteCommandAddNewFilterContainer(IBasicActivateItems activator, IContainer container) : base(activator)
    {
        _container = container;

        SetImpossibleIfReadonly(container);
    }

    public override void Execute()
    {
        base.Execute();

        var factory = (_container?.GetFilterFactory() ?? _host?.GetFilterFactory()) ??
                      throw new Exception("Unable to determine FilterFactory, is host and container null?");
        var newContainer = factory.CreateNewContainer();

        if (_host != null)
        {
            _host.RootFilterContainer_ID = newContainer.ID;
            _host.SaveToDatabase();
        }
        else
        {
            if (_container == null)
                throw new Exception("Command should take container or host but both were null");

            _container.AddChild(newContainer);
        }


        Publish(_host ?? (IMapsDirectlyToDatabaseTable)newContainer);
        Emphasise(newContainer);
    }
}