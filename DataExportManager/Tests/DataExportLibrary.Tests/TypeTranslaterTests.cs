using System;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using ReusableLibraryCode.DatabaseHelpers.Discovery.MySql;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace DataExportLibrary.Tests
{
    public class TypeTranslaterTests
    {
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
        public void TestIsKnownType(string sqlType)
        {
            var tt = new MicrosoftSQLTypeTranslater();
            var t = tt.GetCSharpTypeForSQLDBType(sqlType);

            Assert.IsNotNull(t);
        }

        [TestCase("sql_variant")]
        public void TestNotSupportedTypes(string sqlType)
        {
            var tt = new MicrosoftSQLTypeTranslater();
            Assert.Throws<NotSupportedException>(() => tt.GetCSharpTypeForSQLDBType(sqlType));
        }
    }
}