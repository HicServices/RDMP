// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;

//Based off of ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration

public class ExecuteCommandCreateHoldoutCohort : CohortCreationCommandExecution
{
    private readonly CohortIdentificationConfiguration _cic;
    //private readonly bool _desiredFreezeState;

    public ExecuteCommandCreateHoldoutCohort(IBasicActivateItems activator,
        CohortIdentificationConfiguration cic) : base(activator)
    {
        _cic = cic;
    }

    public override string GetCommandHelp() => "TODO";
    

    public override string GetCommandName() => "Create Holdout Cohort";

    public override void Execute()
    {
        base.Execute();
        var request = GetCohortHoldoutCreationRequest(ExtractableCohortAuditLogBuilder.GetDescription(_cic));
        //here
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.AllCohortsNode, OverlayKind.Add);
}