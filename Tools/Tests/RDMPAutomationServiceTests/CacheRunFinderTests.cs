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
using CatalogueLibrary.Data.Pipelines;
using DataLoadEngineTests.Integration;
using DataLoadEngineTests.Integration.Cache;
using DataLoadEngineTests.Integration.PipelineTests;
using NUnit.Framework;
using RDMPAutomationService.Logic.Cache;
using RDMPAutomationServiceTests.AutomationLoopTests.FictionalCache;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace RDMPAutomationServiceTests
{
    public class CacheRunFinderTests:DatabaseTests
    {
        private Catalogue _cata;
        private LoadMetadata _lmd;
        private LoadProgress _lp;
        private CacheProgress _cp;
        private TestDataPipelineAssembler _pipelineAsembler;
        private HICProjectDirectory _hicProjectDirectory;
        
        private LoadMetadata _lmd2;
        private LoadProgress _lp2;
        private CacheProgress _cp2;


        [SetUp]
        public void SetupDatabaseObjects()
        {
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataWriter));
            RepositoryLocator.CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestDataInventor));

            _cata = new Catalogue(CatalogueRepository, "CataCacheRunFinderTests");
            
            //create an ongoing job (for tests that have max jobs 1 )
            _lmd2 = new LoadMetadata(CatalogueRepository, "Ive got a lovely bunch o' coconuts2");
            _lp2 = new LoadProgress(CatalogueRepository, _lmd2);
            _cp2 = new CacheProgress(CatalogueRepository, _lp2);

            _lmd = new LoadMetadata(CatalogueRepository, "Ive got a lovely bunch o' coconuts");
            _hicProjectDirectory = HICProjectDirectory.CreateDirectoryStructure(new DirectoryInfo(@"c:\temp\CacheRunFinderTests"), true);
            _lmd.LocationOfFlatFiles = _hicProjectDirectory.RootPath.FullName;
            _lmd.SaveToDatabase();

            _lp = new LoadProgress(CatalogueRepository, _lmd);
            _cp = new CacheProgress(CatalogueRepository, _lp);

            _pipelineAsembler = new TestDataPipelineAssembler("CacheRunFinderTests",CatalogueRepository);
            _pipelineAsembler.ConfigureCacheProgressToUseThePipeline(_cp);
            _cp.SaveToDatabase();
            
            _lp.OriginDate = new DateTime(2001,1,1);
            _lp.SaveToDatabase();

        }

        [TearDown]
        public void DeleteDatabaseObjects()
        {
            _cata.DeleteInDatabase();

            if (_cp != null)
                _cp.DeleteInDatabase();

            _cp2.DeleteInDatabase();

            _pipelineAsembler.Destroy();

            _lp.DeleteInDatabase();
            _lp2.DeleteInDatabase();

            _lmd.DeleteInDatabase();
            _lmd2.DeleteInDatabase();

            _hicProjectDirectory.RootPath.Delete(true);
        }

        [Test]
        public void SuggestCache_NoneExist()
        {
            //no caches
            _cp.DeleteInDatabase();
            _cp = null;//tell teardown not to delete it

            Assert.IsNull(new CacheRunFinder(CatalogueRepository, new ToMemoryDataLoadEventListener(false)).SuggestCacheProgress());

        }

        [Test]
        public void SuggestCache_NoCatalogues()
        {
            //Shouldn't suggest anything because although there is a cache there aren't any catalogues associated with it
            Assert.IsNull(new CacheRunFinder(CatalogueRepository, new ToMemoryDataLoadEventListener(false)).SuggestCacheProgress());
        }

        [Test]
        public void SuggestCache_NormalCase()
        {
            //associate the catalogue with the LoadMetadata
            _cata.LoadMetadata_ID = _lmd.ID;
            _cata.SaveToDatabase();

            //Catalogue exists and origin is known for dataset and no cache progress has yet been made
            Assert.AreEqual(_cp, new CacheRunFinder(CatalogueRepository, new ToMemoryDataLoadEventListener(false)).SuggestCacheProgress());
        }


        [Test]
        public void SuggestCache_NoDatasetDate()
        {
            //associate the catalogue with the LoadMetadata
            _cata.LoadMetadata_ID = _lmd.ID;
            _cata.SaveToDatabase();

            //make it so we don't know when the dataset starts so caching won't know how to proceede date wise
            _lp.OriginDate = null;
            _lp.SaveToDatabase();

            //shouldn't get suggested because we don't know the dates to cache from
            Assert.IsNull(new CacheRunFinder(CatalogueRepository, new ToMemoryDataLoadEventListener(false)).SuggestCacheProgress());
        }
        
        [Test]
        public void SuggestCache_NoPipeline()
        {
            //associate the catalogue with the LoadMetadata
            _cata.LoadMetadata_ID = _lmd.ID;
            _cata.SaveToDatabase();

            _cp.Pipeline_ID = null;
            _cp.SaveToDatabase();

            //There is no pipleline configured so caching cannot run
            Assert.IsNull(new CacheRunFinder(CatalogueRepository, new ToMemoryDataLoadEventListener(false)).SuggestCacheProgress());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SuggestCache_PermissionWindow(bool windowEncompasesNow)
        {
            //associate the catalogue with the LoadMetadata
            _cata.LoadMetadata_ID = _lmd.ID;
            _cata.SaveToDatabase();

            var window = new PermissionWindow(CatalogueRepository);
            try
            {
                var todayIs = DateTime.Now.DayOfWeek;

                var oneHourAgoIsh = new TimeSpan(Math.Max(DateTime.Now.Hour - 1, 0), 0, 0);
                var oneHourHence = new TimeSpan(DateTime.Now.Hour + 1, DateTime.Now.Minute, 0);
                var twoHoursHence = new TimeSpan(DateTime.Now.Hour + 2, DateTime.Now.Minute, 0);


                if(windowEncompasesNow)
                    window.PermissionWindowPeriods.Add(new PermissionWindowPeriod((int)todayIs, oneHourAgoIsh, twoHoursHence));
                else
                    window.PermissionWindowPeriods.Add(new PermissionWindowPeriod((int)todayIs,oneHourHence,twoHoursHence));
                window.SaveToDatabase();
                
                _cp.PermissionWindow_ID = window.ID;
                _cp.SaveToDatabase();

                if(windowEncompasesNow)
                    Assert.AreEqual(_cp, new CacheRunFinder(CatalogueRepository, new ToMemoryDataLoadEventListener(false)).SuggestCacheProgress());
                else
                    Assert.IsNull(new CacheRunFinder(CatalogueRepository, new ToMemoryDataLoadEventListener(false)).SuggestCacheProgress());
            }
            finally
            {
                _cp.PermissionWindow_ID = null;
                _cp.SaveToDatabase();
                window.DeleteInDatabase();
            }
        }

        [Test]
        [TestCase(5,0,0,false)]//cache is 5 months of future data loaded into it! expect it not to be suggested!
        [TestCase(5, 0,10, false)]//cache is 5 months of future data loaded into it! expect it not to be suggested!
        [TestCase(-5,0,0,true)]//cache is 5 months out of date, expect it to be suggested
        [TestCase(-5, 5, 0, false)]//cache is 5 months out of date, but there is a shortfall of 10 months (we are only supposed to load data up till 10 months ago)
        [TestCase(-5,0,10, false)]//cache is 5 months out of date, but there is a load delay trigger of 10 months
        [TestCase(-5, 3, 3, false)]//cache is 5 months out of date, but there is shortfall of 3 months + a load delay of 3 months
        [TestCase(-1,2,0,false)]//cache LAG period is 2 months and we are only 1 year out of date, expect it not to be suggeted
        [TestCase(-2,1,0,true)]//cache LAG period is 1 months but we are 2 months out of date, expect it to be suggested
        public void SuggestCache_Progress(int monthOffset, int lagPeriodInMonths,int loadDelayPeriodInMonths, bool expectLegal)
        {
            //associate the catalogue with the LoadMetadata
            _cata.LoadMetadata_ID = _lmd.ID;
            _cata.SaveToDatabase();
            
            //set lag period if theres a flag for it
            if (lagPeriodInMonths != 0)
                _cp.SetCacheLagPeriod(new CacheLagPeriod(lagPeriodInMonths, CacheLagPeriod.PeriodType.Month));
            _cp.SetCacheLagPeriodLoadDelay(new CacheLagPeriod(loadDelayPeriodInMonths,CacheLagPeriod.PeriodType.Month));

            //set the cache fill progress to 5 months before or 5 months after todays date depending on flag
            _cp.CacheFillProgress = DateTime.Now.AddMonths(monthOffset);
            _cp.SaveToDatabase();


            //shouldn't be recommending we execute this surely!
            if (expectLegal)
                Assert.AreEqual(_cp, new CacheRunFinder(CatalogueRepository, new ToMemoryDataLoadEventListener(false)).SuggestCacheProgress());
            else
                Assert.IsNull(new CacheRunFinder(CatalogueRepository, new ToMemoryDataLoadEventListener(false)).SuggestCacheProgress());
        }

    }
}
