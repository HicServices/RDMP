using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Copying.Commands;
using RDMPStartup;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class CatalogueItemMenu : RDMPContextMenuStrip
    {
        private readonly CatalogueItem _catalogueItem;

        public CatalogueItemMenu(RDMPContextMenuStripArgs args, CatalogueItem catalogueItem): base(args, catalogueItem)
        {
            _catalogueItem = catalogueItem;

            Items.Add("Add Issue", _activator.CoreIconProvider.GetImage(RDMPConcept.CatalogueItemIssue,OverlayKind.Add),(s,e)=> AddIssue());

            Add(new ExecuteCommandLinkCatalogueItemToColumnInfo(_activator, catalogueItem));

            //it does not yet have extractability
            Add(new ExecuteCommandMakeCatalogueItemExtractable(_activator, catalogueItem));

            var importDescription = new ToolStripMenuItem("Import Description From Another CatalogueItem");
            
            importDescription.DropDownItems.Add("Any (Ctrl + Shift + I)",null,ImportAnyDescription);
            importDescription.DropDownItems.Add("With same name (Ctrl + I)",null, ImportWithSameName);

            Items.Add(importDescription);
        }

        private void AddIssue()
        {
            var newIssue = new CatalogueItemIssue(RepositoryLocator.CatalogueRepository, _catalogueItem);
            _activator.ActivateCatalogueItemIssue(this,newIssue);
            Publish(_catalogueItem);
        }


        private void ImportWithSameName(object sender, EventArgs e)
        {
            var dialog = new ImportCloneOfCatalogueItem(_catalogueItem.Catalogue, _catalogueItem, true);
            dialog.ShowDialog();
            _catalogueItem.SaveToDatabase();

            Publish(_catalogueItem);
        }

        private void ImportAnyDescription(object sender, EventArgs e)
        {
            var dialog = new ImportCloneOfCatalogueItem(_catalogueItem.Catalogue, _catalogueItem);
            dialog.ShowDialog();
            _catalogueItem.SaveToDatabase();

            Publish(_catalogueItem);
        }

        public void HandleKeyPress(KeyEventArgs k)
        {
            if(k.KeyCode == Keys.I && k.Shift && k.Control)
            {
                ImportAnyDescription(null,null);
                return;
            }
            
            if(k.KeyCode == Keys.I && k.Control)
            {
                ImportWithSameName(null,null);
                return;
            }
        }
    }
}
