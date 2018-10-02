using System;
using System.Linq;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using ReusableLibraryCode.DatabaseHelpers.Discovery.MySql;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using Tests.Common;

namespace DataExportLibrary.Tests
{
    public class TypeTranslaterTests : DatabaseTests
    {
        private DiscoveredDatabase _msDb;
        private DiscoveredDatabase _mysqlDb;
        private DiscoveredDatabase _oracleDb;

        [TestFixtureSetUp]
        public void SetupDatabases()
        {
            _msDb = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

            try
            {
                _mysqlDb = GetCleanedServer(DatabaseType.MYSQLServer);
            }
            catch (InconclusiveException)
            {
                
            }
            try
            {
                _oracleDb = GetCleanedServer(DatabaseType.Oracle);
            }
            catch (InconclusiveException)
            {
                
            }
        }


        [Test]
        public void Test_CSharpToDbType_String()
        {
            var cSharpType = new DatabaseTypeRequest(typeof (string), 10, null);

            var ms = new MicrosoftSQLTypeTranslater();
            var mysql = new MySqlTypeTranslater();
            var oracle = new OracleTypeTranslater();

            Assert.AreEqual("varchar(10)",ms.GetSQLDBTypeForCSharpType(cSharpType));
            Assert.AreEqual("varchar(10)", mysql.GetSQLDBTypeForCSharpType(cSharpType));
            Assert.AreEqual("varchar2(10)", oracle.GetSQLDBTypeForCSharpType(cSharpType));
        }
        
        [Test]
        public void Test_CSharpToDbType_String_Max()
        {
            var cSharpType = new DatabaseTypeRequest(typeof(string), 10000000, null);

            var ms = new MicrosoftSQLTypeTranslater();
            var mysql = new MySqlTypeTranslater();
            var oracle = new OracleTypeTranslater();

            Assert.AreEqual("varchar(max)", ms.GetSQLDBTypeForCSharpType(cSharpType));
            Assert.AreEqual("text", mysql.GetSQLDBTypeForCSharpType(cSharpType));
            Assert.AreEqual("CLOB", oracle.GetSQLDBTypeForCSharpType(cSharpType));
        }

        [Test]
        public void Test_DbToCSharpType_String_Max()
        {
            var ms = new MicrosoftSQLTypeTranslater();
            var mysql = new MySqlTypeTranslater();
            var oracle = new OracleTypeTranslater();

            Assert.AreEqual(typeof(string),ms.GetCSharpTypeForSQLDBType("varchar(max)"));
            Assert.AreEqual(int.MaxValue,ms.GetLengthIfString("varchar(max)"));


            Assert.AreEqual(int.MaxValue, mysql.GetLengthIfString("text"));
            Assert.AreEqual(int.MaxValue, mysql.GetLengthIfString("text"));

            Assert.AreEqual(int.MaxValue, oracle.GetLengthIfString("CLOB"));
            Assert.AreEqual(int.MaxValue, oracle.GetLengthIfString("CLOB"));

        }

        [TestCase("bigint")]
        [TestCase("binary")]
        [TestCase("bit")]
        [TestCase("char")]
        [TestCase("date")]
        [TestCase("datetime")]
        [TestCase("datetime2")]
        [TestCase("datetimeoffset")]
        [TestCase("decimal")]
        [TestCase("varbinary(max)")]
        [TestCase("float")]
        [TestCase("image")]
        [TestCase("int")]
        [TestCase("money")]
        [TestCase("nchar")]
        [TestCase("ntext")]
        [TestCase("numeric")]
        [TestCase("nvarchar")]
        [TestCase("real")]
        [TestCase("rowversion")]
        [TestCase("smalldatetime")]
        [TestCase("smallint")]
        [TestCase("smallmoney")]
        [TestCase("text")]
        [TestCase("time")]
        [TestCase("timestamp")]
        [TestCase("tinyint")]
        [TestCase("uniqueidentifier")]
        [TestCase("varbinary")]
        [TestCase("varchar")]
        [TestCase("xml")]
        public void TestIsKnownType_Microsoft(string sqlType)
        {
            var tt = new MicrosoftSQLTypeTranslater();

            RunKnownTypeTest(tt, _msDb, sqlType.ToLower());
            RunKnownTypeTest(tt, _msDb, sqlType.ToUpper());
        }

