using System;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace Dashboard.Menus.MenuItems
{
    public class DQEMenuItem:RDMPToolStripMenuItem
    {
        private readonly Catalogue _catalogue;
        readonly IExternalDatabaseServer _dqeServer;
        public DQEMenuItem(IActivateItems activator, Catalogue catalogue): base(activator, "Data Quality Engine")
        {
            _catalogue = catalogue;

            var defaults = new ServerDefaults(activator.RepositoryLocator.CatalogueRepository);
            _dqeServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.DQE);

            Image = activator.CoreIconProvider.GetImage(RDMPConcept.DQE);

            Text = _dqeServer == null ? "Create DQE Database" : "Data Quality Engine";
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (_dqeServer == null)
            {
                var cmdCreateDb = new ExecuteCommandCreateNewExternalDatabaseServer(_activator, typeof(DataQualityEngine.Database.Class1).Assembly, ServerDefaults.PermissableDefaults.DQE);
                cmdCreateDb.Execute();
            }
            else
            {
                Exception ex;
                if (!_dqeServer.Discover(DataAccessContext.InternalDataProcessing).Server.RespondsWithinTime(5, out ex))
                    ExceptionViewer.Show(ex);
                else
                    new ExecuteCommandRunDQEOnCatalogue(_activator, _catalogue).Execute();
            }

            
        }
    }
}
