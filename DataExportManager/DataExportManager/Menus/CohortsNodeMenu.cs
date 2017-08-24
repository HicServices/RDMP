using System;
using System.Diagnostics;
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
using DataExportManager.CohortUI;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.Collections.Providers;
using DataExportManager.ItemActivation;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class CohortsNodeMenu:RDMPContextMenuStrip
    {
        public CohortsNodeMenu(IActivateDataExportItems activator):base(activator,null)
        {
            Items.Add("Show detailed summary of all cohorts", CatalogueIcons.CohortsSourceDetailedCounts, (s, e) => ShowDetailedSummaryOfCohorts());

            Items.Add("Create New Empty Cohort Database Using Wizard", FamFamFamIcons.wand,(s, e) => LaunchCohortDatabaseCreationWizard());

            Items.Add("Create blank cohort source (Not recommended)", activator.CoreIconProvider.GetImage(RDMPConcept.ExternalCohortTable, OverlayKind.Problem), (s, e) => AddBlankExternalCohortTable());
            
        }

        public void ShowDetailedSummaryOfCohorts()
        {
            var extractableCohortCollection = new ExtractableCohortCollection();
            extractableCohortCollection.RepositoryLocator = RepositoryLocator;
            _activator.ShowWindow(extractableCohortCollection, true);

            extractableCohortCollection.SetupForAllCohorts(_activator);
        }

        private void AddBlankExternalCohortTable()
        {
            var newExternalCohortTable = new ExternalCohortTable(RepositoryLocator.DataExportRepository,"Blank Cohort Source " + Guid.NewGuid());
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(newExternalCohortTable));
            ((IActivateDataExportItems)_activator).ActivateExternalCohortTable(this, newExternalCohortTable);

        }

        private void LaunchCohortDatabaseCreationWizard()
        {
            var wizard = new CreateNewCohortDatabaseWizardUI();
            wizard.RepositoryLocator = RepositoryLocator;
            var f = _activator.ShowWindow(wizard,true);
            f.FormClosed += (s, e) =>
            {
                if (wizard.ExternalCohortTableCreatedIfAny != null)
                    _activator.RefreshBus.Publish(wizard,
                        new RefreshObjectEventArgs(wizard.ExternalCohortTableCreatedIfAny));
            };
        }
    }
}
