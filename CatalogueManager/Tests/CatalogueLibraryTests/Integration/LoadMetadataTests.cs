using CatalogueLibrary.Data.DataLoad;
using NUnit.Framework;
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
 
     }
}
