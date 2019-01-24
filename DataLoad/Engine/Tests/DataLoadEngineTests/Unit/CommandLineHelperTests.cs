using System;
using System.Data.SqlClient;
using System.IO;
using DataLoadEngine.LoadExecution.Components.Arguments;
using FAnsi.Discovery;
using NUnit.Framework;

namespace DataLoadEngineTests.Unit
{
    class CommandLineHelperTests
    {
        [Test]
        public void TestGetValueString()
        {
            var date = new DateTime(2004, 1, 1);
            Assert.AreEqual("\"2004-01-01\"", CommandLineHelper.GetValueString(date));

            var fi = new FileInfo(@"C:\a\test\path");
            Assert.AreEqual(@"""C:\a\test\path""", CommandLineHelper.GetValueString(fi));

            const string db = "db-name";
            Assert.AreEqual(db, CommandLineHelper.GetValueString(db));

            //notice how server and db don't actually exist, thats cool they implement IMightNotExist
            var dbInfo = new DiscoveredServer(new SqlConnectionStringBuilder(){DataSource = "server"}).ExpectDatabase("db");
            Assert.AreEqual("--database-name=db --database-server=server", CommandLineHelper.CreateArgString("DbInfo", dbInfo));
        }

        [Test]
        public void TestGetValueStringError()
        {
            var obj = new CommandLineHelperTests();
            Assert.Throws<ArgumentException>(()=>CommandLineHelper.GetValueString(obj));
        }

        [Test]
        public void TestCreateArgString()
        {
            var date = new DateTime(2004, 1, 1);
            var argString = CommandLineHelper.CreateArgString("DateFrom", date);
            Assert.AreEqual("--date-from=\"2004-01-01\"", argString);
        }

        [Test]
        public void TestDateTimeCreateArgString()
        {
            var date = new DateTime(2004, 1, 1, 12, 34, 56);
            var argString = CommandLineHelper.CreateArgString("DateFrom", date);
            Assert.AreEqual("--date-from=\"2004-01-01 12:34:56\"", argString);
        }

        [Test]
        public void TestEmptyArgumentName()
        {
            Assert.Throws<ArgumentException>(()=>CommandLineHelper.CreateArgString("", "value"));
        }

        [Test]
        public void TestNameWithoutLeadingUppercaseCharacter()
        {
            Assert.Throws<ArgumentException>(()=>CommandLineHelper.CreateArgString("dateFrom", "2014-01-01"));
        }

        [Test]
        public void TestNullValue()
        {
            Assert.Throws<ArgumentException>(()=>CommandLineHelper.CreateArgString("DateFrom", null));
        }
    }
}
