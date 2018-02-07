using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using NUnit.Framework;
using RDMPAutomationService.Logic.DLE;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace RDMPAutomationServiceTests
{
    public class DLECacheRunFinderTests:DatabaseTests
    {
        [Test]
        public void SuggestLoadMetadata_NoneExist()
        {
            DLERunFinder finder = new DLERunFinder(CatalogueRepository, new ThrowImmediatelyDataLoadEventListener());
            Assert.IsNull(finder.SuggestLoadBecauseCacheAvailable());
        }

             private object[] _sourceLists = {
                                            new object[] {null,0,null,true,false},  //case 1 - no progress made
                                            new object[] {new DateTime(2001,1,1),10,null,true,false},  //case 2 - progress but no cache
                                            new object[] {new DateTime(2001,1,1),10,new DateTime(1999,1,1),true,false},  //case 3 - progress but no cache

                                            new object[] {new DateTime(2001,1,1),10,new DateTime(2001,1,10),true,false},  //case 4 - 9 days is available to load
                                            new object[] {new DateTime(2001,1,1),10,new DateTime(2001,1,11),true,true},  //case 5 - 10 days is available to load
                                            new object[] {new DateTime(2001,1,1),10,new DateTime(2001,1,12),true,true},  //case 6 - 11 days is available to load
                                            new object[] {new DateTime(2001,1,1),10,new DateTime(2001,1,12),false,false},  //case 7 - 11 days is available to load but LoadProgress.AllowAutomation is false
                                            new object[] {new DateTime(2001,1,1),0,new DateTime(2001,1,12),true,false},  //case 8 - 11 days but load is zero
                                        };

        [Test, TestCaseSource("_sourceLists")]
        public void SuggestLoadMetadata_ProgressVsCacheDates(DateTime? origin, int daysToLoad, DateTime? cacheDate, bool allowAutomation, bool expectedSuggestRunning)
        {
            var lmd = new LoadMetadata(CatalogueRepository, "Ive got a lovely bunch of coconuts");
            var lp = new LoadProgress(CatalogueRepository, lmd);
            var cp = new CacheProgress(CatalogueRepository, lp);

            lp.AllowAutomation = allowAutomation;
            lp.SaveToDatabase();

            //create a catalogue
            Catalogue c = new Catalogue(CatalogueRepository, "cata Fish");
            c.LoadMetadata_ID = lmd.ID;
            c.SaveToDatabase();

            try
            {
                DLERunFinder finder = new DLERunFinder(CatalogueRepository, new ThrowImmediatelyDataLoadEventListener());

                //No catalogues
                Assert.IsNull(finder.SuggestLoadBecauseCacheAvailable());
                
                cp.CacheFillProgress = cacheDate;
                cp.SaveToDatabase();

                lp.DataLoadProgress = origin;
                lp.DefaultNumberOfDaysToLoadEachTime = daysToLoad;
                lp.SaveToDatabase();

                if(expectedSuggestRunning)
                    Assert.AreEqual(lp,finder.SuggestLoadBecauseCacheAvailable());
                else
                    Assert.IsNull(finder.SuggestLoadBecauseCacheAvailable());
            }
            finally
            {
                c.DeleteInDatabase();
                cp.DeleteInDatabase();
                lp.DeleteInDatabase();
                lmd.DeleteInDatabase();

            }
        }

        [TestCase(false,false,false,false,true)] //No locks, suggest run
        [TestCase(false, true, false, false, true)] //Window but not a locked window, suggest run
        [TestCase(true, true, false, false, false)] //Catalogue locked because DQE running on it, do not suggest
        [TestCase(false, true, true, false, false)] //Window which is locked, do not suggest
        [TestCase(false, true, false, true, false)] //Load Progress is locked e.g. running in DLE manually now or crashed previously or something, do not suggest
        public void SuggestLoadMetadata_Locks(bool lockCatalogue, bool createPermissionWindow, bool lockPermissionWindow, bool lockLoadProgress, bool expectedSuggestRunning)
        {
            var lmd = new LoadMetadata(CatalogueRepository, "Ive got a lovely bunch of coconuts");
            var lp = new LoadProgress(CatalogueRepository, lmd);
            lp.AllowAutomation = true;
            lp.SaveToDatabase();
            var cp = new CacheProgress(CatalogueRepository, lp);

            AutomationServiceSlot slot = null;
            AutomationJob job = null;
            PermissionWindow window = null;

            //create a catalogue
            Catalogue c = new Catalogue(CatalogueRepository, "cata Fish");
            c.LoadMetadata_ID = lmd.ID;
            c.SaveToDatabase();

            try
            {
                DLERunFinder finder = new DLERunFinder(CatalogueRepository, new ThrowImmediatelyDataLoadEventListener());
                //Shouldnt be any remnant suggestions
                Assert.IsNull(finder.SuggestLoadBecauseCacheAvailable());
                
                //10 days are available to load
                cp.CacheFillProgress = new DateTime(2001, 1, 11); 
                cp.SaveToDatabase();

                lp.DataLoadProgress = new DateTime(2001, 1, 1);
                lp.DefaultNumberOfDaysToLoadEachTime = 10; //and the load size is 10
                lp.SaveToDatabase();


                if (lockCatalogue)
                {
                    slot = new AutomationServiceSlot(CatalogueRepository);
                    job =  slot.AddNewJob( AutomationJobType.DQE, "DQE running on dataset");
                    job.LockCatalogues(new []{c});
                }

                if (createPermissionWindow)
                {

                    window = new PermissionWindow(CatalogueRepository);
                    cp.PermissionWindow_ID = window.ID;
                    cp.SaveToDatabase();
                }

                if(lockPermissionWindow)
                    window.Lock();

                if(lockLoadProgress)
                    lp.Lock();

                if (expectedSuggestRunning)
                    Assert.AreEqual(lp, finder.SuggestLoadBecauseCacheAvailable());
                else
                    Assert.IsNull(finder.SuggestLoadBecauseCacheAvailable());
            }
            finally
            {
                if (slot != null)
                {
                    job.DeleteInDatabase();
                    slot.DeleteInDatabase();
                }

                c.DeleteInDatabase();
                cp.DeleteInDatabase();
                
                if (window != null)
                    window.DeleteInDatabase();
                
                lp.DeleteInDatabase();
                lmd.DeleteInDatabase();

            }
        }

    }
}
