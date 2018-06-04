using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Providers.Nodes;
using DataExportManager.CohortUI;
using DataExportManager.CommandExecution.AtomicCommands;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class AllCohortsNodeMenu:RDMPContextMenuStrip
    {
        [ImportingConstructor]
        public AllCohortsNodeMenu(RDMPContextMenuStripArgs args, AllCohortsNode node)
            : base(args, node)
        {
            Add(new ExecuteCommandShowDetailedSummaryOfAllCohorts(_activator));

            Add(new ExecuteCommandCreateNewCohortDatabaseUsingWizard(_activator));

            Items.Add("Create blank cohort source (Not recommended)", _activator.CoreIconProvider.GetImage(RDMPConcept.ExternalCohortTable, OverlayKind.Problem), (s, e) => AddBlankExternalCohortTable());
            
        }
        
        private void AddBlankExternalCohortTable()
        {
            var newExternalCohortTable = new ExternalCohortTable(RepositoryLocator.DataExportRepository,"Blank Cohort Source " + Guid.NewGuid());
            Publish(newExternalCohortTable);
            Activate(newExternalCohortTable);
        }
    }
}
