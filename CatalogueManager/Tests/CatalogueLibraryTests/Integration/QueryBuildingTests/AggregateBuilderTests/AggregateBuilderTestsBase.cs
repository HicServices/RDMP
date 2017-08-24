using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.QueryBuildingTests.AggregateBuilderTests
{
    public class AggregateBuilderTestsBase : DatabaseTests
    {
        protected Catalogue _c;
        protected CatalogueItem _cataItem1;
        protected CatalogueItem _cataItem2;
        protected TableInfo _ti;
        protected ColumnInfo _columnInfo1;
        protected ColumnInfo _columnInfo2;
        protected ExtractionInformation _ei1;
        protected ExtractionInformation _ei2;
        protected AggregateConfiguration _configuration;
        protected AggregateDimension _dimension1;
        protected AggregateDimension _dimension2;

        [SetUp]
        public void CreateEntities()
        {
            _c = new Catalogue(CatalogueRepository, "AggregateBuilderTests");
            _cataItem1 = new CatalogueItem(CatalogueRepository, _c, "Col1");
            _cataItem2 = new CatalogueItem(CatalogueRepository, _c, "Col2");

            _ti = new TableInfo(CatalogueRepository, "T1");
            _columnInfo1 = new ColumnInfo(CatalogueRepository, "Col1", "varchar(100)", _ti);
            _columnInfo2 = new ColumnInfo(CatalogueRepository, "Col2", "date", _ti);

            _ei1 = new ExtractionInformation(CatalogueRepository, _cataItem1, _columnInfo1, _columnInfo1.Name);
            _ei2 = new ExtractionInformation(CatalogueRepository, _cataItem2, _columnInfo2, _columnInfo2.Name);

            _configuration = new AggregateConfiguration(CatalogueRepository, _c, "MyConfig");

            _dimension1 = new AggregateDimension(CatalogueRepository, _ei1, _configuration);
            _dimension2 = new AggregateDimension(CatalogueRepository, _ei2, _configuration);

            _dimension1.Order = 1;
            _dimension1.SaveToDatabase();
            _dimension2.Order = 2;
            _dimension2.SaveToDatabase();
        }


        [TearDown]
        public void DeleteEntities()
        {
            _configuration.DeleteInDatabase();
            _c.DeleteInDatabase();
            _ti.DeleteInDatabase();
        }
    }
}