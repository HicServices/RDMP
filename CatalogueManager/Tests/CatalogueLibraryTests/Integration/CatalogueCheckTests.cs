using System;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class CatalogueCheckTests:DatabaseTests
    {
        private Catalogue _cata;
        private ColumnInfo _c2;
        private ColumnInfo _c1;
        private TableInfo _tableInfo;
        private CatalogueItem _cataItem;

        [TestFixtureSetUp]
        public void CreateCatalogueEntities()
        {
            _cata = new Catalogue(CatalogueRepository, "fish");
            _cataItem = new CatalogueItem(CatalogueRepository, _cata, "c");
            _tableInfo = new TableInfo(CatalogueRepository, "table");
            _c1 = new ColumnInfo(CatalogueRepository, "col1", "varchar(10)", _tableInfo);
            _c2 = new ColumnInfo(CatalogueRepository, "col2","varchar(10)",_tableInfo);
        }

        [TestFixtureTearDown]
        public void CleanupCatalougeEntities()
        {
            _cata.DeleteInDatabase();
            _tableInfo.DeleteInDatabase();
        }
        
        [Test]
        [ExpectedException(ExpectedMessage="The following invalid characters were found:'\\','.','#'",MatchType=MessageMatch.Contains)]
        public void CatalogueCheck_DodgyName()
        {
            //change in memory
            try {
                
                //catalogue is fine
                _cata.Check(new ThrowImmediatelyCheckNotifier());

                //name broken
                _cata.Name = @"c:\bob.txt#";
                _cata.Check(new ThrowImmediatelyCheckNotifier());
            }
            finally
            {
                //revert it and it is fine again
                _cata.RevertToDatabaseState();
                _cata.Check(new ThrowImmediatelyCheckNotifier());//should work 

            }

        }
    }
}
