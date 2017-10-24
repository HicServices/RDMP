using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
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
    public class ExtractableDatasetMenu:RDMPContextMenuStrip
    {
        private readonly ExtractableDataSet _dataset;

        public ExtractableDatasetMenu(IActivateDataExportItems activator, ExtractableDataSet dataset)
            : base(activator,dataset)
        {
            _dataset = dataset;

            if (_dataset.DisableExtraction)
                Items.Add("ReEnable Extraction", activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSet),
                    (s, e) => SetDisabled(false));
            else
                Items.Add("Disable Extraction (temporarily)", CatalogueIcons.ExtractableDataSetDisabled,
                    (s, e) => SetDisabled(true));

            AddCommonMenuItems();
        }

        private void SetDisabled(bool disable)
        {
            _dataset.DisableExtraction = disable;
            _dataset.SaveToDatabase();
            Publish(_dataset);
        }
    }
}
