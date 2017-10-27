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
    public class ProjectsNodeMenu:RDMPContextMenuStrip
    {
        public ProjectsNodeMenu(IActivateDataExportItems activator)
            : base(activator,null)
        {

            Items.Add("Add New Project", activator.CoreIconProvider.GetImage(RDMPConcept.Project, OverlayKind.Add), (s, e) => AddProject());
            
        }

        private void AddProject()
        {
            Project p = new Project(RepositoryLocator.DataExportRepository,"New Project " + Guid.NewGuid());
            Publish(p);
            ((IActivateDataExportItems)_activator).ActivateProject(this, p);
        }
    }
}
