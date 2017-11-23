using System;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
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

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class ProjectMenu:RDMPContextMenuStrip
    {
        private readonly Project _project;

        public ProjectMenu(IActivateItems activator, Project project, RDMPCollectionCommonFunctionality collection)
            : base(activator,project, collection)
        {
            _project = project;
            
            Items.Add("View Checks", CatalogueIcons.Warning, (s, e) => PopupChecks());
            Add(new ExecuteCommandReleaseProject(_activator).SetTarget(project));
            
            AddCommonMenuItems();
        }


        private void PopupChecks()
        {
            var checker = new ProjectChecker(RepositoryLocator,_project);
            var p = new PopupChecksUI("Checking Project "+ _project,false);
            p.StartChecking(checker);
        }

    }
}
