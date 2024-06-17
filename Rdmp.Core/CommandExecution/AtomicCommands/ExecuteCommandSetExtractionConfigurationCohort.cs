// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandSetExtractionConfigurationCohort : BasicCommandExecution, IAtomicCommand
{
    private readonly ExtractionConfiguration _extractionConfiguration;
    private readonly ExtractableCohort _extractableCohort;

    public ExecuteCommandSetExtractionConfigurationCohort(IBasicActivateItems activator, ExtractionConfiguration extractionConfiguration, ExtractableCohort cohort) : base(activator)
    {
        _extractionConfiguration = extractionConfiguration;
        _extractableCohort = cohort;
    }


    public override void Execute()
    {
        base.Execute();
        _extractionConfiguration.Cohort_ID = _extractableCohort.ID;
        _extractionConfiguration.SaveToDatabase();
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
    iconProvider.GetImage(RDMPConcept.ExtractionConfiguration, OverlayKind.None);

}
