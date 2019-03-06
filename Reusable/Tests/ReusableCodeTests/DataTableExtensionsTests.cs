using System.Data;
using System.IO;
using NUnit.Framework;
using ReusableLibraryCode.Extensions;

namespace ReusableCodeTests
{
    class DataTableExtensionsTests
    {
        [Test]
        public void TestEscaping_CommaInCell()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "out.csv");

            DataTable dt = new DataTable();
            dt.Columns.Add("Phrase");
            dt.Columns.Add("Car");

            dt.Rows.Add("omg,why me!", "Ferrari");

            dt.SaveAsCsv(path);

            var answer = File.ReadAllText(path);

            Assert.AreEqual(answer,
                @"Phrase,Car
""omg,why me!"",Ferrari
");

        }

        [Test]
        public void TestEscaping_CommaAndQuotesInCell()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory,"out.csv");

            DataTable dt = new DataTable();
            dt.Columns.Add("Phrase");
            dt.Columns.Add("Car");

            dt.Rows.Add("omg,\"why\" me!","Ferrari");

            dt.SaveAsCsv(path);

            var answer = File.ReadAllText(path);

            Assert.AreEqual(answer,
                @"Phrase,Car
""omg,""""why"""" me!"",Ferrari
");
        }


        [Test]
        public void TestEscaping_CommaAndQuotesInCell2()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "out.csv");

            DataTable dt = new DataTable();
            dt.Columns.Add("Phrase");
            dt.Columns.Add("Car");

            dt.Rows.Add("\"omg,why me!\"", "Ferrari");

            dt.SaveAsCsv(path);

            var answer = File.ReadAllText(path);

            Assert.AreEqual(answer,
                @"Phrase,Car
""""""omg,why me!"""""",Ferrari
");
        }
    }
}
