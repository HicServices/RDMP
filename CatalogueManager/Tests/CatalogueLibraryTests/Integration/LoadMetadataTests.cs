// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Checks.Checkers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class LoadMetadataTests : DatabaseTests
    {
        [Test]
        public void CreateNewAndGetBackFromDatabase()
        {
            var loadMetadata = new LoadMetadata(CatalogueRepository);

            try
            {
                loadMetadata.LocationOfFlatFiles = "C:\\temp";
                loadMetadata.SaveToDatabase();
                
                var loadMetadataWithIdAfterwards = CatalogueRepository.GetObjectByID<LoadMetadata>(loadMetadata.ID);
                Assert.AreEqual(loadMetadataWithIdAfterwards.LocationOfFlatFiles, "C:\\temp");
            }
            finally
            {
                loadMetadata.DeleteInDatabase();
            }
        }

        [Test]
        public void TestPreExecutionChecker_TablesDontExist()
        {
            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("Imaginary");

            Assert.IsFalse(tbl.Exists());

            var lmd = RdmpMockFactory.Mock_LoadMetadataLoadingTable(tbl);
            var checker = new PreExecutionChecker(lmd, new HICDatabaseConfiguration(DiscoveredDatabaseICanCreateRandomTablesIn.Server));
            var ex = Assert.Throws<Exception>(()=>checker.Check(new ThrowImmediatelyCheckNotifier()));

            StringAssert.IsMatch("Table '.*Imaginary.*' does not exist", ex.Message);
        }
        [Test]
        public void TestPreExecutionChecker_TableIsTableValuedFunction()
        {
            TestableTableValuedFunction f = new TestableTableValuedFunction();
            f.Create(DiscoveredDatabaseICanCreateRandomTablesIn,CatalogueRepository);

            var tbl = f.TableInfoCreated.Discover(DataAccessContext.DataLoad);
            Assert.IsTrue(tbl.Exists());

            var lmd = RdmpMockFactory.Mock_LoadMetadataLoadingTable(f.TableInfoCreated);
            var checker = new PreExecutionChecker(lmd, new HICDatabaseConfiguration(DiscoveredDatabaseICanCreateRandomTablesIn.Server));
            var ex = Assert.Throws<Exception>(() => checker.Check(new ThrowImmediatelyCheckNotifier()));

            StringAssert.IsMatch("Table '.*MyAwesomeFunction.*' is a TableValuedFunction", ex.Message);
        }
     }
}
