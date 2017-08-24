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
            DbConnection connection = DataAccessPortal.GetInstance().ExpectServer(ANOStore_ExternalDatabaseServer, DataAccessContext.DataLoad).GetConnection();

            connection.Open();

            DbCommand cmd = DatabaseCommandHelper.GetCommand("Select version from RoundhousE.Version", connection);
            var version = new Version(cmd.ExecuteScalar().ToString());

            Assert.GreaterOrEqual(version, new Version("0.0.0.0"));

            connection.Close();
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
            DbConnection connection = DataAccessPortal.GetInstance().ExpectServer(IdentifierDump_ExternalDatabaseServer, DataAccessContext.DataLoad).GetConnection();

            connection.Open();

            DbCommand cmd = DatabaseCommandHelper.GetCommand("Select version from RoundhousE.Version", connection);
            var version = new Version(cmd.ExecuteScalar().ToString());

            Assert.GreaterOrEqual(version, new Version("0.0.0.0"));

            connection.Close();
        }

          /*
        [Test]
        [ExpectedException(ExpectedMessage = "This suffix has already been used, please change suffix and try again")]
        public void CreateSameSuffixTwice()
        {
//            ANOOperation.DemandCreationOfNewConfiguration(ANOStore_ExternalDatabaseServer, "ANO_CreateSameSuffixTwice", "varchar(10)", "AAAAAAAA", "T", "Latin1_General_CI_AS");
  //          ANOOperation.DemandCreationOfNewConfiguration(ANOStore_ExternalDatabaseServer, "ANO_CreateSameSuffixTwice", "varchar(10)", "AAAAAAAA", "T", "Latin1_General_CI_AS");
        }
        
      
        [Test]
        [TestCase("varchar(1)")]
        [TestCase("binary")]
        [TestCase("text")]
        [TestCase("varbinary")]
        [TestCase("varbinary", "AAAAAAAA", "F", "Exciting Collation")]
        public void CreateANOConfiguration_InvalidTable(string datatype)
        {
            ANOOperation.DemandCreationOfNewConfiguration(ANOStore_ExternalDatabaseServer, "ANOFreakazilla", datatype, "AAAAAAAA", "F", "Latin1_General_bin");
        }
        [Test]
        [TestCase("varchar(10)", "AAAAAAAA", "F", "Latin1_General_CI_AS")]
        public void CreateANOConfiguration_ValidTable(string datatype, string pattern, string suffix, string collation)
        {
            ANOOperation.DemandCreationOfNewConfiguration(ANOStore_ExternalDatabaseServer, "ANOFreakazilla", datatype, pattern, suffix, collation);
            Assert.Contains("ANOFreakazilla", ANOOperation.GetAllANOTables(ANOStore_ExternalDatabaseServer).ToArray());

            ANOOperation.RequestDeletionOfExistingConfiguration(ANOStore_ExternalDatabaseServer, "ANOFreakazilla");
            Assert.IsFalse(ANOOperation.GetAllANOTables(ANOStore_ExternalDatabaseServer).Contains("ANOFreakazilla"));
        }*/
    }
}
