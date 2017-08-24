using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportManager.CohortUI;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.Collections.Providers;
using DataExportManager.ItemActivation;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableUIComponents;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class ExtractableDataSetsNodeMenu:RDMPContextMenuStrip
    {
        public ExtractableDataSetsNodeMenu(IActivateDataExportItems activator)
            : base(activator,null)
        {


            Items.Add("Make Catalogue(s) Extractable", activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSet, OverlayKind.Add), (s, e) => MakeCataloguesExtractable());
            Items.Add("Add Extractable Data Set Package", activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSetPackage, OverlayKind.Add), (s, e) => CreateExtractableDatasetPackage());
        }

        private void CreateExtractableDatasetPackage()
        {
            TypeTextOrCancelDialog dialog = new TypeTextOrCancelDialog("Name for new Package","Type a name for the new package",1000);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var newPackage = new ExtractableDataSetPackage(RepositoryLocator.DataExportRepository, dialog.ResultText);
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(newPackage));
            }
        }

        private void MakeCataloguesExtractable()
        {
            var existing = RepositoryLocator.DataExportRepository.GetAllObjects<ExtractableDataSet>().Select(e=>e.Catalogue_ID);

            var available = RepositoryLocator.CatalogueRepository.GetAllCataloguesWithAtLeastOneExtractableItem().Where(c => !existing.Contains(c.ID));

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(available, false, false);
            dialog.AllowMultiSelect = true;
            
            if(dialog.ShowDialog() == DialogResult.OK)
                foreach (Catalogue c in dialog.MultiSelected)
                {
                    var ds = new ExtractableDataSet(RepositoryLocator.DataExportRepository,c);
                    _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(ds));
                }
            
        }

    }
}
