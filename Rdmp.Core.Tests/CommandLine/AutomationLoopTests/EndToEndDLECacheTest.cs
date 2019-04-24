// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DataProvider.FromCache;
using NUnit.Framework;
using Tests.Common;
using Tests.Common.Helpers;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class EndToEndDLECacheTest:DatabaseTests
    {
        [Test]
        public void RunEndToEndDLECacheTest()
        {
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataWriter));
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataInventor));

            int timeoutInMilliseconds = 120000;
            
            var setup = new DLEEndToEndTestSetup(
                DiscoveredServerICanCreateRandomDatabasesAndTablesOn,
                UnitTestLoggingConnectionString,
                RepositoryLocator,
                DiscoveredServerICanCreateRandomDatabasesAndTablesOn);

            LoadMetadata lmd;
            setup.SetUp(timeoutInMilliseconds,out lmd);

            LoadProgress lp = new LoadProgress(CatalogueRepository,lmd);
            lp.DataLoadProgress = new DateTime(2001,1,1);
            lp.DefaultNumberOfDaysToLoadEachTime = 10;
            lp.SaveToDatabase();

            var cp = new CacheProgress(CatalogueRepository, lp);
            cp.CacheFillProgress = new DateTime(2001,1,11); //10 days available to load
            cp.SaveToDatabase();

            var assembler = new TestDataPipelineAssembler("RunEndToEndDLECacheTest pipe", CatalogueRepository);
            assembler.ConfigureCacheProgressToUseThePipeline(cp);

            //setup the cache process task
            var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.GetFiles);
            pt.Path = typeof (BasicCacheDataProvider).FullName;
            pt.ProcessTaskType = ProcessTaskType.DataProvider;
            pt.SaveToDatabase();
            pt.CreateArgumentsForClassIfNotExists<BasicCacheDataProvider>();

            var attacher = lmd.ProcessTasks.Single(p => p.ProcessTaskType == ProcessTaskType.Attacher);
            var patternArgument  = (ProcessTaskArgument)attacher.GetAllArguments().Single(a => a.Name.Equals("FilePattern"));
            patternArgument.SetValue("*.csv");
            patternArgument.SaveToDatabase();

            
            var LoadDirectory = new LoadDirectory(lmd.LocationOfFlatFiles);

            //take the forLoading file
            var csvFile = LoadDirectory.ForLoading.GetFiles().Single();

            //and move it to the cache and give it a date in the range we expect for the cached data
            csvFile.MoveTo(Path.Combine(LoadDirectory.Cache.FullName,"2001-01-09.csv"));
                       
            setup.RecordPreExecutionState();

            int newRows;
            setup.RunAutomationServiceToCompletion(timeoutInMilliseconds,out newRows);

            Assert.AreEqual(10,newRows);

            Assert.AreEqual(0,LoadDirectory.Cache.GetFiles().Count());
            Assert.AreEqual(0, LoadDirectory.ForLoading.GetFiles().Count());
            Assert.AreEqual(1, LoadDirectory.ForArchiving.GetFiles().Count());
            
            var archiveFile = LoadDirectory.ForArchiving.GetFiles()[0];
            Assert.AreEqual(".zip",archiveFile.Extension);


            //load progress should be updated to the largest date in the cache (2001-01-09)
            lp.RevertToDatabaseState();
            Assert.AreEqual(lp.DataLoadProgress, new DateTime(2001,01,09));

            cp.DeleteInDatabase();
            lp.DeleteInDatabase();

            assembler.Destroy();

            setup.VerifyNoErrorsAfterExecutionThenCleanup(timeoutInMilliseconds);


            
        }
    }
}
