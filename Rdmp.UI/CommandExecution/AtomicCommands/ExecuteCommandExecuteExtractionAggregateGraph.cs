// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ProjectUI.Graphs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandExecuteExtractionAggregateGraph : BasicUICommandExecution
{
    private readonly ExtractionAggregateGraphObjectCollection _collection;

    public ExecuteCommandExecuteExtractionAggregateGraph(IActivateItems activator,
        ExtractionAggregateGraphObjectCollection collection) : base(activator)
    {
        _collection = collection;

        if (_collection.IsImpossible(out var reason))
            SetImpossible(reason);
    }

    public override string GetCommandHelp() =>
        "Shows a subset of the main graph as it applies to the records that will be extracted";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.AggregateGraph);

    public override void Execute()
    {
        base.Execute();

        Activator.Activate<ExtractionAggregateGraphUI>(_collection);
    }
}