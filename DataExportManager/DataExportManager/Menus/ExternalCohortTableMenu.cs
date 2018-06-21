using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.CohortUI.ImportCustomData;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;
using DataExportManager.SimpleDialogs;
using HIC.Logging;
using HIC.Logging.Listeners;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Pipelines;
using RDMPStartup;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ExternalCohortTableMenu : RDMPContextMenuStrip
    {
        private readonly ExternalCohortTable _externalCohortTable;
        private Project _project;
        private ToolStripMenuItem _importExistingCohort;

        public ExternalCohortTableMenu(RDMPContextMenuStripArgs args, ExternalCohortTable externalCohortTable): base(args, externalCohortTable)
        {
            _externalCohortTable = externalCohortTable;
            Add(new ExecuteCommandImportFileAsNewCohort(_activator));

            Add(new ExecuteCommandExecuteCohortIdentificationConfigurationAndCommitResults(_activator));

            _importExistingCohort = new ToolStripMenuItem("Import an Already Existing Cohort", _activator.CoreIconProvider.GetImage(RDMPConcept.CohortAggregate, OverlayKind.Import), (s, e) => ImportAlreadyExistingCohort());
            Items.Add(_importExistingCohort);

        }

        
        public void SetScopeIsProject(Project project)
        {
            if(_project != null)
                throw new Exception("Do not call this method more than once");

            _project = project;
            Items.Remove(_importExistingCohort);//remove any menu items that are not also appropriate in the project scope

            if (_project.ProjectNumber == null)
            {
                foreach (var item in Items.OfType<ToolStripMenuItem>())
                {
                    item.Text += " (Project has no Project Number)";
                    item.Enabled = false;
                }
            }

        }

        private void ImportAlreadyExistingCohort()
        {
            SelectWhichCohortToImport importDialog = new SelectWhichCohortToImport(_externalCohortTable);
            importDialog.RepositoryLocator = RepositoryLocator;

            if (importDialog.ShowDialog(this) == DialogResult.OK)
            {
                int toAddID = importDialog.IDToImport;
                try
                {
                    var newCohort = new ExtractableCohort(RepositoryLocator.DataExportRepository, _externalCohortTable, toAddID);
                    Publish(newCohort);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }
    }
}
