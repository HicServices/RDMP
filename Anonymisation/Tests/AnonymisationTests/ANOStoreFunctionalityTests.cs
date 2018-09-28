using System;
using System.Data.Common;
using System.Data.SqlClient;
using CatalogueLibrary.Data.DataLoad;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace AnonymisationTests
{
    public class ANOStoreFunctionalityTests:TestsRequiringFullAnonymisationSuite
    {
        [Test]
        public void CanAccessANODatabase_Directly()
        {
            var server = ANOStore_Database.Server;
            using (var con = server.GetConnection())
            {
                con.Open();

                var cmd = server.GetCommand("Select version from RoundhousE.Version", con);
                var version = new Version(cmd.ExecuteScalar().ToString());

                Assert.GreaterOrEqual(version, new Version("0.0.0.0"));

                con.Close();
            }
        }

        [Test]
        public void CanAccessANODatabase_ViaExternalServerPointer()
        {
            using (var connection = DataAccessPortal.GetInstance().ExpectServer(ANOStore_ExternalDatabaseServer, DataAccessContext.DataLoad).GetConnection())
            {
                connection.Open();

                DbCommand cmd = DatabaseCommandHelper.GetCommand("Select version from RoundhousE.Version", connection);
                var version = new Version(cmd.ExecuteScalar().ToString());

                Assert.GreaterOrEqual(version, new Version("0.0.0.0"));

                connection.Close();
            }

            
        }

        [Test]
        public void CanAccessIdentifierDumpDatabase_Directly()
        {
            using (var con = IdentifierDump_Database.Server.GetConnection())
            {
                con.Open();

                var cmd = IdentifierDump_Database.Server.GetCommand("Select version from RoundhousE.Version", con);
                var version = new Version(cmd.ExecuteScalar().ToString());

                Assert.GreaterOrEqual(version, new Version("0.0.0.0"));

                con.Close();
            }
        }

        [Test]
        public void CanAccessIdentifierDumpDatabase_ViaExternalServerPointer()
        {
            using(var connection = DataAccessPortal.GetInstance().ExpectServer(IdentifierDump_ExternalDatabaseServer, DataAccessContext.DataLoad).GetConnection())
            {
                connection.Open();

                DbCommand cmd = DatabaseCommandHelper.GetCommand("Select version from RoundhousE.Version", connection);
                var version = new Version(cmd.ExecuteScalar().ToString());

                Assert.GreaterOrEqual(version, new Version("0.0.0.0"));

                connection.Close();
            }
        }
    }
}
