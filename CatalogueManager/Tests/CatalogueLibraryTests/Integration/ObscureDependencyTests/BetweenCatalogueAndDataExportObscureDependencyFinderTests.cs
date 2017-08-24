using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using DataExportLibrary;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using RDMPStartup;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.ObscureDependencyTests
{
    public class BetweenCatalogueAndDataExportObscureDependencyFinderTests : DatabaseTests
    {
        [Test]
        public void PreventDeletingCatalogueBecauseOfLinkedDatasetTest()
        {
            var obscura = new BetweenCatalogueAndDataExportObscureDependencyFinder(RepositoryLocator);
            var cata = new Catalogue(CatalogueRepository, "MyCata");

            //catalogue exists in isolation so is deletable
            Assert.DoesNotThrow(()=>obscura.ThrowIfDeleteDisallowed(cata));

            //there is a new dataset which is linked to Catalogue
            var dataset = new ExtractableDataSet(DataExportRepository,cata);
            
            //and suddenly we cannot delete the catalogue
            var ex = Assert.Throws<Exception>(() => obscura.ThrowIfDeleteDisallowed(cata));
            Assert.IsTrue(ex.Message.Contains("Cannot delete Catalogue MyCata because there are ExtractableDataSets which depend on them "));

            //also if we try to force through a delete it should behave in identical manner
            var ex2 = Assert.Throws<Exception>(cata.DeleteInDatabase);
            Assert.IsTrue(ex2.Message.Contains("Cannot delete Catalogue MyCata because there are ExtractableDataSets which depend on them "));

            //now we delete the linked dataset
            dataset.DeleteInDatabase();

            //and because there is now no longer a dataset dependency on the catalogue we can delete it
            Assert.DoesNotThrow(() => obscura.ThrowIfDeleteDisallowed(cata));
            
            //and the delete works too
            cata.DeleteInDatabase();

            //both objects still exist in memory of course but we should be able to see they have disapeared 
            Assert.IsTrue(dataset.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted);
            Assert.IsTrue(cata.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted);
        }

        [Test]
        public void AllowDeletingWhenDataExportManagerIsNotSet()
        {
            var noDataExportManagerExists = new LinkedRepositoryProvider(RepositoryLocator.CatalogueRepository.ConnectionString,null);

            var obscura1 = new BetweenCatalogueAndDataExportObscureDependencyFinder(RepositoryLocator);
            var obscura2 = new BetweenCatalogueAndDataExportObscureDependencyFinder(noDataExportManagerExists);

            var cata = new Catalogue(CatalogueRepository, "MyCata");
            var dataset = new ExtractableDataSet(DataExportRepository,cata);

            //we cannot delete it because there is a dependency
            var ex = Assert.Throws<Exception>(() => obscura1.ThrowIfDeleteDisallowed(cata));
            Assert.IsTrue(ex.Message.Contains("Cannot delete Catalogue MyCata because there are ExtractableDataSets which depend on them "));

            //the second finder simulates when the registry doesn't have a record of the data export repository so it is unable to check it so it will let you delete it just fine
            Assert.DoesNotThrow(() => obscura2.ThrowIfDeleteDisallowed(cata));

            //now delete them in the correct order
            dataset.DeleteInDatabase();
            cata.DeleteInDatabase();

        }
    }
}
