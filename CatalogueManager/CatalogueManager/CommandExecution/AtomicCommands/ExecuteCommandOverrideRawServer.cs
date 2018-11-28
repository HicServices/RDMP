using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandOverrideRawServer:BasicUICommandExecution,IAtomicCommand,IAtomicCommandWithTarget
    {
        private readonly LoadMetadata _loadMetadata;
        private ExternalDatabaseServer _server;
        private ExternalDatabaseServer[] _available;

        public ExecuteCommandOverrideRawServer(IActivateItems activator,LoadMetadata loadMetadata) : base(activator)
        {
            _loadMetadata = loadMetadata;
            _available =
                activator.CoreChildProvider.AllExternalServers.Where(s => string.IsNullOrWhiteSpace(s.CreatedByAssembly)).ToArray();

            if(!_available.Any())
                SetImpossible("There are no compatible servers");
        }

        public override void Execute()
        {
            base.Execute();

            if (_server == null)
            {
                var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_available,true, false);

                if (dialog.ShowDialog() == DialogResult.OK)
                    _server = dialog.Selected as ExternalDatabaseServer;
                else
                    return;
            }

            _loadMetadata.OverrideRAWServer_ID = _server == null ? null : (int?)_server.ID;
            _loadMetadata.SaveToDatabase();

            Publish(_loadMetadata);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExternalDatabaseServer, OverlayKind.Link);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            var candidate = target as ExternalDatabaseServer;

            if (candidate != null && _available.Contains(candidate))
                _server = candidate;

            return this;
        }
    }
}