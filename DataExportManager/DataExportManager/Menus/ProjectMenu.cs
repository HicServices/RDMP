using System;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI;
using DataExportManager.CohortUI.CohortSourceManagement;
using DataExportManager.CommandExecution.AtomicCommands;
using HIC.Common.Validation.Constraints.Primary;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableUIComponents;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ProjectMenu:RDMPContextMenuStrip
    {
        private readonly Project _project;

        public ProjectMenu(RDMPContextMenuStripArgs args, Project project)
            : base(args,project)
        {
            _project = project;

            Add(new ExecuteCommandRunChecksInPopupWindow(_activator, new ProjectChecker(RepositoryLocator, _project)));
            Add(new ExecuteCommandRelease(_activator).SetTarget(project));
            Add(new ExecuteCommandCreateNewCatalogueByImportingFile(_activator).SetTarget(_project));
        }

    }
}
