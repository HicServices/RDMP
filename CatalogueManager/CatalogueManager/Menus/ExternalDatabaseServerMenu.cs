using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections.Arbitrary;
using CatalogueManager.Icons.IconProvision;
using HIC.Logging;
using ReusableLibraryCode.DataAccess;

namespace CatalogueManager.Menus
{
    class ExternalDatabaseServerMenu : RDMPContextMenuStrip
    {
        private readonly ExternalDatabaseServer _server;

        public ExternalDatabaseServerMenu(RDMPContextMenuStripArgs args, ExternalDatabaseServer server) : base(args, server)
        {
            _server = server;
            if (server.WasCreatedByDatabaseAssembly(typeof (HIC.Logging.Database.Class1).Assembly))
            {
                var viewLogs = new ToolStripMenuItem("View Logs",CatalogueIcons.Logging);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.DataLoadTasks), Keys.None,viewLogs);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.DataLoadRuns), Keys.None, viewLogs);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.FatalErrors), Keys.None, viewLogs);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.TableLoadRuns), Keys.None, viewLogs);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.DataSources), Keys.None, viewLogs);
                Add(new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.ProgressMessages), Keys.None, viewLogs);

                viewLogs.DropDownItems.Add(new ToolStripSeparator());

                viewLogs.DropDownItems.Add(new ToolStripMenuItem("Query with SQL", CatalogueIcons.SQL, ExecuteSqlOnLoggingDatabase));
                
                Items.Add(viewLogs);
            }
        }

        private void ExecuteSqlOnLoggingDatabase(object sender, EventArgs e)
        {
            var collection = new ArbitraryTableExtractionUICollection(_server.Discover(DataAccessContext.Logging).ExpectTable("DataLoadTask"));
            _activator.Activate<ViewSQLAndResultsWithDataGridUI>(collection);
        }
    }
}
