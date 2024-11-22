// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers.Nodes.UsedByProject;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.CohortUI;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal sealed class ExecuteCommandShowSummaryOfCohorts : BasicUICommandExecution
{
    private readonly string _commandName;
    private readonly ExtractableCohort[] _onlyCohorts;

    public ExecuteCommandShowSummaryOfCohorts(IActivateItems activator)
        : base(activator)
    {
        _commandName = "Show Cohort Summary";
    }

    public ExecuteCommandShowSummaryOfCohorts(IActivateItems activator, CohortSourceUsedByProjectNode projectSource) :
        base(activator)
    {
        _commandName = "Show Cohort Summary";

        if (projectSource.IsEmptyNode)
            SetImpossible("Node is empty");
        else
            _onlyCohorts = projectSource.CohortsUsed.Select(static u => u.ObjectBeingUsed).ToArray();
    }

    [UseWithObjectConstructor]
    public ExecuteCommandShowSummaryOfCohorts(IActivateItems activator, ExternalCohortTable externalCohortTable) :
        base(activator)
    {
        _commandName = "Show Detailed Summary of Cohorts";
        _onlyCohorts = activator.CoreChildProvider.GetChildren(externalCohortTable).OfType<ExtractableCohort>()
            .ToArray();
    }

    public override string GetCommandHelp() =>
        "Show information about the cohort lists stored in your cohort database (number of patients etc)";

    public override string GetCommandName() => _commandName;

    public override void Execute()
    {
        var extractableCohortCollection = new ExtractableCohortCollectionUI();
        extractableCohortCollection.SetItemActivator(Activator);
        Activator.ShowWindow(extractableCohortCollection, true);

        if (_onlyCohorts != null)
            extractableCohortCollection.SetupFor(_onlyCohorts);
        else
            extractableCohortCollection.SetupForAllCohorts(Activator);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.AllCohortsNode);
}