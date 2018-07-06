using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution;
using CatalogueManager.Icons.IconProvision;
using HIC.Logging;

namespace CatalogueManager.Menus
{
    class ExternalDatabaseServerMenu : RDMPContextMenuStrip
    {
        public ExternalDatabaseServerMenu(RDMPContextMenuStripArgs args, ExternalDatabaseServer server) : base(args, server)
        {
            if (server.WasCreatedByDatabaseAssembly(typeof (HIC.Logging.Database.Class1).Assembly))
            {
                var viewLogs = new ToolStripMenuItem("View Logs",CatalogueIcons.Logging);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.DataLoadTasks), Keys.None,viewLogs);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.DataLoadRuns), Keys.None, viewLogs);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.FatalErrors), Keys.None, viewLogs);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.TableLoadRuns), Keys.None, viewLogs);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.DataSources), Keys.None, viewLogs);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.ProgressMessages), Keys.None, viewLogs);
                Items.Add(viewLogs);
            }
        }
    }
}
