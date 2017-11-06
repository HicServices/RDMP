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
            
            Items.Add("View Checks", CatalogueIcons.Warning, (s, e) => PopupChecks());
            Items.Add("Set Extraction Folder", CatalogueIcons.ExtractionFolderNode, (s, e) => ChooseExtractionFolder());

            Add(new ExecuteCommandReleaseProject(_activator).SetTarget(project));
            
            AddCommonMenuItems();
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

    }
}
