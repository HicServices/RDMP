// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandAddNewAggregateGraph : BasicCommandExecution, IAtomicCommand
{
    private readonly Catalogue _catalogue;
    private readonly string _name;

    public ExecuteCommandAddNewAggregateGraph(IBasicActivateItems activator, Catalogue catalogue, string name = null) :
        base(activator)
    {
        _catalogue = catalogue;
        _name = name;
        if (_catalogue != null && _catalogue.GetAllExtractionInformation(ExtractionCategory.Any).All(ei => ei.ColumnInfo == null))
            SetImpossible("Catalogue has no extractable columns");
    }

    public override string GetCommandHelp() =>
        "Add a new graph for understanding the data in a dataset e.g. number of records per year";

    public override void Execute()
    {
        base.Execute();

        var c = _catalogue;
        var name = _name;

        if (c == null)
        {
            if (BasicActivator.SelectObject(new DialogArgs
            {
                WindowTitle = "Add Aggregate Graph",
                TaskDescription = "Select which Catalogue you want to add the graph to."
            }, BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>(), out var selected))
                c = selected;
            else
                // user cancelled selecting a Catalogue
                return;
        }

        if (name == null && BasicActivator.IsInteractive)
            if (!BasicActivator.TypeText(new DialogArgs
            {
                WindowTitle = "Graph Name",
                EntryLabel = "Name"
            }, 255, null, out name, false))
                // user cancelled typing a name for the graph
                return;

        var newAggregate = new AggregateConfiguration(BasicActivator.RepositoryLocator.CatalogueRepository, c, name ??
            $"New Aggregate {Guid.NewGuid()}");
        Publish(_catalogue);
        Activate(newAggregate);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.AggregateGraph, OverlayKind.Add);
}