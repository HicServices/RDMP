using System;
using System.ComponentModel.Composition;
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
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Providers;
using DataExportManager.CommandExecution.AtomicCommands;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class CohortsNodeMenu:RDMPContextMenuStrip
    {
        [ImportingConstructor]
        public CohortsNodeMenu(IActivateItems activator,AllCohortsNode node):base(activator,null)
        {

            Add(new ExecuteCommandShowDetailedSummaryOfAllCohorts(activator));
            
            Items.Add("Create New Empty Cohort Database Using Wizard", FamFamFamIcons.wand,(s, e) => LaunchCohortDatabaseCreationWizard());

            Items.Add("Create blank cohort source (Not recommended)", activator.CoreIconProvider.GetImage(RDMPConcept.ExternalCohortTable, OverlayKind.Problem), (s, e) => AddBlankExternalCohortTable());
            
        }
        
        private void AddBlankExternalCohortTable()
        {
            var newExternalCohortTable = new ExternalCohortTable(RepositoryLocator.DataExportRepository,"Blank Cohort Source " + Guid.NewGuid());
            Publish(newExternalCohortTable);
            Activate(newExternalCohortTable);
        }

        private void LaunchCohortDatabaseCreationWizard()
        {
            var wizard = new CreateNewCohortDatabaseWizardUI();
            wizard.RepositoryLocator = RepositoryLocator;
            var f = _activator.ShowWindow(wizard,true);
            f.FormClosed += (s, e) =>
            {
                if (wizard.ExternalCohortTableCreatedIfAny != null)
                    Publish(wizard.ExternalCohortTableCreatedIfAny);
            };
        }
    }
}
