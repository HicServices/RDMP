// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.Sharing;
using CatalogueManager.Icons.IconProvision;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class LoadMetadataMenu:RDMPContextMenuStrip
    {
        public LoadMetadataMenu(RDMPContextMenuStripArgs args, LoadMetadata loadMetadata) : base(args, loadMetadata)
        {
            Add(new ExecuteCommandViewLoadDiagram(_activator,loadMetadata));

            Add(new ExecuteCommandEditLoadMetadataDescription(_activator, loadMetadata));

            Add(new ExecuteCommandExportObjectsToFileUI(_activator, new IMapsDirectlyToDatabaseTable[] {loadMetadata}));

            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandOverrideRawServer(_activator, loadMetadata));
            Add(new ExecuteCommandCreateNewLoadMetadata(_activator));
            
            ReBrandActivateAs("Check and Execute",RDMPConcept.LoadMetadata,OverlayKind.Execute);
        }
    }
}
