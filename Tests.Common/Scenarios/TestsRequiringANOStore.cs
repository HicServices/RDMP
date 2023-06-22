// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Tests.Common.Scenarios;

public class TestsRequiringANOStore:TestsRequiringA
{
    protected ExternalDatabaseServer ANOStore_ExternalDatabaseServer { get; set; }
    protected DiscoveredDatabase ANOStore_Database { get; set; }
    protected string ANOStore_DatabaseName = TestDatabaseNames.GetConsistentName("ANOStore");

    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        ANOStore_Database = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(ANOStore_DatabaseName);
            
        CreateANODatabase();

        CreateReferenceInCatalogueToANODatabase();
    }
        
    private void CreateANODatabase()
    {
        if (ANOStore_Database.Exists())
            ANOStore_Database.Drop();

        var scriptCreate = new MasterDatabaseScriptExecutor(ANOStore_Database);
            
        scriptCreate.CreateAndPatchDatabase(new ANOStorePatcher(), ThrowImmediatelyCheckNotifier.Quiet);
    }

    private void CreateReferenceInCatalogueToANODatabase()
    {
        RemovePreExistingReference();

        //now create a new reference!
        ANOStore_ExternalDatabaseServer = new ExternalDatabaseServer(CatalogueRepository, ANOStore_DatabaseName,new ANOStorePatcher());
        ANOStore_ExternalDatabaseServer.SetProperties(ANOStore_Database);

        CatalogueRepository.SetDefault(PermissableDefaults.ANOStore, ANOStore_ExternalDatabaseServer);
    }

    private void RemovePreExistingReference()
    {
        //There will likely be an old reference to the external database server
        var preExisting = CatalogueRepository.GetAllObjects<ExternalDatabaseServer>().SingleOrDefault(e => e.Name.Equals(ANOStore_DatabaseName));

        if (preExisting == null) return;

        //Some child tests will likely create ANOTables that reference this server so we need to cleanup those for them so that we can cleanup the old server reference too
        foreach (var lingeringTablesReferencingServer in CatalogueRepository.GetAllObjects<ANOTable>().Where(a => a.Server_ID == preExisting.ID))
        {
            //unhook the anonymisation transform from any ColumnInfos using it
            foreach (var colWithANOTransform in CatalogueRepository.GetAllObjects<ColumnInfo>().Where(c => c.ANOTable_ID == lingeringTablesReferencingServer.ID))
            {
                Console.WriteLine(
                    $"Unhooked ColumnInfo {colWithANOTransform} from ANOTable {lingeringTablesReferencingServer}");
                colWithANOTransform.ANOTable_ID = null;
                colWithANOTransform.SaveToDatabase();
            }
                
            TruncateANOTable(lingeringTablesReferencingServer);
            lingeringTablesReferencingServer.DeleteInDatabase();
        }

        //now delete the old server reference
        preExisting.DeleteInDatabase();
    }

    protected void TruncateANOTable(ANOTable anoTable)
    {
        Console.WriteLine($"Truncating table {anoTable.TableName} on server {ANOStore_ExternalDatabaseServer}");
            
        var server = ANOStore_Database.Server;
        using var con = server.GetConnection();
        con.Open();
        using(var cmdDelete = server.GetCommand(
                  $"if exists (select top 1 * from sys.tables where name ='{anoTable.TableName}') TRUNCATE TABLE {anoTable.TableName}", con))
            cmdDelete.ExecuteNonQuery();
        con.Close();
    }
}