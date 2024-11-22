// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SubComponents.Graphs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandViewCohortAggregateGraph : BasicUICommandExecution
{
    private readonly CohortSummaryAggregateGraphObjectCollection _collection;
    private const float DefaultWeight = 2.6f;

    public ExecuteCommandViewCohortAggregateGraph(IActivateItems activator,
        CohortSummaryAggregateGraphObjectCollection collection) : base(activator)
    {
        Weight = DefaultWeight;

        _collection = collection;

        if (collection.CohortIfAny != null && collection.CohortIfAny.IsJoinablePatientIndexTable())
            SetImpossible("Graphs cannot be generated for Patient Index tables");
    }

    public override string GetCommandHelp() =>
        "Shows a subset of the main graph as it applies to the people in your cohort";

    public override string GetCommandName() => _collection.Graph.Name;

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.AggregateGraph);

    public override void Execute()
    {
        base.Execute();

        Activator.Activate<CohortSummaryAggregateGraphUI>(_collection);
    }
}