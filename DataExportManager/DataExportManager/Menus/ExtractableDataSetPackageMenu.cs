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
using DataExportLibrary.Providers;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ExtractableDataSetPackageMenu : RDMPContextMenuStrip
    {
        private readonly ExtractableDataSetPackage _package;
        private readonly DataExportChildProvider _childProvider;

        public ExtractableDataSetPackageMenu(RDMPContextMenuStripArgs args, ExtractableDataSetPackage package): base(args, package)
        {
            _package = package;
            _childProvider = (DataExportChildProvider) _activator.CoreChildProvider;
            Items.Add("Add ExtractableDataSet(s) to Package", _activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSet, OverlayKind.Link), AddExtractableDatasetToPackage);

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