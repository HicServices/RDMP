// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.SimpleDialogs;
using ReusableLibraryCode;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Menus
{
    class CatalogueItemsNodeMenu : RDMPContextMenuStrip
    {
        public CatalogueItemsNodeMenu(RDMPContextMenuStripArgs args, CatalogueItemsNode node): base(args, node)
        {
            var iconProvider = _activator.CoreIconProvider;

            ReBrandActivateAs("Bulk Process Catalogue Items",RDMPConcept.CatalogueItem,OverlayKind.Edit);

            Add(new ExecuteCommandAddNewCatalogueItem(_activator, node.Catalogue));
            Items.Add("Paste Clipboard as new Catalogue Items", iconProvider.GetImage(RDMPConcept.Clipboard,OverlayKind.Import), (s, e) => PasteClipboardAsNewCatalogueItems(node.Catalogue));

            Add(new ExecuteCommandReOrderColumns(_activator, node.Catalogue));
            
            Add(new ExecuteCommandGuessAssociatedColumns(_activator, node.Catalogue,null));

            Add(new ExecuteCommandChangeExtractionCategory(_activator,node.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any)));

            Add(new ExecuteCommandImportCatalogueItemDescriptions(_activator, node.Catalogue,null/*pick at runtime*/));
        }
         
        private void PasteClipboardAsNewCatalogueItems(Catalogue c)
        {
            string[] toImport = UsefulStuff.GetInstance().GetArrayOfColumnNamesFromStringPastedInByUser(Clipboard.GetText()).ToArray();

            if (toImport.Any())
                if (MessageBox.Show("Add " + toImport.Length + " new CatalogueItems into Catalogue " + c.Name + "?", "Paste In New Columns?", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    foreach (string name in toImport)
                        new CatalogueItem(RepositoryLocator.CatalogueRepository, c, name);

                    Publish(c);
                }
        }
    }
}