using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Cloning;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class CatalogueClonerTests : DatabaseTests
    {
        private Catalogue _sourceCatalogue;
        private CatalogueRepository _cloneRepository;
        const string testDatabaseName = "IntegrationTests_CatalogueRuntimeClonedDatabase";

        private Exception _setupException;
        private CatalogueCloner _cloner;

        protected override void SetUp()
        {
            try
            {
                base.SetUp();

                _sourceCatalogue = new Catalogue(CatalogueRepository, "CatalogueClonerTests");

                //setup new Catalogue databse to clone into
                //make copy of this so we can mess with Initial Catalog without confusing other unit tests

                var cloneConnectionStringBuilder =
                    new SqlConnectionStringBuilder(ServerICanCreateRandomDatabasesAndTablesOn.ConnectionString)
                    {
                        InitialCatalog = testDatabaseName
                    };
                
                _cloneRepository = new CatalogueRepository(cloneConnectionStringBuilder);
                DropTestDatabase(testDatabaseName);

                _cloner = new CatalogueCloner(CatalogueRepository, _cloneRepository);
                _cloner.CreateNewEmptyCatalogueDatabase(testDatabaseName, new ThrowImmediatelyCheckNotifier());
            }
            catch (Exception e)
            {
                _setupException = e;
            }

        }

        [TestFixtureTearDown]
        protected void TearDown()
        {
            _sourceCatalogue.DeleteInDatabase();
            DropTestDatabase(((SqlConnectionStringBuilder)_cloneRepository.ConnectionStringBuilder).InitialCatalog);
        }

        [Test]
        public void CloneIntoNewDatabase_AllCatalogues_SameNumberInBothCatalogues()
        {
            if (_setupException != null)
                throw _setupException;

            try
            {
                var catalogues = CatalogueRepository.GetAllCatalogues();
                int numberWeAttemptedToClone = catalogues.Length;
                int[] itemsInEachCatalogue = new int[catalogues.Length];

                for (int i = 0; i < catalogues.Length; i++)
                    itemsInEachCatalogue[i] = catalogues[i].CatalogueItems.Count();

                foreach (var catalogue in catalogues)
                {
                    var catalogueCloner = new CatalogueCloner(CatalogueRepository, _cloneRepository);
                    catalogueCloner.CloneIntoNewDatabase(catalogue, CatalogueCloner.CloneDepth.CatalogueItem, null, true);
                }

                int numberWeCloned = _cloneRepository.GetAllCatalogues().Count();

                //confirm that there are the same number of catalogue items in the new databas as the old for each catalogue
                for (int i = 0; i < catalogues.Length; i++)
                    Assert.AreEqual(itemsInEachCatalogue[i], catalogues[i].CatalogueItems.Count());

                Assert.AreEqual(numberWeAttemptedToClone,numberWeCloned);
            }
            finally
            {
                //nuke EVERYTHING in the new clone database
                foreach (var clonedCatalogue in _cloneRepository.GetAllCatalogues())
                    clonedCatalogue.DeleteInDatabase();
            }
        }

      
        
        [Test]
        public void CloneIntoNewDatabase_SingleCatalogue_ProducesSameSQL()
        {
            if (_setupException != null)
                throw _setupException;

            try
            {
                //cleanup old Catalogue
                IDeleteable toCleanup = CatalogueRepository.GetAllCatalogues().SingleOrDefault(c => c.Name.Equals("CloneIntoNewDatabase_SingleCatalogue_ProducesSameSQL"));
                if(toCleanup != null)
                    toCleanup.DeleteInDatabase();
                
                //cleanup old TableInfo
                toCleanup = CatalogueRepository.GetAllObjects<TableInfo>().SingleOrDefault(c => c.Name.Equals("BaddieTable"));
                if(toCleanup != null)
                    toCleanup.DeleteInDatabase();

                
                //create new Catalogue
                Catalogue toClone = new Catalogue(CatalogueRepository, "CloneIntoNewDatabase_SingleCatalogue_ProducesSameSQL");

                //create new catalogue item
                CatalogueItem cataItem = new CatalogueItem(CatalogueRepository, toClone, "Field1");

                //create new TableInfo
                TableInfo tableInfo = new TableInfo(CatalogueRepository, "BaddieTable");

                //create new column info
                ColumnInfo columnInfo = new ColumnInfo(CatalogueRepository, "Field1", "varchar(100)", tableInfo);

                //Create association between entities
                cataItem.SetColumnInfo(columnInfo);

                //create extraction SQL
                ExtractionInformation e = new ExtractionInformation(CatalogueRepository, cataItem, columnInfo,"LEN(BaddieTable..Field)")
                {
                    ExtractionCategory = ExtractionCategory.Core,
                    Alias = "LengthOfAField"
                };
                e.SaveToDatabase();

                var catalogueCloner = new CatalogueCloner(CatalogueRepository, _cloneRepository);
                catalogueCloner.CloneIntoNewDatabase(toClone, CatalogueCloner.CloneDepth.FullTree, null);
            
                var clonedCatalogue = _cloneRepository.GetObjectByID<Catalogue>(toClone.ID);

                Assert.IsNotNull(clonedCatalogue);
                Assert.AreEqual(toClone.Periodicity, clonedCatalogue.Periodicity);
                Assert.AreEqual(toClone.Name, clonedCatalogue.Name);
                Assert.AreEqual(toClone.Description, clonedCatalogue.Description);
                Assert.AreEqual(toClone.ValidatorXML, clonedCatalogue.ValidatorXML);
                Assert.AreEqual(toClone.ID, clonedCatalogue.ID); //even the IDs should be the same because we should be using IDENTITY_INSERT

                //iron test is that the Query coming out of the clone is the same as the query coming out of the main db
                QueryBuilder qbSource = new QueryBuilder(null,null);
                qbSource.AddColumnRange(toClone.GetAllExtractionInformation(ExtractionCategory.Supplemental));
                qbSource.AddColumnRange(toClone.GetAllExtractionInformation(ExtractionCategory.Core));
                string queryFromSource = qbSource.SQL;

                QueryBuilder qbDestination = new QueryBuilder(null,null);
                qbDestination.AddColumnRange(clonedCatalogue.GetAllExtractionInformation(ExtractionCategory.Supplemental));
                qbDestination.AddColumnRange(clonedCatalogue.GetAllExtractionInformation(ExtractionCategory.Core));

                string queryFromDestination = qbDestination.SQL;
                Assert.AreEqual(queryFromDestination,queryFromSource);

                toClone.DeleteInDatabase();
                tableInfo.DeleteInDatabase();
            }
            finally
            {
                //nuke EVERYTHING in the new clone database
                foreach (Catalogue cata in _cloneRepository.GetAllCatalogues())
                    cata.DeleteInDatabase();
            }
        }

        [Test]
        public void CreateDuplicateInSameDatabase_DatabaseHasCorrectTables_CloneCatalogue()
        {
            if (_setupException != null)
                throw _setupException;

            Catalogue toClone = new Catalogue(CatalogueRepository, "TestCatalogue");
            int toCloneID = toClone.ID;

            CatalogueCloner cc = new CatalogueCloner(CatalogueRepository, CatalogueRepository);
            Catalogue clone = cc.CreateDuplicateInSameDatabase(toClone);

            Assert.IsNotNull(clone);
            Assert.AreNotEqual(toClone.ID, clone.ID);

            try
            {
                Assert.AreEqual(toClone.Periodicity, clone.Periodicity);
                Assert.AreEqual(toClone.Name + "_DUPLICATE", clone.Name );
                Assert.AreEqual(toClone.Description, clone.Description);
                Assert.AreEqual(toClone.ValidatorXML, clone.ValidatorXML);

                //check it copied CatalogueItems too
                Assert.AreEqual(
                    toClone.CatalogueItems.Count(),
                    toClone.CatalogueItems.Count()
                    );
            }
            finally
            {
                //absolutely do not ever delete the thing we are trying to clone! - note that this is a double check but it is against original value, it 
                //is possible that clone is comprimised by this point but the original ID toCloneID should still be correct
                Debug.Assert(clone.ID != toCloneID);

                //clone was successfully created - delete it
                clone.DeleteInDatabase();

                //do not remove this assertion as we are about to nuke something we were told to clone, make damn sure we are deleting a ttest catalogue!
                Assert.AreEqual(toClone.Name, "TestCatalogue");
                toClone.DeleteInDatabase();
            }
        }

        #region Tests that are engineered to Throw
        [Test]
        [ExpectedException(ExpectedException = typeof(MissingTableException))]
        public void CloneIntoNewDatabase_DatabaseDoesntHaveCorrectTables_Throw()
        {
            if (_setupException != null)
                throw _setupException;

            var catalogue = new Catalogue(CatalogueRepository, "CloneIntoNewDatabase_DatabaseDoesntHaveCorrectTables_Throw");

            try
            {
                // build a repository pointing at an empty database to clone into (won't work because the empty database is actually 100% empty with no tables - as intended
                var destinationBuilder = new SqlConnectionStringBuilder(DatabaseICanCreateRandomTablesIn.ConnectionString);
                destinationBuilder.InitialCatalog = DatabaseICanCreateRandomTablesIn.InitialCatalog;
                var destinationRepository = new CatalogueRepository(destinationBuilder);

                var catalogueCloner = new CatalogueCloner(CatalogueRepository, destinationRepository);
                catalogueCloner.CloneIntoNewDatabase(catalogue, CatalogueCloner.CloneDepth.CatalogueOnly);
            }
            finally
            {
                catalogue.DeleteInDatabase();
            }
         
        }
        #endregion

        private void DropTestDatabase(string databaseToDrop)
        {
            if (_setupException != null)
                throw _setupException;

            string destinationConStr = ServerICanCreateRandomDatabasesAndTablesOn.ConnectionString;

            using (var con = new SqlConnection(destinationConStr))
            {
                try
                {
                    con.Open();
                    var cmdDelete = new SqlCommand(@"if db_id('" + databaseToDrop + @"') is not null 
begin
ALTER DATABASE " + databaseToDrop + @" SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
drop database " + databaseToDrop + @"
end", con);
                    cmdDelete.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Couldn't delete database: " + e);
                }
            }
        }
    }
}
