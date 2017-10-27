using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus.MenuItems
{
    internal class SetDumpServerMenuItem : RDMPToolStripMenuItem
    {
        private readonly TableInfo _tableInfo;
        private ExternalDatabaseServer[] _availableServers;

        public SetDumpServerMenuItem(IActivateItems activator, TableInfo tableInfo): base(activator,"Add Dump Server")
        {
            _tableInfo = tableInfo;

            //cannot set server if we already have one
            Enabled = tableInfo.IdentifierDumpServer_ID == null;
            Image = activator.CoreIconProvider.GetImage(RDMPConcept.ExternalDatabaseServer, OverlayKind.Add);

            var img = CatalogueIcons.ExternalDatabaseServer_IdentifierDump;
            var overlay = new IconOverlayProvider();

            var cataRepo = activator.RepositoryLocator.CatalogueRepository;

            _availableServers = cataRepo.GetAllObjects<ExternalDatabaseServer>()
                .Where(
                    s =>
                        s.CreatedByAssembly != null &&
                        s.CreatedByAssembly.Equals(typeof(IdentifierDump.Database.Class1).Assembly.GetName().Name)).ToArray();


            var miUseExisting = new ToolStripMenuItem("Use Existing...", overlay.GetOverlayNoCache(img, OverlayKind.Link),UseExisting);
            miUseExisting.Enabled = _availableServers.Any();

            DropDownItems.Add(miUseExisting);
            DropDownItems.Add("Create New...", overlay.GetOverlayNoCache(img, OverlayKind.Add), CreateNewIdentifierDumpServer);

        }

        private void UseExisting(object sender, EventArgs e)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_availableServers, false, false);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var selected = (ExternalDatabaseServer) dialog.Selected;

                if(selected != null)
                {
                    _tableInfo.IdentifierDumpServer_ID = selected.ID;
                    _tableInfo.SaveToDatabase();

                    _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_tableInfo));
                }
            }
        }

        private void CreateNewIdentifierDumpServer(object sender, EventArgs e)
        {
            var cmd = new ExecuteCommandCreateNewExternalDatabaseServer(_activator, typeof(IdentifierDump.Database.Class1).Assembly, ServerDefaults.PermissableDefaults.IdentifierDumpServer_ID);
            cmd.Execute();

            if (cmd.ServerCreatedIfAny != null)
            {
                _tableInfo.IdentifierDumpServer_ID = cmd.ServerCreatedIfAny.ID;
                _tableInfo.SaveToDatabase();

                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_tableInfo));
            }
        }
    }
}