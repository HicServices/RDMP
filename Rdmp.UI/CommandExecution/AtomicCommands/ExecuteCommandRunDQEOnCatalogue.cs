// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.DataQualityUIs;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandRunDQEOnCatalogue:BasicUICommandExecution,IAtomicCommandWithTarget
{
    private Catalogue _catalogue;
        
    [UseWithObjectConstructor]
    public ExecuteCommandRunDQEOnCatalogue(IActivateItems activator,Catalogue catalogue): base(activator)
    {
        _catalogue = catalogue;
    }

    public ExecuteCommandRunDQEOnCatalogue(IActivateItems activator):base(activator)
    {
    }

    public override string GetCommandHelp()
    {
        return "Runs the data quality engine on the dataset using the currently configured validation rules and stores the results in the default DQE results database";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.DQE, OverlayKind.Execute);
    }

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        _catalogue = (Catalogue) target;
        return this;
    }

    public override void Execute()
    {
        base.Execute();

        var c = _catalogue ?? SelectOne<Catalogue>(BasicActivator.RepositoryLocator.CatalogueRepository);

        if(c == null)
            return;

        Activator.Activate<DQEExecutionControlUI, Catalogue>(c);
    }

    public override string GetCommandName()
    {
        return "Data Quality Engine";
    }
}