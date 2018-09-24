using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints.Secondary;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.Validation
{
    public class ReferentialIntegrityConstraintTests :DatabaseTests
    {
        private TableInfo _tableInfo;
        private ColumnInfo[] _columnInfo;
        private ReferentialIntegrityConstraint _constraint;

        [TestFixtureSetUp]
        public void Setup()
        {
            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("ReferentialIntegrityConstraintTests");

            if(tbl.Exists())
                tbl.Drop();

            var server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            
            using (var con = server.GetConnection())
            {
                con.Open();

                server.GetCommand("CREATE TABLE ReferentialIntegrityConstraintTests(MyValue int)", con).ExecuteNonQuery();
                server.GetCommand("INSERT INTO ReferentialIntegrityConstraintTests (MyValue) VALUES (5)", con).ExecuteNonQuery();
            }

            TableInfoImporter importer = new TableInfoImporter(CatalogueRepository, tbl);
            importer.DoImport(out _tableInfo,out _columnInfo);

            _constraint = new ReferentialIntegrityConstraint(CatalogueRepository);
            _constraint.OtherColumnInfo = _columnInfo.Single();
        }

        [Test]
        [TestCase(5, false)]
        [TestCase("5", false)]
        [TestCase(4, true)]
        [TestCase(6, true)]
        [TestCase(-5, true)]
        public void NormalLogic(object value, bool expectFailure)
        {
            _constraint.InvertLogic = false;
            ValidationFailure failure = _constraint.Validate(value, null, null);

            //if it did not fail validation and we expected failure
            if(failure == null && expectFailure)
                Assert.Fail();

            //or it did fail validation and we did not expect failure
            if(failure != null && !expectFailure)
                Assert.Fail();

            Assert.Pass();
        }


        [Test]
        [TestCase(5, true)]
        [TestCase("5", true)]
        [TestCase(4, false)]
        [TestCase(6, false)]
        [TestCase(-5, false)]
        public void InvertedLogic(object value, bool expectFailure)
        {
            _constraint.InvertLogic = true;
            ValidationFailure failure = _constraint.Validate(value, null, null);

            //if it did not fail validation and we expected failure
            if (failure == null && expectFailure)
                Assert.Fail();

            //or it did fail validation and we did not expect failure
            if (failure != null && !expectFailure)
                Assert.Fail();

            Assert.Pass();
        }

        

        [TestFixtureTearDown]
        public void Drop()
        {
            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("ReferentialIntegrityConstraintTests");
            
            if(tbl.Exists())
                tbl.Drop();

            var credentials = (DataAccessCredentials)_tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
            _tableInfo.DeleteInDatabase();

            if(credentials != null)
                credentials.DeleteInDatabase();
        }
    }
}
