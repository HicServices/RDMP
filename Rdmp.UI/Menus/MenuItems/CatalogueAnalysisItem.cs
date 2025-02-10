using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.MainFormUITabs;
using Rdmp.UI.SimpleDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.Menus.MenuItems
{
    public  class CatalogueAnalysisItem: RDMPToolStripMenuItem
    {

        private readonly Catalogue _catalogue;
        private readonly IExternalDatabaseServer _dqeServer;
        public CatalogueAnalysisItem(IActivateItems activator, Catalogue catalogue): base(activator, "Catalogue Analysis")
        {
            _catalogue = catalogue;

            _dqeServer = activator.RepositoryLocator.CatalogueRepository.GetDefaultFor(PermissableDefaults.DQE);

            Image = activator.CoreIconProvider.GetImage(RDMPConcept.DQE).ImageToBitmap();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (_dqeServer == null)
            {
                var cmdCreateDb = new ExecuteCommandCreateNewExternalDatabaseServer(_activator,
                    new DataQualityEnginePatcher(), PermissableDefaults.DQE);
                cmdCreateDb.Execute();
            }
            else
            {
                if (!_dqeServer.Discover(DataAccessContext.InternalDataProcessing).Server.RespondsWithinTime(5, out var ex))
                    ExceptionViewer.Show(ex);
                else
                    new ExecuteCommanndAnalyseCatalogue(_activator, _catalogue).Execute();
            }
        }
    }
}
