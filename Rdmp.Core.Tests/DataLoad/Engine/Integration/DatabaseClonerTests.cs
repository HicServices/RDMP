// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CatalogueLibrary;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations;
using ReusableLibraryCode.Progress;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration
{
    class DatabaseClonerTests : TestsRequiringFullAnonymisationSuite
    {
        private Exception _setupException;

        private UserAcceptanceTestEnvironmentHelper _userAcceptanceTestEnvironmentHelper;

        [OneTimeSetUp]
        protected void CallUserAcceptanceTestEnvironmentHelper_SetUp()
        {
            try
            {
                _userAcceptanceTestEnvironmentHelper = new UserAcceptanceTestEnvironmentHelper(
                    DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder.ConnectionString,
                    CatalogueRepository.ConnectionString,
                    UnitTestLoggingConnectionString.ConnectionString, 
                    ANOStore_Database.Server.Builder.ConnectionString, 
                    IdentifierDump_Database.Server.Builder.ConnectionString,
                    RepositoryLocator);

                _userAcceptanceTestEnvironmentHelper.SetUp();
            }
            catch (Exception e)
            {
                _setupException = e;
            }
        }

        [OneTimeTearDown]
        protected void AfterAllDatabaseClonerTests()
        {
            _userAcceptanceTestEnvironmentHelper.TearDown();
        }

        [SetUp]
        public void BeforeEachTest()
        {
            if (_setupException != null)
            {
                Console.WriteLine(_setupException.Message);
                throw _setupException;
            }
        }

        [Test]
        public void TestRawDatabaseCreationWithoutANOConfiguration()
        {
            var testEnvironment = _userAcceptanceTestEnvironmentHelper.TestEnvironment;
            var databaseNamingScheme = new FixedStagingDatabaseNamer("fish");
            var hicDatabaseConfig = new HICDatabaseConfiguration(testEnvironment.DemographyCatalogue.LoadMetadata);
            
            var rawDbInfo = hicDatabaseConfig.DeployInfo[LoadBubble.Raw];
            
            if (rawDbInfo.Exists())
            {
                foreach (var t in rawDbInfo.DiscoverTables(true))
                    t.Drop();

                rawDbInfo.Drop();
            }
            
            var cloner = new DatabaseCloner(hicDatabaseConfig);
            cloner.CreateDatabaseForStage(LoadBubble.Raw);
            cloner.CreateTablesInDatabaseFromCatalogueInfo(new ThrowImmediatelyDataLoadEventListener(), testEnvironment.DemographyTableInfo, LoadBubble.Raw);
            
            var table = hicDatabaseConfig.DeployInfo.DatabaseInfoList[LoadBubble.Raw].ExpectTable(testEnvironment.DemographyTableInfo.GetRuntimeName());
            Assert.IsTrue(table.Exists());

            // ensure that the RAW tables doesn't have any hic_ fields in it
            Assert.IsFalse(table.DiscoverColumns().Any(c => c.GetRuntimeName().StartsWith("hic_")));

            cloner.LoadCompletedSoDispose(ExitCodeType.Success, null);
        }


        [Test]
        public void TestRawDatabaseCreationWithANOConfiguration()
        {
            var testEnvironment = _userAcceptanceTestEnvironmentHelper.TestEnvironment;
            var hicDatabaseConfig = new HICDatabaseConfiguration(testEnvironment.DemographyCatalogue.LoadMetadata);

            var rawDbInfo = hicDatabaseConfig.DeployInfo[LoadBubble.Raw];

            if (rawDbInfo.Exists())
            {
                foreach (DiscoveredTable t in rawDbInfo.DiscoverTables(true))
                    t.Drop();

                rawDbInfo.Drop();
            }

            var cloner = new DatabaseCloner(hicDatabaseConfig);
            cloner.CreateDatabaseForStage(LoadBubble.Raw);
            cloner.CreateTablesInDatabaseFromCatalogueInfo(new ThrowImmediatelyDataLoadEventListener(),testEnvironment.DemographyTableInfo, LoadBubble.Raw);

            // The data type of the ANOCHI column in the LIVE database should be varchar(12), but in the RAW database it should be non-ANO and varchar(10)

            // Check column in 'live' database is as expected
            var liveDbInfo = hicDatabaseConfig.DeployInfo[LoadBubble.Live];
            var liveTable = liveDbInfo.ExpectTable(testEnvironment.DemographyTableInfo.GetRuntimeName());

            Assert.IsTrue(liveTable.Exists());
            
            var anoCHIColumn = liveTable.DiscoverColumn("ANOCHI");
            Assert.IsTrue(anoCHIColumn.DataType.SQLType.Equals("varchar(12)"));

            // now check the RAW column is the 'non-ANO' version
            var rawTable = rawDbInfo.ExpectTable(testEnvironment.DemographyTableInfo.GetRuntimeName());
            
            Assert.IsTrue(rawTable.Exists());

            var columns = rawTable.DiscoverColumns();
            var chiCol = columns.SingleOrDefault(c => c.GetRuntimeName().Equals("CHI"));

            Assert.IsNotNull(chiCol, "CHI column not found in RAW database");
            Assert.AreEqual("varchar(10)", chiCol.DataType.SQLType);
            
            cloner.LoadCompletedSoDispose(ExitCodeType.Success, null);
        }
    }
}
