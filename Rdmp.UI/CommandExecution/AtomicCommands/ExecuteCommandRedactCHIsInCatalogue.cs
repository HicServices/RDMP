// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandRedactCHIsInCatalogue: BasicUICommandExecution
{
    private readonly ICatalogue _catalogue;
    private readonly IActivateItems _activator;
    public ExecuteCommandRedactCHIsInCatalogue(IActivateItems activator, ICatalogue catalogue) : base(activator)
    {
        _catalogue = catalogue;
        _activator = activator;
    }

    public override void Execute()
    {
        base.Execute();
        var dialog = new RedactChisInCatalogueDialog(_activator, _catalogue);
        dialog.Show();
    }
}
