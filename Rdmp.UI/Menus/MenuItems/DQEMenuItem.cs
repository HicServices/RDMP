// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Icons;

namespace Rdmp.UI.Menus.MenuItems;

class DQEMenuItem:RDMPToolStripMenuItem
{
    private readonly Catalogue _catalogue;
    readonly IExternalDatabaseServer _dqeServer;
    public DQEMenuItem(IActivateItems activator, Catalogue catalogue): base(activator, "Data Quality Engine...")
    {
        _catalogue = catalogue;

        _dqeServer = activator.RepositoryLocator.CatalogueRepository.GetDefaultFor(PermissableDefaults.DQE);

        Image = activator.CoreIconProvider.GetImage(RDMPConcept.DQE).ImageToBitmap();

        Text = _dqeServer == null ? "Create DQE Database..." : "Data Quality Engine...";
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);

        if (_dqeServer == null)
        {
            var cmdCreateDb = new ExecuteCommandCreateNewExternalDatabaseServer(_activator, new DataQualityEnginePatcher(), PermissableDefaults.DQE);
            cmdCreateDb.Execute();
        }
        else
        {
            Exception ex;
            if (!_dqeServer.Discover(DataAccessContext.InternalDataProcessing).Server.RespondsWithinTime(5, out ex))
                ExceptionViewer.Show(ex);
            else
                new ExecuteCommandRunDQEOnCatalogue(_activator, _catalogue).Execute();
        }

            
    }
}