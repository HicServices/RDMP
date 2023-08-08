// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class ServerDefaultsTests : DatabaseTests
{
    [Test]
    public void TestClearSameDefaultTwice()
    {
        Assert.IsNotNull(CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
        CatalogueRepository.ClearDefault(PermissableDefaults.LiveLoggingServer_ID);
        CatalogueRepository.ClearDefault(PermissableDefaults.LiveLoggingServer_ID);
        CatalogueRepository.ClearDefault(PermissableDefaults.LiveLoggingServer_ID);
        Assert.IsNull(CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
    }

    [Test]
    public void CreateNewExternalServerAndConfigureItAsDefault()
    {
        var databaseServer = new ExternalDatabaseServer(CatalogueRepository, "Deleteme", null);

        try
        {
            Assert.AreEqual("Deleteme", databaseServer.Name);
            databaseServer.Password = "nothing"; //automatically encrypts password

            Assert.AreNotEqual("nothing", databaseServer.Password); //should not match what we just set it to
            Assert.AreEqual("nothing",
                databaseServer
                    .GetDecryptedPassword()); //should match what we set it to because of explicit call to decrypt

            databaseServer.Server = "Bob";
            databaseServer.Database = "TEST";
            databaseServer.SaveToDatabase();

            var cata = new Catalogue(CatalogueRepository,
                "TestCatalogueFor_CreateNewExternalServerAndConfigureItAsDefault");
            cata.DeleteInDatabase();
        }
        finally
        {
            databaseServer.DeleteInDatabase();
        }
    }

    [Test]
    public void TestDeletingClearsDefault()
    {
        var eds = new ExternalDatabaseServer(CatalogueRepository, "mydb", new LoggingDatabasePatcher());
        var old = CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

        try
        {
            //make the new server the default for logging
            CatalogueRepository.SetDefault(PermissableDefaults.LiveLoggingServer_ID, eds);

            //now we deleted it!
            eds.DeleteInDatabase();

            Assert.IsNull(CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
        }
        finally
        {
            CatalogueRepository.SetDefault(PermissableDefaults.LiveLoggingServer_ID, old);
        }
    }
}