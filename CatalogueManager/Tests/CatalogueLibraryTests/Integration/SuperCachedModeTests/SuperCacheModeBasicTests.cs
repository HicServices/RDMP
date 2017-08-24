using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable.RepositoryResultCaching;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Performance;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.SuperCachedModeTests
{
    public class SuperCacheModeBasicTests:DatabaseTests
    {
        [Test]
        public void SameObjectReturned_ByID()
        {
            var myCata = new Catalogue(CatalogueRepository, "MyCata");
            try
            {
                var fromDb1 = CatalogueRepository.GetObjectByID<Catalogue>(myCata.ID);
                var fromDb2 = CatalogueRepository.GetObjectByID<Catalogue>(myCata.ID);

                Assert.AreEqual(myCata,fromDb1);
                Assert.AreEqual(myCata, fromDb2);
                Assert.IsFalse(myCata == fromDb1);
                Assert.IsFalse(myCata == fromDb2);
                Assert.IsFalse(fromDb1 == fromDb2);
                
                Catalogue fromDb4;

                using(CatalogueRepository.SuperCachingMode())
                {
                    var fromDb3 = CatalogueRepository.GetObjectByID<Catalogue>(myCata.ID);
                    fromDb4 = CatalogueRepository.GetObjectByID<Catalogue>(myCata.ID);

                    Assert.IsTrue(fromDb3 == fromDb4);
                }


                var fromDb5 = CatalogueRepository.GetObjectByID<Catalogue>(myCata.ID);
                var fromDb6 = CatalogueRepository.GetObjectByID<Catalogue>(myCata.ID);

                Assert.IsFalse(fromDb5 == fromDb4);
                Assert.IsFalse(fromDb5 == fromDb6);

            }
            finally
            {
                myCata.DeleteInDatabase();
            }
        }

        [Test]
        public void SameObjectReturned_GetAll()
        {
            var myCata = new Catalogue(CatalogueRepository, "MyCata");

            try
            {
                using (CatalogueRepository.SuperCachingMode())
                {
                    var allcatas = CatalogueRepository.GetAllObjects<Catalogue>();
                    var getcopy = CatalogueRepository.GetObjectByID<Catalogue>(allcatas[0].ID);

                    Assert.AreEqual(allcatas[0],getcopy);
                    Assert.IsTrue(allcatas[0] ==  getcopy);
                }
            }
            finally
            {
                myCata.DeleteInDatabase();
            }
        }

        [Test]
        public void NoSaving()
        {
            var myCata = new Catalogue(CatalogueRepository, "MyCata");

            try
            {
                Assert.DoesNotThrow(myCata.SaveToDatabase);

                using (CatalogueRepository.SuperCachingMode())
                {
                    Assert.Throws<SuperCachingModeIsOnException>(myCata.SaveToDatabase);
                }
                Assert.DoesNotThrow(myCata.SaveToDatabase);
            }
            finally
            {
                myCata.DeleteInDatabase();
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void StillExistsTest_JustCallExists(bool doDelete)
        {
            var myCata = new Catalogue(CatalogueRepository, "MyCata");

            try
            {
                Assert.IsTrue(myCata.Exists());
                
                if (doDelete)
                {
                    myCata.DeleteInDatabase();
                    Assert.IsFalse(myCata.Exists());
                }

                var counter = new ComprehensiveQueryPerformanceCounter();
                DatabaseCommandHelper.PerformanceCounter = counter;
                Assert.AreEqual(counter.CacheHits,0); 

                using (CatalogueRepository.SuperCachingMode())
                {
                    Assert.AreEqual(myCata.Exists(),!doDelete);
                    Assert.AreEqual(counter.CacheHits, 0);
                    Assert.AreEqual(counter.CacheMisses, 1);

                    Assert.AreEqual(myCata.Exists(), !doDelete);
                    Assert.AreEqual(counter.CacheHits, 1);
                    Assert.AreEqual(counter.CacheMisses, 1);

                    Assert.AreEqual(myCata.Exists(), !doDelete);
                    Assert.AreEqual(counter.CacheHits, 2);
                    Assert.AreEqual(counter.CacheMisses, 1);

                }

                Assert.AreEqual(myCata.Exists(), !doDelete);
            }
            finally
            {
                if(!doDelete)
                    myCata.DeleteInDatabase();
            }
        }

        [Test]
        public void StillExistsTest_CallGetByID()
        {
            var myCata = new Catalogue(CatalogueRepository, "MyCata");

            try
            {
                Assert.IsTrue(myCata.Exists());
                var counter = new ComprehensiveQueryPerformanceCounter();
                DatabaseCommandHelper.PerformanceCounter = counter;
                Assert.AreEqual(counter.CacheHits, 0);

                using (CatalogueRepository.SuperCachingMode())
                {
                    //causes the cache to know the object exists because it returned it right?
                    CatalogueRepository.GetObjectByID<Catalogue>(myCata.ID);
                    Assert.AreEqual(counter.CacheHits, 0);
                    Assert.AreEqual(counter.CacheMisses, 1); //the get by ID shouldn't have been a cache hit

                    Assert.AreEqual(myCata.Exists(), true);
                    Assert.AreEqual(counter.CacheHits,1);
                    Assert.AreEqual(counter.CacheMisses, 1);

                    Assert.AreEqual(myCata.Exists(), true);
                    Assert.AreEqual(counter.CacheHits, 2);
                    Assert.AreEqual(counter.CacheMisses, 1);

                    Assert.AreEqual(myCata.Exists(), true);
                    Assert.AreEqual(counter.CacheHits, 3);
                    Assert.AreEqual(counter.CacheMisses, 1);


                }
            }
            finally
            {
                myCata.DeleteInDatabase();
            }
        }

        [Test]
        public void NoStaleObjectsTest()
        {
            var myCata = new Catalogue(CatalogueRepository, "MyCata");

            try
            {
                myCata.Name = "fishymcfish";
                myCata.SaveToDatabase();
                
                using (CatalogueRepository.SuperCachingMode())
                {
                    //causes the cache to know the object exists because it returned it right?
                    var cachedVersion = CatalogueRepository.GetObjectByID<Catalogue>(myCata.ID);
                    Assert.AreEqual(cachedVersion.Name, "fishymcfish");

                    CatalogueRepository.GetObjectByID<Catalogue>(myCata.ID);
                    Assert.AreEqual(cachedVersion.Name, "fishymcfish");
                }

                myCata.Name = "dogface";
                myCata.SaveToDatabase();

                using (CatalogueRepository.SuperCachingMode())
                {
                    //causes the cache to know the object exists because it returned it right?
                    var cachedVersion = CatalogueRepository.GetObjectByID<Catalogue>(myCata.ID);
                    Assert.AreEqual(cachedVersion.Name, "dogface");

                    CatalogueRepository.GetObjectByID<Catalogue>(myCata.ID);
                    Assert.AreEqual(cachedVersion.Name, "dogface");
                }

            }
            finally
            {
                myCata.DeleteInDatabase();
            }
        }
    }
}
