using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    
    public class HangingConnectionTest:DatabaseTests
    {
        string testDbName = "HangingConnectionTest";


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TestConnection(bool explicitClose)
        {
            //drop it if it existed
            if (DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(testDbName).Exists())
                DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(testDbName).ForceDrop();
            
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
            
            db.ForceDrop();
        }

        void ThrowIfDatabaseLock()
        {
            var serverCopy = new DiscoveredServer(new SqlConnectionStringBuilder(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder.ConnectionString));
            serverCopy.ChangeDatabase("master");
            using (var con = serverCopy.GetConnection())
            {
                con.Open();
                var r = serverCopy.GetCommand("exec sp_who2", con).ExecuteReader();
                while (r.Read())
                    if (r["DBName"].Equals(testDbName))
                    {
                        object[] vals = new object[r.VisibleFieldCount];
                        r.GetValues(vals);
                        throw new Exception("Someone is locking " + testDbName + ":" + Environment.NewLine + string.Join(",", vals));
                        
                    }
            }

        }
    }
}
