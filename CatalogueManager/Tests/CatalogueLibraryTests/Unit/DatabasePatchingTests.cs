using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.ExternalDatabaseServerPatching;
using CatalogueLibrary.Repositories;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using Rhino.Mocks;
using Tests.Common;

namespace CatalogueLibraryTests.Unit
{
    public class DatabasePatchingTests:DatabaseTests
    {
        [Test]
        public void CorrectlyDeterminesListOfDatabasesToPatch()
        {
            // mock the existence of a default logging server
            var defaultLoggingServer = MockRepository.GenerateStub<IExternalDatabaseServer>();
            defaultLoggingServer.Stub(s => s.ID).Return(4);

            var catalogues = new List<ICatalogue>
            {
                MockRepository.GenerateStub<ICatalogue>(),
                MockRepository.GenerateStub<ICatalogue>(),
                MockRepository.GenerateStub<ICatalogue>()
            };

            // Selection of catalogues which use a combination of unique, shared and null logging servers
            catalogues[0].LiveLoggingServer_ID = 1;
            catalogues[0].TestLoggingServer_ID = null;

            catalogues[1].LiveLoggingServer_ID = 2;
            catalogues[1].TestLoggingServer_ID = 3;

            catalogues[2].LiveLoggingServer_ID = null;
            catalogues[2].TestLoggingServer_ID = 2;
            
            // These are the servers that will be 'loaded' during the patching
            var server1 = new ExternalDatabaseServer(CatalogueRepository, "Server1");
            var server2 = new ExternalDatabaseServer(CatalogueRepository, "Server2");
            var server3 = new ExternalDatabaseServer(CatalogueRepository, "Server3");
            var server4 = new ExternalDatabaseServer(CatalogueRepository, "Server4");

            ServerDefaults d = new ServerDefaults(CatalogueRepository);
            
            var oldDefault = d.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);
            d.SetDefault(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID , server4);

            var c1 = new Catalogue(CatalogueRepository, "c1");
            c1.TestLoggingServer_ID = server1.ID;
            c1.LiveLoggingServer_ID = server2.ID;
            c1.SaveToDatabase();

            var c2 = new Catalogue(CatalogueRepository, "c2");
            c2.LiveLoggingServer_ID = server3.ID;
            c2.SaveToDatabase();
            
            try
            {
                var patcher = new LoggingDatabasePatcher(CatalogueRepository);
            
                // Act
                //patcher.CheckAndPatch(catalogues.ToArray());

                Assembly hostAssembly, dbAssembly;
                var result = patcher.FindDatabases(out hostAssembly, out dbAssembly);


                Assert.AreEqual("HIC.Logging", hostAssembly.GetName().Name);
                Assert.AreEqual("HIC.Logging.Database", dbAssembly.GetName().Name);

                Assert.Contains(server1,result);
                Assert.Contains(server2, result);
                Assert.Contains(server3, result);
                Assert.Contains(server4, result);
            }
            finally
            {
                if (oldDefault != null)
                    d.SetDefault(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID, oldDefault);

                c1.DeleteInDatabase();
                c2.DeleteInDatabase();

                server1.DeleteInDatabase();
                server2.DeleteInDatabase();
                server3.DeleteInDatabase();
                server4.DeleteInDatabase();
            }
        }
    }
}