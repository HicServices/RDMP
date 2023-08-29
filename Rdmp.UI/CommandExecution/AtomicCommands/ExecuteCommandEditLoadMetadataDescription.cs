// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal class ExecuteCommandEditLoadMetadataDescription : BasicUICommandExecution, IAtomicCommand
{
    private LoadMetadata _loadMetadata;

    public ExecuteCommandEditLoadMetadataDescription(IActivateItems activator, LoadMetadata loadMetadata) :
        base(activator)
    {
        _loadMetadata = loadMetadata;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.LoadMetadata, OverlayKind.Edit);

    public override string GetCommandName() => "Edit Description";

    public override void Execute()
    {
        Activator.Activate<LoadMetadataUI, LoadMetadata>(_loadMetadata);
    }
}