// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Adds a new <see cref="ExtractionProgress"/> for windowed date based extractions of a dataset.
/// </summary>
public class ExecuteCommandAddExtractionProgress : BasicCommandExecution
{
    private readonly ISelectedDataSets _sds;

    public ExecuteCommandAddExtractionProgress(IBasicActivateItems activator, ISelectedDataSets sds) : base(activator)
    {
        _sds = sds;

        if (_sds.ExtractionProgressIfAny != null)
        {
            SetImpossible("Catalogue already has an ExtractionProgress");
            return;
        }

        if (_sds.GetCatalogue()?.TimeCoverage_ExtractionInformation_ID == null)
        {
            SetImpossible("Catalogue does not have a time coverage field configured");
        }
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.ExtractionProgress, OverlayKind.Add);

    public override void Execute()
    {
        base.Execute();

        var ep = new ExtractionProgress(BasicActivator.RepositoryLocator.DataExportRepository, _sds);
        Publish(ep);
        Activate(ep);
    }
}