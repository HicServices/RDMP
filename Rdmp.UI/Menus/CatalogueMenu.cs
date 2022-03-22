// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.Sharing;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Menus.MenuItems;

namespace Rdmp.UI.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class CatalogueMenu:RDMPContextMenuStrip
    {

        public CatalogueMenu(RDMPContextMenuStripArgs args, Catalogue catalogue):base(args,catalogue)
        {
            var isApiCall = catalogue.IsApiCall();
            
            Add(new ExecuteCommandGenerateMetadataReport(_activator, catalogue) {
                Weight = -99.059f
            },Keys.None,AtomicCommandFactory.Metadata);

            Add(new ExecuteCommandImportCatalogueDescriptionsFromShare(_activator, catalogue)
            {
                Weight = -95.09f,
            },Keys.None,AtomicCommandFactory.Metadata);
            
            
            Add(new ExecuteCommandExportInDublinCoreFormat(_activator, catalogue)
            {
                Weight = -90.10f,
            }, Keys.None,AtomicCommandFactory.Metadata);
            Add(new ExecuteCommandImportDublinCoreFormat(_activator, catalogue)
            {
                Weight = -90.09f,
            }, Keys.None, AtomicCommandFactory.Metadata);

            Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, catalogue,null) {
                OverrideCommandName = "Lookup Table Relationship", Weight = -86.9f}, Keys.None, AtomicCommandFactory.Add);

            if (!isApiCall)
            {
                Items.Add(new DQEMenuItem(_activator, catalogue));

                //create right click context menu
                Add(new ExecuteCommandViewCatalogueExtractionSqlUI(_activator) {
                    Weight = -99.001f,
                    OverrideCommandName = "Catalogue Extraction SQL",
                SuggestedCategory = AtomicCommandFactory.View}.SetTarget(catalogue));
            }

            if (catalogue.LoadMetadata_ID != null)
            {
                var dir = catalogue.LoadMetadata.LocationOfFlatFiles;
                DirectoryInfo dirReal;
                if (dir != null)
                {
                    try
                    {
                        dirReal = new DirectoryInfo(dir);
                    }
                    catch (Exception)
                    {
                        // if the directory name is bad or corrupt
                        return;
                    }
                    Add(new ExecuteCommandOpenInExplorer(_activator, dirReal) { OverrideCommandName = "Open Load Directory"});
                }
            }
            
        }

    }
}
