// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    
    public class VersioningTests :DatabaseTests
    {
        [Test]
        public void MasterDatabaseScriptExecutor_CreateDatabase()
        {
            string dbName = "CreateANewCatalogueDatabaseWithMasterDatabaseScriptExecutor";

            var database = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbName);
            
            if(database.Exists())
                database.Drop();

            MasterDatabaseScriptExecutor executor = new MasterDatabaseScriptExecutor(database);
            executor.CreateDatabase(@"
CREATE TABLE Bob
(
age int
)
GO", "1.0.0.0", new ThrowImmediatelyCheckNotifier());

            var versionTable = database.ExpectTable("Version");
            var bobTable = database.ExpectTable("Bob");

            Assert.IsTrue(versionTable.Exists());
            Assert.IsTrue(bobTable.Exists());

            database.Drop();
        }

    }
}
