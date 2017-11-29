using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.Collections.Providers;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableUIComponents;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class ExtractableDatasetMenu:RDMPContextMenuStrip
    {
        private readonly ExtractableDataSet _dataset;

        public ExtractableDatasetMenu(RDMPContextMenuStripArgs args, ExtractableDataSet dataset)
            : base(args,dataset)
        {
            _dataset = dataset;

            if (_dataset.DisableExtraction)
                Items.Add("ReEnable Extraction", _activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSet),
                    (s, e) => SetDisabled(false));
            else
                Items.Add("Disable Extraction (temporarily)", CatalogueIcons.ExtractableDataSetDisabled,
                    (s, e) => SetDisabled(true));

        }

        private void SetDisabled(bool disable)
        {
            _dataset.DisableExtraction = disable;
            _dataset.SaveToDatabase();
            Publish(_dataset);
        }
    }
}
