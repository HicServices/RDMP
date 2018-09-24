using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using CatalogueManager.Menus.MenuItems;
using Dashboard.CommandExecution.AtomicCommands;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace Dashboard.Menus.MenuItems
{
    public class DQEMenuItem:RDMPToolStripMenuItem
    {
        public DQEMenuItem(IActivateItems activator, Catalogue catalogue): base(activator, "Data Quality Engine")
        {
            var defaults = new ServerDefaults(activator.RepositoryLocator.CatalogueRepository);
            var dqeServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.DQE);
            var iconProvider = activator.CoreIconProvider;

            Image = activator.CoreIconProvider.GetImage(RDMPConcept.DQE);

            if (dqeServer == null)
            {
                var cmdCreateDb = new ExecuteCommandCreateNewExternalDatabaseServer(_activator, typeof(DataQualityEngine.Database.Class1).Assembly, ServerDefaults.PermissableDefaults.DQE);
                cmdCreateDb.OverrideCommandName = "Create DQE Database";
                Add(cmdCreateDb);

                DropDownItems.Add("Create link to existing DQE Database", iconProvider.GetImage(RDMPConcept.Database, OverlayKind.Link), (s, e) => LaunchManageExternalServers());
                DropDownItems.Add(new ToolStripSeparator());
            }
            else
            {
                Exception ex;

                if (!dqeServer.Discover(DataAccessContext.InternalDataProcessing).Server.RespondsWithinTime(5, out ex))
                {
                    DropDownItems.Add(new ToolStripMenuItem("Data Quality Engine Server Broken!", _activator.CoreIconProvider.GetImage(RDMPConcept.DQE, OverlayKind.Problem), (s, e) => ExceptionViewer.Show(ex)));
                    return;
                }
            }

            Add(new ExecuteCommandConfigureCatalogueValidationRules(_activator).SetTarget(catalogue));

            DropDownItems.Add(new ToolStripSeparator());

            Add(new ExecuteCommandRunDQEOnCatalogue(_activator).SetTarget(catalogue));

            DropDownItems.Add(new ToolStripSeparator());

            Add(new ExecuteCommandViewDQEResultsForCatalogue(_activator).SetTarget(catalogue));
        }
        
        
        private void LaunchManageExternalServers()
        {
            new ExecuteCommandConfigureDefaultServers(_activator).Execute();
        }
    }
}
