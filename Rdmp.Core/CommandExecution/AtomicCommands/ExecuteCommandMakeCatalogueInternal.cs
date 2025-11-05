// Copyright (c) The University of Dundee 2018-2025
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandMakeCatalogueInternal : BasicCommandExecution, IAtomicCommandWithTarget
{
    private ICatalogue _catalogue;

    [UseWithObjectConstructor]
    public ExecuteCommandMakeCatalogueInternal(IBasicActivateItems itemActivator, ICatalogue catalogue) : this(itemActivator)
    {
        SetCatalogue(catalogue);
    }

    public ExecuteCommandMakeCatalogueInternal(IBasicActivateItems itemActivator) : base(itemActivator)
    {
        UseTripleDotSuffix = true;
    }

    public override string GetCommandHelp() =>
        "Mark the Catalogue as Internal to restrict its extractability";

    public override void Execute()
    {
        if (_catalogue == null)
            SetCatalogue(SelectOne<Catalogue>(BasicActivator.RepositoryLocator.CatalogueRepository));

        if (_catalogue == null)
            return;

        _catalogue.IsInternalDataset = true;
        _catalogue.SaveToDatabase();
        Publish(_catalogue);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => BasicActivator.CoreIconProvider.GetImage(_catalogue, OverlayKind.Internal);

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        switch (target)
        {
            case Catalogue catalogue:
                SetCatalogue(catalogue);
                break;
            default:
                break;
        }

        return this;
    }

    private void SetCatalogue(ICatalogue catalogue)
    {
        ResetImpossibleness();

        _catalogue = catalogue;

        if (catalogue == null)
        {
            SetImpossible("Catalogue cannot be null");
            return;
        }

        if (_catalogue.IsInternalDataset)
            SetImpossible("Catalogue is already marked as Internal");
    }
}