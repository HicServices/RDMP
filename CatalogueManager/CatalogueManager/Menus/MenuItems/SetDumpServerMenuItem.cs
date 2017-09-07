using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus.MenuItems
{
    internal class SetDumpServerMenuItem : ToolStripMenuItem
    {
        private readonly IActivateItems _activator;
        private readonly TableInfo _tableInfo;

        public SetDumpServerMenuItem(IActivateItems activator, TableInfo tableInfo): base("Set Dump Server")
        {
            _activator = activator;
            _tableInfo = tableInfo;

            var img = CatalogueIcons.ExternalDatabaseServer_IdentifierDump;
            var overlay = new IconOverlayProvider();

            var cataRepo = activator.RepositoryLocator.CatalogueRepository;

            var iddServers = cataRepo.GetAllObjects<ExternalDatabaseServer>()
                .Where(
                    s =>
                        s.CreatedByAssembly != null &&
                        s.CreatedByAssembly.Equals(typeof(IdentifierDump.Database.Class1).Assembly.GetName().Name)).ToArray();

            //cannot change server if 
            Enabled = tableInfo.IdentifierDumpServer_ID == null;

            foreach (ExternalDatabaseServer v in iddServers)
            {
                var server = v;
                DropDownItems.Add(
                    new ToolStripMenuItem(server.Name,
                        overlay.GetOverlayNoCache(img,OverlayKind.Link),
                        (s, e) => SetDiscardedColumnTo(server)));
            }

            DropDownItems.Add(new ToolStripSeparator());
            DropDownItems.Add("Create New...", overlay.GetOverlayNoCache(img, OverlayKind.Add), (s, e) => CreateNewIdentifierDumpServer(tableInfo));

            DropDownItems.Add(new ToolStripSeparator());


            var clearDumpMenuItem = new ToolStripMenuItem("Clear Dump Server", null, ClearDumpServer);
            clearDumpMenuItem.Enabled = _tableInfo.IdentifierDumpServer_ID != null;
            DropDownItems.Add(clearDumpMenuItem);

        }
        private void CreateNewIdentifierDumpServer(TableInfo tableInfo)
        {
            var cmd = new ExecuteCommandCreateNewExternalDatabaseServer(_activator, typeof(IdentifierDump.Database.Class1).Assembly, ServerDefaults.PermissableDefaults.IdentifierDumpServer_ID);
            cmd.Execute();

            if (cmd.ServerCreatedIfAny != null)
            {
                tableInfo.IdentifierDumpServer_ID = cmd.ServerCreatedIfAny.ID;
                tableInfo.SaveToDatabase();

                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(tableInfo));
            }
        }

        private void SetDiscardedColumnTo(ExternalDatabaseServer server)
        {
            _tableInfo.IdentifierDumpServer_ID = server.ID;

            _tableInfo.SaveToDatabase();
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_tableInfo));
        }

        private void ClearDumpServer(object sender, EventArgs e)
        {
            _tableInfo.IdentifierDumpServer_ID = null;

            _tableInfo.SaveToDatabase();
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_tableInfo));
        }
    }
}