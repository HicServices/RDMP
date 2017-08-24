using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{

    public class LoadProgressTest : DatabaseTests
    {
        [Test]
        public void CreateNewScheduleTest()
        {
            var loadMetadata = new LoadMetadata(CatalogueRepository);
            var loadProgress = new LoadProgress(CatalogueRepository, loadMetadata);

            Assert.AreEqual(loadProgress.LoadMetadata_ID, loadMetadata.ID);

            loadProgress.DeleteInDatabase();
            loadMetadata.DeleteInDatabase();
        }

        [Test]
        public void LoadProgress_Equals()
        {
            var loadMetadata = new LoadMetadata(CatalogueRepository);


            LoadProgress progress = new LoadProgress(CatalogueRepository, loadMetadata);
            LoadProgress progressCopy = CatalogueRepository.GetObjectByID<LoadProgress>(progress.ID);
            
            progressCopy.Name = "fish";
            progressCopy.OriginDate = new DateTime(2001,01,01);
            
            try
            {
                //values are different
                Assert.AreNotEqual(progressCopy.OriginDate, progress.OriginDate);
                Assert.AreNotEqual(progressCopy.Name, progress.Name);

                //IDs are the same
                Assert.AreEqual(progressCopy.ID, progress.ID);

                //therefore objects are the same
                Assert.IsTrue(progressCopy.Equals(progress));

            }
            finally
            {
                progress.DeleteInDatabase();
                loadMetadata.DeleteInDatabase();
            }
        }
    }
}