        [TestCase("BOOL")]
        [TestCase("BOOLEAN")]
        [TestCase("TINYINT")]
        [TestCase("CHARACTER VARYING(10)")]
        [TestCase("FIXED")]
        [TestCase("DEC")]
        [TestCase("VARCHAR(10)")]
        [TestCase("DECIMAL")]
        [TestCase("FLOAT4")]
        [TestCase("FLOAT")]
        [TestCase("FLOAT8")]
        [TestCase("DOUBLE")]
        [TestCase("INT1")]
        [TestCase("INT2")]
        [TestCase("INT3")]
        [TestCase("INT4")]
        [TestCase("INT8")]
        [TestCase("SMALLINT")]
        [TestCase("MEDIUMINT")]
        [TestCase("INT")]
        [TestCase("BIGINT")]
        [TestCase("LONG VARBINARY")]
        [TestCase("MEDIUMBLOB")]
        [TestCase("LONG VARCHAR")]
        [TestCase("MEDIUMTEXT")]
        [TestCase("LONG")]
        [TestCase("MIDDLEINT")]
        [TestCase("NUMERIC")]
        [TestCase("INTEGER")]
        [TestCase("BIT")]
        [TestCase("SMALLINT(3)")]
        [TestCase("INT UNSIGNED")]
        [TestCase("INT UNSIGNED ZEROFILL")]
        [TestCase("SMALLINT UNSIGNED")]
        [TestCase("SMALLINT ZEROFILL UNSIGNED")]
        [TestCase("LONGTEXT")]
        [TestCase("CHAR(10)")]
        [TestCase("TEXT")]
        [TestCase("BLOB")]
        [TestCase("ENUM('fish','carrot')")]
        [TestCase("SET('fish','carrot')")]

        [TestCase("VARBINARY(10)")]

        [TestCase("date")]
        [TestCase("datetime")]
        [TestCase("TIMESTAMP")]
        [TestCase("TIME")]

        /* Not supported
        [TestCase("GEOMETRY")]
        [TestCase("POINT")]
        [TestCase("LINESTRING")]
        [TestCase("POLYGON")]
        [TestCase("MULTIPOINT")]
        [TestCase("MULTILINESTRING")]
        [TestCase("MULTIPOLYGON")]
        [TestCase("GEOMETRYCOLLECTION")]
        */


        [TestCase("nchar")]
        [TestCase("nvarchar(10)")]
        [TestCase("real")]
        public void TestIsKnownType_MySql(string sqlType)
        {
            var tt = new MySqlTypeTranslater();

            RunKnownTypeTest(tt, _mysqlDb, sqlType.ToLower());
            RunKnownTypeTest(tt, _mysqlDb, sqlType.ToUpper());
        }

        private void RunKnownTypeTest(TypeTranslater tt, DiscoveredDatabase db, string sqlType)
        {
            var tBefore = tt.GetCSharpTypeForSQLDBType(sqlType);

            Assert.IsNotNull(tBefore);


            if (db != null)
            {
                var tbl = db.CreateTable("TTT", new[] { new DatabaseColumnRequest("MyCol", sqlType) });

                try
                {
                    var datatypeComputer = tbl.DiscoverColumns().Single().GetDataTypeComputer();
                    Assert.IsNotNull(datatypeComputer.CurrentEstimate);
                    var tAfter = datatypeComputer.CurrentEstimate;

                    Assert.AreEqual(tBefore, tAfter);
                }
                finally
                {
                    tbl.Drop();
                }
            }
        }

        [TestCase("sql_variant")]
        public void TestNotSupportedTypes(string sqlType)
        {
            var tt = new MicrosoftSQLTypeTranslater();
            Assert.Throws<NotSupportedException>(() => tt.GetCSharpTypeForSQLDBType(sqlType));
        }
    }
}