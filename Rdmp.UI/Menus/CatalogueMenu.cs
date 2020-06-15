// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Providers;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.Sharing;
using Rdmp.UI.Menus.MenuItems;

namespace Rdmp.UI.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class CatalogueMenu:RDMPContextMenuStrip
    {
        public CatalogueMenu(RDMPContextMenuStripArgs args, Catalogue catalogue):base(args,catalogue)
        {
            //create right click context menu
            Add(new ExecuteCommandViewCatalogueExtractionSql(_activator).SetTarget(catalogue));

            Items.Add(new ToolStripSeparator());

            var addItem = new ToolStripMenuItem("Add", null);
            Add(new ExecuteCommandAddNewSupportingSqlTable(_activator, catalogue), Keys.None, addItem);
            Add(new ExecuteCommandAddNewSupportingDocument(_activator, catalogue), Keys.None, addItem);
            Add(new ExecuteCommandAddNewAggregateGraph(_activator, catalogue), Keys.None, addItem);
            Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, catalogue,null), Keys.None, addItem);
            Add(new ExecuteCommandAddNewCatalogueItem(_activator, catalogue), Keys.None, addItem);
            Items.Add(addItem);

            Add(new ExecuteCommandGenerateMetadataReport(_activator, catalogue));

            var extractability = new ToolStripMenuItem("Extractability");
            Add(new ExecuteCommandChangeExtractability(_activator, catalogue),Keys.None,extractability);
            Add(new ExecuteCommandMakeCatalogueProjectSpecific(_activator).SetTarget(catalogue),Keys.None,extractability);
            Add(new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator, catalogue),Keys.None,extractability);
            Add(new ExecuteCommandSetExtractionIdentifier(_activator,catalogue),Keys.None,extractability);

            Items.Add(extractability);

            var extract = new ToolStripMenuItem("Import/Export Descriptions");
            Add(new ExecuteCommandExportObjectsToFileUI(_activator, new[] {catalogue}),Keys.None,extract);
            Add(new ExecuteCommandImportCatalogueDescriptionsFromShare(_activator, catalogue),Keys.None,extract);
            Add(new ExecuteCommandExportInDublinCoreFormat(_activator, catalogue),Keys.None,extract);
            Add(new ExecuteCommandImportDublinCoreFormat(_activator, catalogue), Keys.None, extract);
            Add(new ExecuteCommandExtractMetadata(_activator, new []{ catalogue},null,null,null,false){OverrideCommandName = "Custom..."}, Keys.None, extract);
            Items.Add(extract);

            Items.Add(new DQEMenuItem(_activator,catalogue));

            
        }

    }
}
