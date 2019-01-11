using System;
using System.Data;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using FAnsi.Naming;
using NUnit.Framework;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace QueryCachingTests
{
    public class ExtractableAggregateCachingTests:QueryCachingDatabaseTests
    {

        private Catalogue _cata;
        private AggregateConfiguration _config;
        private CachedAggregateConfigurationResultsManager _manager;
        private TableInfo _table;
        private ColumnInfo _columnInfo;
        private CatalogueItem _catalogueItem;
        private ExtractionInformation _extractionInformation;

        [SetUp]
        public void CreateEntities()
        {

            _cata =
               new Catalogue(CatalogueRepository,"ExtractableAggregateCachingTests");

            _table = new TableInfo(CatalogueRepository,"ExtractableAggregateCachingTests");
            _columnInfo = new ColumnInfo(CatalogueRepository,"Col1", "varchar(1000)", _table);
            
            _catalogueItem = new CatalogueItem(CatalogueRepository,_cata, "Col1");


            _extractionInformation = new ExtractionInformation(CatalogueRepository, _catalogueItem, _columnInfo, "Col1");


            _config
                =
                new AggregateConfiguration(CatalogueRepository, _cata, "ExtractableAggregateCachingTests");

            _manager = new CachedAggregateConfigurationResultsManager(QueryCachingDatabaseServer);

        }

        [TearDown]
        public void TearDown()
        {
            _config.DeleteInDatabase();
            _extractionInformation.DeleteInDatabase();
            _table.DeleteInDatabase();
            _cata.DeleteInDatabase();
        }

        [Test]
        public void BasicCase()
        {
           var ex = Assert.Throws<ArgumentException>(()=>_manager.CommitResults(new CacheCommitExtractableAggregate(_config,"I've got a lovely bunch of coconuts",new DataTable(),30 )));

                
            Assert.IsTrue(ex.Message.StartsWith("The DataTable that you claimed was an ExtractableAggregateResults had zero columns and therefore cannot be cached"));
            
            DataTable dt = new DataTable();
            dt.Columns.Add("Col1");
            dt.Rows.Add("fishy!");

            var ex2 = Assert.Throws<NotSupportedException>(() => _manager.CommitResults(new CacheCommitExtractableAggregate(_config, "I've got a lovely bunch of coconuts", dt, 30)));

            Assert.IsTrue(
                ex2.Message.StartsWith(
                "Aggregate ExtractableAggregateCachingTests is not marked as IsExtractable therefore cannot be cached"));

            

            _config.IsExtractable = true;
            _config.SaveToDatabase();


            //make the underlying column an is extraction identifier
            _extractionInformation.IsExtractionIdentifier = true;
            _extractionInformation.SaveToDatabase();

            AggregateDimension dim = new AggregateDimension(CatalogueRepository, _extractionInformation, _config);

            var ex3 = Assert.Throws<NotSupportedException>(() => _manager.CommitResults(new CacheCommitExtractableAggregate(_config, "I've got a lovely bunch of coconuts", dt, 30)));

            Assert.IsTrue(
                ex3.Message.StartsWith(
                "Aggregate ExtractableAggregateCachingTests contains dimensions marked as IsExtractionIdentifier or HashOnDataRelease (Col1)"));

            _extractionInformation.IsExtractionIdentifier = false;
            _extractionInformation.SaveToDatabase();

            Assert.DoesNotThrow(() => _manager.CommitResults(new CacheCommitExtractableAggregate(_config, "I've got a lovely bunch of coconuts", dt, 30)));

            dim.DeleteInDatabase();


            using (var con = DataAccessPortal.GetInstance().ExpectServer(QueryCachingDatabaseServer, DataAccessContext.InternalDataProcessing).GetConnection())
            {
                
                IHasFullyQualifiedNameToo table = _manager.GetLatestResultsTableUnsafe(_config, AggregateOperation.ExtractableAggregateResults);
    
                con.Open();
                var r = DatabaseCommandHelper.GetCommand("Select * from " + table.GetFullyQualifiedName(), con).ExecuteReader();

                Assert.IsTrue(r.Read());
                Assert.AreEqual("fishy!",r["Col1"]);

            }
            

        }
    }
}
