// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Validation;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandConfigureCatalogueValidationRules : BasicUICommandExecution,IAtomicCommandWithTarget
{
    private Catalogue _catalogue;

    public ExecuteCommandConfigureCatalogueValidationRules(IActivateItems activator) : base(activator)
    {
            
    }

    public override string GetCommandHelp()
    {
        return "Allows you to specify validation rules for columns in the dataset and pick the time coverage/pivot fields";
    }

    public override string GetCommandName()
    {
        return "Validation Rules...";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.DQE, OverlayKind.Edit);
    }

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        _catalogue = (Catalogue) target;
        return this;
    }

    public override void Execute()
    {
        base.Execute();

        if (_catalogue == null)
            _catalogue = SelectOne<Catalogue>(Activator.RepositoryLocator.CatalogueRepository);

        if(_catalogue == null)
            return;

        Activator.Activate<ValidationSetupUI, Catalogue>(_catalogue);
    }
}