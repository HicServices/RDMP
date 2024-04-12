// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal class ExecuteCommandUpdateCatalogueDataLocation : BasicUICommandExecution, IAtomicCommand
{
    private readonly Catalogue _catalogue;
    private IActivateItems _activator;

    public string columnNameMapping = null;



    public ExecuteCommandUpdateCatalogueDataLocation(IActivateItems activator, Catalogue catalogue) : base(activator)
    {
        _catalogue = catalogue;
        _activator=activator;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.CatalogueItem, OverlayKind.Edit);

    public override void Execute()
    {
        var ui = new UpdateCatalogueDataLocationUI(_activator,_catalogue);
        ui.Show();
    }
}