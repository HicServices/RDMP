using System;
using System.Data;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace DataLoadEngineTests.Integration.CrossDatabaseTypeTests
{
    class HowDoWeAchieveMd5Test:DatabaseTests
    {

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void TestMd5String(DatabaseType type)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("F");
            dt.Rows.Add(new[] {"Fish"});

            var db = GetCleanedServer(type);
            var tbl = db.CreateTable("MD5Test", dt);

            var col = tbl.DiscoverColumn("F");

            var sql = "SELECT " + tbl.GetQuerySyntaxHelper().HowDoWeAchieveMd5(col.GetFullyQualifiedName()) + " FROM " + tbl.GetFullyQualifiedName();


            using (var con = db.Server.GetConnection())
            {
                con.Open();
                var cmd = db.Server.GetCommand(sql, con);
                var value = cmd.ExecuteScalar();


                Console.WriteLine("Value was:" + value);

                Assert.IsNotNull(value);
                Assert.AreNotEqual("Fish",value);
                Assert.GreaterOrEqual(value.ToString().Length,32);
                
            }

        }

        
    }
}
