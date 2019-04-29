// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Diagnostics;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.DataLoad;
using Rdmp.Core.CommandLine.Options.Abstracts;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.CommandLine.AutomationLoopTests
{
    public class DLEEndToEndTestSetup
    {
        public DiscoveredServer ServerICanCreateRandomDatabasesAndTablesOn { get; set; }
        public SqlConnectionStringBuilder UnitTestLoggingConnectionString { get; set; }
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }
        public DiscoveredServer DiscoveredServerICanCreateRandomDatabasesAndTablesOn { get; set; }
        public ICatalogueRepository CatalogueRepository { get; set; }

        public DLEEndToEndTestSetup(DiscoveredServer serverICanCreateRandomDatabasesAndTablesOn, SqlConnectionStringBuilder unitTestLoggingConnectionString, IRDMPPlatformRepositoryServiceLocator repositoryLocator, DiscoveredServer discoveredServerICanCreateRandomDatabasesAndTablesOn)
        {
            ServerICanCreateRandomDatabasesAndTablesOn = serverICanCreateRandomDatabasesAndTablesOn;
            UnitTestLoggingConnectionString = unitTestLoggingConnectionString;
            RepositoryLocator = repositoryLocator;
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn = discoveredServerICanCreateRandomDatabasesAndTablesOn;
            CatalogueRepository = repositoryLocator.CatalogueRepository;
        }

        private UserAcceptanceTestEnvironment _stage1_setupCatalogue;
        private int _rowsBefore;
        private Catalogue _testCatalogue;
        private DirectoryInfo _testFolder;

        public void SetUp(int timeoutInMilliseconds,out LoadMetadata lmd)
        {
            var rootFolder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);;
            _testFolder = rootFolder.CreateSubdirectory("TestTheTestDatasetSetup");
            var datasetFolder = _testFolder.CreateSubdirectory("TestDataset");
            
            _stage1_setupCatalogue = new UserAcceptanceTestEnvironment((SqlConnectionStringBuilder) ServerICanCreateRandomDatabasesAndTablesOn.Builder, datasetFolder.FullName, UnitTestLoggingConnectionString, "Internal", null, null, RepositoryLocator);
            
            //create it all
            _stage1_setupCatalogue.Check(new AcceptAllCheckNotifier());

            //what did we create?
            _testCatalogue = _stage1_setupCatalogue.DemographyCatalogue;
            lmd = _stage1_setupCatalogue.DemographyCatalogue.LoadMetadata;

        }

        public void RecordPreExecutionState()
        {
            //the number of rows before data is automatically loaded (hopefully below)
            _rowsBefore = DataAccessPortal.GetInstance()
                .ExpectDatabase(_stage1_setupCatalogue.DemographyTableInfo, DataAccessContext.InternalDataProcessing)
                .ExpectTable("TestTableForDMP")
                .GetRowCount();

        }

        public void RunAutomationServiceToCompletion(int timeoutInMilliseconds, out int newRows)
        {
            
            //start an automation loop in the slot, it should pickup the load
            var auto = new DleRunner(new DleOptions() { LoadMetadata = _stage1_setupCatalogue.DemographyCatalogue.LoadMetadata.ID,Command = CommandLineActivity.run});
            auto.Run(RepositoryLocator,new ThrowImmediatelyDataLoadEventListener(), new ThrowImmediatelyCheckNotifier(), new GracefulCancellationToken());

            //also shouldn't be any logged errors
            var lm = new LogManager(_testCatalogue.LiveLoggingServer);

            var log = lm.GetArchivalDataLoadInfos(_testCatalogue.LoggingDataTask).FirstOrDefault();

            if(log == null)
                throw new Exception("No log messages found for logging task " + _testCatalogue.LoggingDataTask);

            Assert.AreEqual(0, log.Errors.Count);

            //number after
            var rowsAfter = DataAccessPortal.GetInstance()
                .ExpectDatabase(_stage1_setupCatalogue.DemographyTableInfo, DataAccessContext.InternalDataProcessing)
                .ExpectTable("TestTableForDMP")
                .GetRowCount();

            newRows = rowsAfter - _rowsBefore;
        }

        public void VerifyNoErrorsAfterExecutionThenCleanup(int timeoutInMilliseconds)
        {
            //RAW should have been cleaned up
            Assert.IsFalse(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("DMP_Test_RAW").Exists());
            
            _stage1_setupCatalogue.DestroyEnvironment();
            _testFolder.Delete(true);
        }
    }
}