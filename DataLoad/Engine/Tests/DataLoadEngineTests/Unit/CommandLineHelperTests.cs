using System;
using System.Data.SqlClient;
using System.IO;
using DataLoadEngine.LoadExecution.Components.Arguments;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Rhino.Mocks;

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
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetValueStringError()
        {
            var obj = new CommandLineHelperTests();
            CommandLineHelper.GetValueString(obj);
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
        [ExpectedException(ExpectedException = typeof (ArgumentException))]
        public void TestEmptyArgumentName()
        {
            CommandLineHelper.CreateArgString("", "value");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public void TestNameWithoutLeadingUppercaseCharacter()
        {
            CommandLineHelper.CreateArgString("dateFrom", "2014-01-01");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentException))]
        public void TestNullValue()
        {
            CommandLineHelper.CreateArgString("DateFrom", null);
        }
    }
}
