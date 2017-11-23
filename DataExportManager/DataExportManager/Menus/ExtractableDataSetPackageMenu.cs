using System;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportManager.Collections.Providers;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class ExtractableDataSetPackageMenu : RDMPContextMenuStrip
    {
        private readonly ExtractableDataSetPackage _package;
        private readonly DataExportChildProvider _childProvider;

        public ExtractableDataSetPackageMenu(IActivateItems activator, ExtractableDataSetPackage package, RDMPCollectionCommonFunctionality collection):base(activator,package, collection)
        {
            _package = package;
            _childProvider = (DataExportChildProvider) activator.CoreChildProvider;
            Items.Add("Add ExtractableDataSet(s) to Package", activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSet, OverlayKind.Link), AddExtractableDatasetToPackage);

            AddCommonMenuItems();
        }

        private void AddExtractableDatasetToPackage(object sender, EventArgs e)
        {
            var contents = _childProvider.PackageContents;
            var notInPackage = _childProvider.ExtractableDataSets.Except(contents.GetAllDataSets(_package,_childProvider.ExtractableDataSets));


            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(notInPackage, false, false);
            dialog.AllowMultiSelect = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (ExtractableDataSet ds in dialog.MultiSelected.Cast<ExtractableDataSet>())
                    contents.AddDataSetToPackage(_package, ds);

                //package contents changed
                if(dialog.MultiSelected.Any())
                    Publish(_package);
            }
        }
    }
}