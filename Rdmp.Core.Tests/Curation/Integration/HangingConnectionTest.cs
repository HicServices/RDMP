// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Data.SqlClient;
using System.Threading;
using FAnsi.Discovery;
using NUnit.Framework;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class HangingConnectionTest:DatabaseTests
{
    private string testDbName = "HangingConnectionTest";


    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TestConnection(bool explicitClose)
    {
        //drop it if it existed
        if (DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(testDbName).Exists())
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(testDbName).Drop();
            
        DiscoveredServerICanCreateRandomDatabasesAndTablesOn.CreateDatabase(testDbName);
        Thread.Sleep(500);

        ThrowIfDatabaseLock();
            
        var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(testDbName);

        ThrowIfDatabaseLock();

        using (var con = db.Server.GetConnection())
        {
            con.Open();

            //we are currently connected so this should throw
            Assert.Throws<Exception>(ThrowIfDatabaseLock);

        }
        Thread.Sleep(500);

        if (explicitClose)
        {
            SqlConnection.ClearAllPools();
            Thread.Sleep(500);
            Assert.DoesNotThrow(ThrowIfDatabaseLock);//in this case we told .net to clear the pools which leaves the server free of locks/hanging connections
        }
        else
        {
            Assert.Throws<Exception>(ThrowIfDatabaseLock);//despite us closing the connection and using the 'using' block .net still keeps a connection in sleep state to the server ><
        }
            
        db.Drop();
    }

    private void ThrowIfDatabaseLock()
    {
        var serverCopy = new DiscoveredServer(new SqlConnectionStringBuilder(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder.ConnectionString));
        serverCopy.ChangeDatabase("master");
        using var con = serverCopy.GetConnection();
        con.Open();
        var r = serverCopy.GetCommand("exec sp_who2", con).ExecuteReader();
        while (r.Read())
            if (r["DBName"].Equals(testDbName))
            {
                var vals = new object[r.VisibleFieldCount];
                r.GetValues(vals);
                throw new Exception(
                    $"Someone is locking {testDbName}:{Environment.NewLine}{string.Join(",", vals)}");

            }
    }
}