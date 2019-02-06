// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.ModelBinding;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using FAnsi.Discovery;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Reports
{
    /// <summary>
    /// Calculates the size of all the databases on an server, the rowcounts of all talbes as well as summarising key value pair descriptors of the server (version etc).
    /// </summary>
    public class DatabaseSizeReport : RequiresMicrosoftOffice, ICheckable
    {
        private readonly TableInfo[] _tableInfos;
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
            
            StringBuilder sb = new StringBuilder();
            
            foreach (KeyValuePair<DiscoveredServer, List<DatabaseSizeFacts>> kvp in servers)
            {
                try
                {
                    //anounce the server
                    AddServerDetails(sb, kvp.Key);
                    AddDatabaseDetails(sb, kvp.Key,kvp.Value,notifier);
                    notifier.OnCheckPerformed(new CheckEventArgs("Finished processing server " + kvp.Key, CheckResult.Success));
                }
                catch (Exception ex)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not process server " + kvp.Key, CheckResult.Fail,ex));
                }
            }
            
            var f = GetUniqueFilenameInWorkArea("DatabaseSizeReport", ".csv");
            File.WriteAllText(f.FullName,sb.ToString());
            ShowFile(f);
        }

        private void AddDatabaseDetails(StringBuilder sb,DiscoveredServer server, List<DatabaseSizeFacts> databases,ICheckNotifier notifier)
        {
            
            foreach (var database in databases)
            {
                try
                {
                    var discoveredDatabase = server.ExpectDatabase(database.DatabaseName);
                    //Get a dictionary that describes the database
                    var dbDictionary = discoveredDatabase.DescribeDatabase();

                    sb.Append("Database").Append(",");
                    sb.AppendLine(string.Join(",",dbDictionary.Keys));
                    
                    
                    sb.Append( database.DatabaseName).Append(",");
                    sb.AppendLine(string.Join(",",dbDictionary.Values));

                    sb.AppendLine("TableName,Row count (Fast),Dependant Catalogues");

                    foreach (var kvp in database.Tables)
                    {
                        string tableName = kvp.Key.GetRuntimeName();

                        sb.Append(tableName).Append(",");

                        try
                        {
                            var rowcount = discoveredDatabase.ExpectTable(tableName).GetRowCount();
                            sb.Append(rowcount).Append(",");
                            notifier.OnCheckPerformed(
                                new CheckEventArgs("Processed table " + tableName + " with row count " + rowcount,
                                    CheckResult.Success));

                            sb.AppendLine("\"" + string.Join(",", kvp.Value.Select(c=>c.Name)) + "\"");
                        }
                        catch (Exception ex)
                        {
                            notifier.OnCheckPerformed(new CheckEventArgs("Could not get row count for table " + tableName,CheckResult.Warning,ex));
                            sb.AppendLine("Unknown");
                        }
                    }
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Could not process database " + database.DatabaseName,CheckResult.Fail, e));
                }

                sb.AppendLine();
            }
        }

        private void AddServerDetails(StringBuilder sb, DiscoveredServer discoveredServer)
        {
            Dictionary<string, string> description = discoveredServer.DescribeServer();

            sb.AppendLine("Server " + discoveredServer.Name);

            foreach (KeyValuePair<string, string> kvp in description)
                sb.Append(kvp.Key).Append(",").AppendLine(kvp.Value);
        }


        private class DatabaseSizeFacts
        {
            public string DatabaseName { get; set; }
            public Dictionary<TableInfo,Catalogue[]> Tables { get; private set; }

            public DatabaseSizeFacts(CatalogueRepository repository,TableInfo t)
            {
                DatabaseName = t.GetDatabaseRuntimeName();
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
