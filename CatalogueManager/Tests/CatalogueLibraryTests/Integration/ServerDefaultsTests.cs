// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
            var originalTestLoggingServer = defaults.GetDefaultFor(PermissableDefaults.TestLoggingServer_ID);

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

                defaults.SetDefault(PermissableDefaults.TestLoggingServer_ID,databaseServer);
                Catalogue cata = new Catalogue(CatalogueRepository, "TestCatalogueFor_CreateNewExternalServerAndConfigureItAsDefault");

                Assert.AreEqual(databaseServer.ID, cata.TestLoggingServer_ID);
                cata.DeleteInDatabase();

            }
            finally
            {
                databaseServer.DeleteInDatabase();

                //Although we have not expressedly cleared the default, we have deleted the server which should cascade into the defaults table
                Assert.IsNull(defaults.GetDefaultFor(PermissableDefaults.TestLoggingServer_ID));

                //reset the original one
                if (originalTestLoggingServer != null)
                    defaults.SetDefault(PermissableDefaults.TestLoggingServer_ID, originalTestLoggingServer);
            }

        }
    }
}
