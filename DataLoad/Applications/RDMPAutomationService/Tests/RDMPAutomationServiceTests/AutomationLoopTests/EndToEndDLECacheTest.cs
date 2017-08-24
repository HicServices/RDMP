using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DataProvider;
using DataLoadEngine.DataProvider.FromCache;
using Diagnostics;
using NUnit.Framework;
using RDMPAutomationService.Logic.Cache;
using RDMPAutomationService.Logic.DLE;
using RDMPAutomationServiceTests.AutomationLoopTests.FictionalCache;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class EndToEndDLECacheTest:AutomationTests
    {
        [Test]
        public void RunEndToEndDLECacheTest()
        {
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataWritter));
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataInventor));

            int timeoutInMilliseconds = 120000;
            
            var setup = new DLEEndToEndTestSetup(ServerICanCreateRandomDatabasesAndTablesOn, UnitTestLoggingConnectionString,RepositoryLocator, DiscoveredServerICanCreateRandomDatabasesAndTablesOn);

            LoadMetadata lmd;
            setup.SetUp(timeoutInMilliseconds,out lmd);

            LoadProgress lp = new LoadProgress(CatalogueRepository,lmd);
            lp.DataLoadProgress = new DateTime(2001,1,1);
            lp.DefaultNumberOfDaysToLoadEachTime = 10;
            lp.AllowAutomation = true;
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

            //The run finder should be suggesting this run - See DLECacheRunFinderTest.cs
            Assert.AreEqual(lp,new DLERunFinder(CatalogueRepository).SuggestLoadBecauseCacheAvailable());

            var hicProjectDirectory = new HICProjectDirectory(lmd.LocationOfFlatFiles, false);

            //take the forLoading file
            var csvFile = hicProjectDirectory.ForLoading.GetFiles().Single();

            //and move it to the cache and give it a date in the range we expect for the cached data
            csvFile.MoveTo(Path.Combine(hicProjectDirectory.Cache.FullName,"2001-01-09.csv"));
                       
            setup.RecordPreExecutionState();

            int newRows;
            setup.RunAutomationServiceToCompletion(timeoutInMilliseconds,out newRows);

            Assert.AreEqual(10,newRows);

            Assert.AreEqual(0,hicProjectDirectory.Cache.GetFiles().Count());
            Assert.AreEqual(0, hicProjectDirectory.ForLoading.GetFiles().Count());
            Assert.AreEqual(1, hicProjectDirectory.ForArchiving.GetFiles().Count());
            
            var archiveFile = hicProjectDirectory.ForArchiving.GetFiles()[0];
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
