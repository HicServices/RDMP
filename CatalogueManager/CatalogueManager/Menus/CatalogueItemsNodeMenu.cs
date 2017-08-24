using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableLibraryCode;

namespace CatalogueManager.Menus
{
    public class CatalogueItemsNodeMenu : RDMPContextMenuStrip
    {
        public CatalogueItemsNodeMenu(IActivateItems activator, CatalogueItemsNode node) : base(activator, null)
        {
            var iconProvider = activator.CoreIconProvider;

            Items.Add(new AddCatalogueItemMenuItem(activator, node.Catalogue));
            Items.Add("Bulk Process Catalogue Items...", null, (s, e) => BulkProcessCatalogueItems(node.Catalogue));
            Items.Add("Paste Clipboard as new Catalogue Items", iconProvider.GetImage(RDMPConcept.Clipboard,OverlayKind.Import), (s, e) => PasteClipboardAsNewCatalogueItems(node.Catalogue));
            Items.Add("Re-Order Columns", iconProvider.GetImage(RDMPConcept.ReOrder),(s, e) => ReOrderCatalogueItems(node.Catalogue));
            Items.Add("Import Catalogue Item...", null, (s, e) => ImportCatalogueItem(node.Catalogue));
            Items.Add("Guess Associated Columns From TableInfo...", iconProvider.GetImage(RDMPConcept.ExtractionInformation,OverlayKind.Problem), (s, e) => GuessAssociatedColumns(node.Catalogue));
        }

        private void ReOrderCatalogueItems(Catalogue catalogue)
        {
            _activator.ActivateReOrderCatalogueItems(catalogue);
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

                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(c));
            }
        }

        private void ImportCatalogueItem(Catalogue c)
        {
            ImportCloneOfCatalogueItem cloner = new ImportCloneOfCatalogueItem(c);
            cloner.ShowDialog();

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(c));
        }

        private void PasteClipboardAsNewCatalogueItems(Catalogue c)
        {
            string[] toImport = UsefulStuff.GetInstance().GetArrayOfColumnNamesFromStringPastedInByUser(Clipboard.GetText()).ToArray();

            if (toImport.Any())
                if (MessageBox.Show("Add " + toImport.Length + " new CatalogueItems into Catalogue " + c.Name + "?", "Paste In New Columns?", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    foreach (string name in toImport)
                        new CatalogueItem(RepositoryLocator.CatalogueRepository, c, name);

                    _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(c));
                }
        }

        private void BulkProcessCatalogueItems(Catalogue c)
        {
            BulkProcessCatalogueItems bulkProcess = new BulkProcessCatalogueItems(c);
            bulkProcess.ShowDialog();

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(c));
        }
    }
}