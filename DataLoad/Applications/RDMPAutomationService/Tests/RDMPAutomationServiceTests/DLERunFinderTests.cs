using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.DataLoad;
using NUnit.Framework;
using RDMPAutomationService.Logic.DLE;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace RDMPAutomationServiceTests
{
    public class DLERunFinderTests:DatabaseTests
    {
        [Test]
        public void SuggestLoadMetadata_NoneExist()
        {
            DLERunFinder finder = new DLERunFinder(CatalogueRepository, new ThrowImmediatelyDataLoadEventListener());
            Assert.IsNull(finder.SuggestLoad());
        }

        [Test]
        public void SuggestLoadMetadata_NoCatalogues()
        {
            var lmd = new LoadMetadata(CatalogueRepository, "Ive got a lovely bunch of coconuts");
            var periodically = new LoadPeriodically(CatalogueRepository, lmd, 1);
            try
            {
                DLERunFinder finder = new DLERunFinder(CatalogueRepository, new ThrowImmediatelyDataLoadEventListener());
                Assert.IsNull(finder.SuggestLoad());
            }
            finally
            {
                periodically.DeleteInDatabase();
                lmd.DeleteInDatabase();
            }
        }

        [Test]
        public void SuggestLoadMetadata_AlreadyLocked()
        {
            Catalogue cata = new Catalogue(CatalogueRepository,"mycata");

            LoadMetadata lmd = new LoadMetadata(CatalogueRepository,"lmd");
            cata.LoadMetadata_ID = lmd.ID;
            cata.SaveToDatabase();

            var periodically = new LoadPeriodically(CatalogueRepository, lmd, 1);

            AutomationServiceSlot slot = new AutomationServiceSlot(CatalogueRepository);
            slot.Lock();
            AutomationJob job = slot.AddNewJob(AutomationJobType.DLE, "SuggestLoadMetadata_AlreadyLocked");
            job.LockCatalogues(new []{cata});
            try
            {
                DLERunFinder finder = new DLERunFinder(CatalogueRepository, new ThrowImmediatelyDataLoadEventListener());
                Assert.IsNull(finder.SuggestLoad());
            }
            finally
            {
                periodically.DeleteInDatabase();
                
                job.DeleteInDatabase();
                slot.Unlock();
                slot.DeleteInDatabase();
                cata.DeleteInDatabase();
                lmd.DeleteInDatabase();
            }
        }
        [Test]
        public void SuggestLoadMetadata_CircularDependency()
        {
            Catalogue cata = new Catalogue(CatalogueRepository, "mycata");
            Catalogue cata2 = new Catalogue(CatalogueRepository, "mycata2");


            LoadMetadata lmd = new LoadMetadata(CatalogueRepository, "lmd");
            LoadMetadata lmd2 = new LoadMetadata(CatalogueRepository, "lmd2");

            cata.LoadMetadata_ID = lmd.ID;
            cata.SaveToDatabase();

            cata2.LoadMetadata_ID = lmd2.ID;
            cata2.SaveToDatabase();

            var periodically = new LoadPeriodically(CatalogueRepository, lmd, 1);
            var periodically2 = new LoadPeriodically(CatalogueRepository, lmd2, 1);

            try
            {
                periodically.OnSuccessLaunchLoadMetadata_ID = lmd2.ID;
                periodically2.OnSuccessLaunchLoadMetadata_ID = lmd.ID;

                periodically.SaveToDatabase();
                var ex = Assert.Throws<Exception>(periodically2.SaveToDatabase);
                Assert.IsTrue(ex.Message.Contains("Circular reference encountered in IDs of LoadPeriodically"));
            }
            finally
            {
                periodically.DeleteInDatabase();
                periodically2.DeleteInDatabase();

                cata.DeleteInDatabase();
                cata2.DeleteInDatabase();

                lmd.DeleteInDatabase();
                lmd2.DeleteInDatabase();
            }
        }

        [Test]
        public void SuggestLoadMetadata_Normal()
        {
            Catalogue cata = new Catalogue(CatalogueRepository, "mycata");

            LoadMetadata lmd = new LoadMetadata(CatalogueRepository, "lmd");
            cata.LoadMetadata_ID = lmd.ID;
            cata.SaveToDatabase();

            var periodically = new LoadPeriodically(CatalogueRepository, lmd, 1);

            try
            {
                DLERunFinder finder = new DLERunFinder(CatalogueRepository, new ThrowImmediatelyDataLoadEventListener());
                Assert.AreEqual(periodically,finder.SuggestLoad());
            }
            finally
            {
                periodically.DeleteInDatabase();

                cata.DeleteInDatabase();
                lmd.DeleteInDatabase();
            }
        }


        [Test]
        public void SuggestLoadMetadata_SuggestHead()
        {
            //sets up a chain that goes cata3=> cata2=> cata1 where each => is a LMD+LoadPeriodically 
            Catalogue cata = new Catalogue(CatalogueRepository, "mycata");
            Catalogue cata2 = new Catalogue(CatalogueRepository, "mycata2");
            Catalogue cata3 = new Catalogue(CatalogueRepository, "mycata3");


            LoadMetadata lmd = new LoadMetadata(CatalogueRepository, "lmd");
            LoadMetadata lmd2 = new LoadMetadata(CatalogueRepository, "lmd2");
            LoadMetadata lmd3 = new LoadMetadata(CatalogueRepository, "lmd3"); 
            
            cata.LoadMetadata_ID = lmd.ID;
            cata.SaveToDatabase();

            cata2.LoadMetadata_ID = lmd2.ID;
            cata2.SaveToDatabase();

            cata3.LoadMetadata_ID = lmd3.ID;
            cata3.SaveToDatabase();

            //setup cata2=>cata1
            var periodically = new LoadPeriodically(CatalogueRepository, lmd2, 1);
            periodically.OnSuccessLaunchLoadMetadata_ID = lmd.ID;
            periodically.SaveToDatabase();

            //setup cata3=>cata2=>cata1
            var periodically2 = new LoadPeriodically(CatalogueRepository, lmd3, 1);
            periodically2.OnSuccessLaunchLoadMetadata_ID = lmd2.ID;
            periodically2.SaveToDatabase();

            try
            {

                Assert.IsFalse(periodically.IsLoadDue(null));
                Assert.IsTrue(periodically2.IsLoadDue(null));

                DLERunFinder finder = new DLERunFinder(CatalogueRepository, new ThrowImmediatelyDataLoadEventListener());
                Assert.AreEqual(periodically2, finder.SuggestLoad());
            }
            finally
            {
                periodically.DeleteInDatabase();
                periodically2.DeleteInDatabase();

                cata.DeleteInDatabase();
                cata2.DeleteInDatabase();
                cata3.DeleteInDatabase();

                lmd.DeleteInDatabase();
                lmd2.DeleteInDatabase();
                lmd3.DeleteInDatabase();
            }
        }

    }
}
