// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using ReusableLibraryCode.Icons;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Menus.MenuItems
{
    internal class SetDumpServerMenuItem : RDMPToolStripMenuItem
    {
        private readonly ITableInfo _tableInfo;
        private ExternalDatabaseServer[] _availableServers;

        public SetDumpServerMenuItem(IActivateItems activator, ITableInfo tableInfo): base(activator,"Add Dump Server")
        {
            _tableInfo = tableInfo;

            //cannot set server if we already have one
            Enabled = tableInfo.IdentifierDumpServer_ID == null;
            Image = activator.CoreIconProvider.GetImage(RDMPConcept.ExternalDatabaseServer, OverlayKind.Add).ToBitmap();

            var img = CatalogueIcons.ExternalDatabaseServer_IdentifierDump;
            var overlay = new IconOverlayProvider();

            var cataRepo = activator.RepositoryLocator.CatalogueRepository;

            _availableServers = cataRepo.GetAllDatabases<IdentifierDumpDatabasePatcher>();

            var miUseExisting = new ToolStripMenuItem("Use Existing...", overlay.GetOverlayNoCache(img, OverlayKind.Link).ImageToBitmap(),UseExisting);
            miUseExisting.Enabled = _availableServers.Any();

            DropDownItems.Add(miUseExisting);
            DropDownItems.Add("Create New...", overlay.GetOverlayNoCache(img, OverlayKind.Add).ImageToBitmap(), CreateNewIdentifierDumpServer);

        }

        private void UseExisting(object sender, EventArgs e)
        {
            if(_activator.SelectObject(new DialogArgs
            {
                TaskDescription = "In which server should the table dump unloaded identifiable data during data loads.  This server must be already configured correctly to support identifier dumping (i.e. be an IdentifierDump).",
            },_availableServers,out var selected))
            {
                _tableInfo.IdentifierDumpServer_ID = selected.ID;
                _tableInfo.SaveToDatabase();

                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs((TableInfo)_tableInfo));
            }
        }

        private void CreateNewIdentifierDumpServer(object sender, EventArgs e)
        {
            var cmd = new ExecuteCommandCreateNewExternalDatabaseServer(_activator, new IdentifierDumpDatabasePatcher(), PermissableDefaults.IdentifierDumpServer_ID);
            cmd.Execute();

            if (cmd.ServerCreatedIfAny != null)
            {
                _tableInfo.IdentifierDumpServer_ID = cmd.ServerCreatedIfAny.ID;
                _tableInfo.SaveToDatabase();

                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs((TableInfo)_tableInfo));
            }
        }
    }
}