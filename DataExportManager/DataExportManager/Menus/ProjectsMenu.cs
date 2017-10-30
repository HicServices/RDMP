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
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.Collections.Providers;
using DataExportManager.CommandExecution.AtomicCommands;
using HIC.Common.Validation.Constraints.Primary;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class ProjectsMenu:RDMPContextMenuStrip
    {
        private readonly Project _project;

        public ProjectsMenu(IActivateItems activator,  Project project)
            : base(activator,project)
        {
            _project = project;
            var childProvider = (DataExportChildProvider) activator.CoreChildProvider;


            Items.Add("Add New Extraction Configuration", GetImage(RDMPConcept.ExtractionConfiguration, OverlayKind.Add), (s, e) => AddExtractionConfiguration());
            Items.Add("View Checks", CatalogueIcons.Warning, (s, e) => PopupChecks());
            Items.Add("Set Extraction Folder", CatalogueIcons.ExtractionFolderNode, (s, e) => ChooseExtractionFolder());

            Add(new ExecuteCommandReleaseProject(_activator).SetTarget(project));
            
            var importCohort = new ToolStripMenuItem("Import Cohort", GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Import));

            if (!childProvider.CohortSources.Any())
            {
                importCohort.Text = "Import Cohort (You must create at least one 'Cohort Source' first)";
                importCohort.Enabled = false;
            }
            else if (childProvider.CohortSources.Length == 1)
            {
                //there is only one so we can add the import items directly to the menu
                AddCohortImportOptionsAsDropDownItemsOf(importCohort, childProvider.CohortSources[0]);
            }
            else //there are many user must pick one source to import into
            foreach (var source in childProvider.CohortSources)
            {
                var currentSource = new ToolStripMenuItem(source.Name);

                AddCohortImportOptionsAsDropDownItemsOf(currentSource, source);

                importCohort.DropDownItems.Add(currentSource);
            }

            this.Items.Add(importCohort);
            
            AddCommonMenuItems();
        }

        private void AddCohortImportOptionsAsDropDownItemsOf(ToolStripMenuItem toolStripMenuItem, ExternalCohortTable source)
        {
            //lets hijack the ExternalCohortTableMenu, after all anything relevant to them is probably also relevant here right
            var proxyMenu = new ExternalCohortTableMenu((IActivateItems)_activator, source);
            proxyMenu.SetScopeIsProject(_project);

            //get all the items from the menu we just created
            var proxyItems = proxyMenu.Items.Cast<ToolStripItem>().ToArray();
            proxyMenu.Items.Clear();

            //and move them to our dropdown
            foreach (var proxyItem in proxyItems)
                toolStripMenuItem.DropDownItems.Add(proxyItem);
        }

        private void ChooseExtractionFolder()
        {
            using (var ofd = new FolderBrowserDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _project.ExtractionDirectory = ofd.SelectedPath;
                    _project.SaveToDatabase();
                    Publish(_project);
                }
            }
        }

        private void PopupChecks()
        {
            var checker = new ProjectChecker(RepositoryLocator,_project);
            var p = new PopupChecksUI("Checking Project "+ _project,false);
            p.StartChecking(checker);
        }

        private void AddExtractionConfiguration()
        {
            var newConfig = new ExtractionConfiguration(RepositoryLocator.DataExportRepository, _project);
            
            //refresh the project
            Publish(_project);
            Activate(newConfig);
        }
    }
}
