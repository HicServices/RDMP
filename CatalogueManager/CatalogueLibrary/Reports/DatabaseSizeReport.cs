using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.ModelBinding;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace CatalogueLibrary.Reports
{
    public class DatabaseSizeReport : RequiresMicrosoftOffice,ICheckable
    {
        private readonly TableInfo[] _tableInfos;


        private Microsoft.Office.Interop.Excel.Application xlApp;
        private object _missing = false;
        private CatalogueRepository _repository;

        public DatabaseSizeReport(TableInfo[] tableInfos, CatalogueRepository repository)
        {
            _tableInfos = tableInfos;
            _repository = repository;
        }

        public void Check(ICheckNotifier notifier)
        {
            var servers = new Dictionary<DiscoveredServer, List<DatabaseSizeFacts>>();

            foreach (TableInfo t in _tableInfos)
            {
                var key = servers.Keys.SingleOrDefault(k => k.Name.Equals(t.Server));
                if(key == null)
                {
                    var server = DataAccessPortal.GetInstance().ExpectServer(t, DataAccessContext.InternalDataProcessing);
                    servers.Add(server, new List<DatabaseSizeFacts>());
                    key = server;
                }

                //see if we already know about this database
                var toAddTo = servers[key].SingleOrDefault(db => db.DatabaseName.Equals(t.Database));

                //it is not novel
                if(toAddTo != null)
                    toAddTo.AddTableInfo(t); //add it as another table we know about in that database
                else
                    servers[key].Add(new DatabaseSizeFacts(_repository,t));//it is novel so add it as a new database we know only that table t is in

                notifier.OnCheckPerformed(new CheckEventArgs("Identified dependencies of " + t, CheckResult.Success));
            }
            
            xlApp = new Application();

            xlApp.Visible = false;
            xlApp.UserControl = false;

            Workbook wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet ws = (Worksheet)wb.Worksheets[1];


            if (ws == null)
            {
                Console.WriteLine("Worksheet could not be created. Check that your office installation and project references are correct.");
            }

            //excel indexes cells from 1
            int currentRow = 1;

            foreach (KeyValuePair<DiscoveredServer, List<DatabaseSizeFacts>> kvp in servers)
            {
                try
                {
                    //anounce the server
                    AddServerDetails(ws, ref currentRow, kvp.Key);
                    currentRow++;
                  
                    AddDatabaseDetails(ws, ref currentRow, kvp.Key,kvp.Value,notifier);
                    notifier.OnCheckPerformed(new CheckEventArgs("Finished processing server " + kvp.Key, CheckResult.Success));
                }
                catch (Exception ex)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not process server " + kvp.Key, CheckResult.Fail,ex));
                    currentRow ++;
                }
            }
            
            xlApp.Visible = true;
        }

        private void AddDatabaseDetails(Worksheet ws, ref int currentRow, DiscoveredServer server, List<DatabaseSizeFacts> databases,ICheckNotifier notifier)
        {
            
            foreach (var database in databases)
            {
                try
                {
                    var discoveredDatabase = server.ExpectDatabase(database.DatabaseName);
                    //Get a dictionary that describes the database
                    var dbDictionary = discoveredDatabase.DescribeDatabase();

                    //output 2 lines:
                    //line 1 is headers for the database being described
                    //line 2 is the database description
                    ws.Cells[currentRow, 1] = "Database";
                    ws.Cells[currentRow + 1, 1] = database.DatabaseName;
                
                    int currentColumn = 2;

                    foreach (var kvp in dbDictionary)
                    {
                        ws.Cells[currentRow, currentColumn] = kvp.Key;
                        ws.Cells[currentRow+1, currentColumn] = kvp.Value;
                        currentColumn++;
                    }
                    currentRow += 2;

                    ws.Cells[currentRow, 1] = "TableName";
                    ws.Cells[currentRow, 2] = "Row count (Fast)";
                    ws.Cells[currentRow, 3] = "Dependant Catalogues";

                    currentRow++;

                    foreach (var kvp in database.Tables)
                    {
                        string tableName = kvp.Key.GetRuntimeName();

                        ws.Cells[currentRow, 1] = tableName;
                        try
                        {
                            var rowcount = discoveredDatabase.ExpectTable(tableName).GetRowCount();
                            ws.Cells[currentRow, 2] = rowcount;
                            notifier.OnCheckPerformed(
                                new CheckEventArgs("Processed table " + tableName + " with row count " + rowcount,
                                    CheckResult.Success));

                            ws.Cells[currentRow, 3] = string.Join(",", kvp.Value.Select(c=>c.Name));
                        }
                        catch (Exception ex)
                        {
                            notifier.OnCheckPerformed(new CheckEventArgs("Could not get row count for table " + tableName,CheckResult.Warning,ex));
                            ws.Cells[currentRow, 2] = "Unknown";
                        }
                        currentRow++;    
                    
                    }
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not process database " + database.DatabaseName,CheckResult.Fail, e));
                }
                currentRow++;
            }
        }

        private void AddServerDetails(Worksheet ws, ref int currentRow, DiscoveredServer discoveredServer)
        {
            Dictionary<string, string> description = discoveredServer.DescribeServer();

            ws.Cells[currentRow, 1] = "Server " + discoveredServer.Name;
            currentRow ++;

            foreach (KeyValuePair<string, string> kvp in description)
            {
                ws.Cells[currentRow, 1] = kvp.Key;
                ws.Cells[currentRow, 2] = kvp.Value;
                currentRow++;
            }
        }


        private class DatabaseSizeFacts
        {
            public string DatabaseName { get; set; }
            public Dictionary<TableInfo,Catalogue[]> Tables { get; private set; }

            public DatabaseSizeFacts(CatalogueRepository repository,TableInfo t)
            {
                DatabaseName = t.Database;
                Tables = new Dictionary<TableInfo, Catalogue[]>();

                AddTableInfo(t);
            }

            public void AddTableInfo(TableInfo tableInfo)
            {
                Tables.Add(tableInfo,tableInfo.GetAllRelatedCatalogues());
            }
        }
    }
}
