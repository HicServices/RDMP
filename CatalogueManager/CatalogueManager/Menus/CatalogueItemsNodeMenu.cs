// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
     class CatalogueItemsNodeMenu : RDMPContextMenuStrip
    {
        public CatalogueItemsNodeMenu(RDMPContextMenuStripArgs args, CatalogueItemsNode node): base(args, node)
        {
            var iconProvider = _activator.CoreIconProvider;

            ReBrandActivateAs("Bulk Process Catalogue Items...",RDMPConcept.CatalogueItem,OverlayKind.Edit);

            Add(new ExecuteCommandAddNewCatalogueItem(_activator, node.Catalogue));
            Items.Add("Paste Clipboard as new Catalogue Items", iconProvider.GetImage(RDMPConcept.Clipboard,OverlayKind.Import), (s, e) => PasteClipboardAsNewCatalogueItems(node.Catalogue));

            Add(new ExecuteCommandReOrderColumns(_activator, node.Catalogue));
            
            Items.Add("Guess Associated Columns From TableInfo...", iconProvider.GetImage(RDMPConcept.ExtractionInformation,OverlayKind.Problem), (s, e) => GuessAssociatedColumns(node.Catalogue));
        }
         
        private void GuessAssociatedColumns(Catalogue c)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>(), false, false);

            int itemsSeen = 0;
            int itemsQualifying = 0;
            int successCount = 0;
            int failCount = 0;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var selectedTableInfo = (TableInfo)dialog.Selected;

                //get all columns for the selected parent
                ColumnInfo[] guessPool = selectedTableInfo.ColumnInfos.ToArray();
                foreach (CatalogueItem catalogueItem in c.CatalogueItems)
                {
                    itemsSeen++;
                    //catalogue item already has one an associated column so skip it
                    if (catalogueItem.ColumnInfo_ID != null)
                        continue;

                    //guess the associated columns
                    ColumnInfo[] guesses = catalogueItem.GuessAssociatedColumn(guessPool).ToArray();

                    itemsQualifying++;

                    //if there is exactly 1 column that matches the guess
                    if (guesses.Length == 1)
                    {
                        catalogueItem.SetColumnInfo(guesses[0]);
                        successCount++;
                    }
                    else
                    {
                        bool acceptedOne = false;

                        for (int i = 0; i < guesses.Length; i++)
                        //note that this sneakily also deals with case where guesses is empty
                        {
                            DialogResult dialogResult =
                                MessageBox.Show(
                                    "Found multiple matches, approve match?:" + Environment.NewLine + catalogueItem.Name +
                                    Environment.NewLine + guesses[i], "Multiple matched guesses",
                                    MessageBoxButtons.YesNo);

                            if (dialogResult == DialogResult.Yes)
                            {
                                catalogueItem.SetColumnInfo(guesses[i]);
                                successCount++;
                                acceptedOne = true;
                                break;
                            }
                        }

                        if (!acceptedOne)
                            failCount++;
                    }

                }

                MessageBox.Show(
                    "Examined:" + itemsSeen + " CatalogueItems" + Environment.NewLine +
                    "Orphans Seen:" + itemsQualifying + Environment.NewLine +
                    "Guess Success:" + successCount + Environment.NewLine +
                    "Guess Failed:" + failCount + Environment.NewLine
                    );

                Publish(c);
            }
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