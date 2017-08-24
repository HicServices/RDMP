using System.Diagnostics;
using CatalogueLibrary.Data;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class ServerDefaultsTests : DatabaseTests
    {
        [Test]
        public void CreateNewExternalServerAndConfigureItAsDefault()
        {

            ServerDefaults defaults = new ServerDefaults(CatalogueRepository);
            var originalTestLoggingServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.TestLoggingServer_ID);

            var databaseServer = new ExternalDatabaseServer(CatalogueRepository, "Deleteme");

            try
            {
                Assert.AreEqual("Deleteme",databaseServer.Name);
                databaseServer.Password = "nothing"; //automatically encrypts password

                Assert.AreNotEqual("nothing",databaseServer.Password);//should not match what we just set it to
                Assert.AreEqual("nothing", databaseServer.GetDecryptedPassword());//should match what we set it to because of explicit call to decrypt

                databaseServer.Server = "Bob";
                databaseServer.Database = "TEST";
                databaseServer.SaveToDatabase();

                defaults.SetDefault(ServerDefaults.PermissableDefaults.TestLoggingServer_ID,databaseServer);
                Catalogue cata = new Catalogue(CatalogueRepository, "TestCatalogueFor_CreateNewExternalServerAndConfigureItAsDefault");

                Assert.AreEqual(databaseServer.ID, cata.TestLoggingServer_ID);
                cata.DeleteInDatabase();

            }
            finally
            {
                databaseServer.DeleteInDatabase();

                //Although we have not expressedly cleared the default, we have deleted the server which should cascade into the defaults table
                Assert.IsNull(defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.TestLoggingServer_ID));

                //reset the original one
                if (originalTestLoggingServer != null)
                    defaults.SetDefault(ServerDefaults.PermissableDefaults.TestLoggingServer_ID, originalTestLoggingServer);
            }

        }
    }
}
