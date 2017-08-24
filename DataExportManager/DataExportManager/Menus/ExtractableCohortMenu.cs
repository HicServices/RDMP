using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.CohortUI.ImportCustomData;
using DataExportManager.Collections.Providers;
using DataExportManager.DataViewing.Collections;
using DataExportManager.ItemActivation;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableUIComponents;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class ExtractableCohortMenu:RDMPContextMenuStrip
    {
        private readonly ExtractableCohort _cohort;

        public ExtractableCohortMenu(IActivateDataExportItems activator, ExtractableCohort cohort)
            : base(activator,cohort)
        {
            _cohort = cohort;
            Items.Add("View TOP 100 identifiers",null, (s, e) => ViewTop100());

            Items.Add("Import File as custom data", CatalogueIcons.ImportFile, (s, e) => ImportFileAsCustomData());

            Items.Add("Import CohortIdentificationConfiguration PatientIndexTable as custom data", activator.CoreIconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Import), (s, e) => ExecutePatientIndexTableAndImportAsCustomData());

            AddCommonMenuItems();

        }

        private void ExecutePatientIndexTableAndImportAsCustomData()
        {
            var patientIndexTables = RepositoryLocator.CatalogueRepository.GetAllObjects<JoinableCohortAggregateConfiguration>().Select(j => j.AggregateConfiguration).Distinct().ToArray();

            var chooser = new SelectIMapsDirectlyToDatabaseTableDialog(patientIndexTables, false, false);

            if (chooser.ShowDialog() == DialogResult.OK)
            {
                var chosen = chooser.Selected as AggregateConfiguration;
                if (chosen != null)
                {
                    var importer = new ImportCustomDataFileUI(_activator,_cohort, chosen);
                    importer.RepositoryLocator = RepositoryLocator;
                    _activator.ShowWindow(importer, true);
                }
            }
        }

        private void ImportFileAsCustomData()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;

            DialogResult dialogResult = ofd.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                foreach (string file in ofd.FileNames)
                {
                    var importer = new ImportCustomDataFileUI(_activator, _cohort, new FlatFileToLoad(new FileInfo(file)));
                    importer.RepositoryLocator = RepositoryLocator;
                    _activator.ShowWindow(importer, true);
                }
            }
        }

        private void ViewTop100()
        {
            _activator.ViewDataSample(new ViewCohortExtractionUICollection(_cohort));
        }

    }
}
