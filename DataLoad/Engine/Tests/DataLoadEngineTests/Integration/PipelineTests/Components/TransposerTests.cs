using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using LoadModules.Generic.DataFlowOperations;
using NUnit.Framework;

namespace DataLoadEngineTests.Integration.PipelineTests.Components
{
    public class TransposerTests
    {
        DataTable dt = new DataTable();


        [TestFixtureSetUp]
        public void Setup()
        {
            dt.Columns.Add("recipe");
            dt.Columns.Add("Fishcakes");
            dt.Columns.Add("Chips");
            dt.Columns.Add("Gateau");
            dt.Rows.Add("protein", "20", "30", "40");
            dt.Rows.Add("fat", "11", "2", "33");
            dt.Rows.Add("carbohydrate", "55", "0", "5");
        }

        [Test]
        public void TransposerTest_ThrowOnDualBatches()
        {
            var transposer = new Transposer();
            transposer.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());
            var ex = Assert.Throws<NotSupportedException>(()=>transposer.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
            Assert.AreEqual("Error, we received multiple batches, Transposer only works when all the data arrives in a single DataTable",ex.Message);
        }

        [Test]
        public void TransposerTest_ThrowOnEmptyDataTable()
        {
            var transposer = new Transposer();
            var ex = Assert.Throws<NotSupportedException>(()=>transposer.ProcessPipelineData(new DataTable(), new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
            Assert.AreEqual("DataTable toProcess had 0 rows and 0 columns, thus it cannot be transposed", ex.Message);
        }


        [Test]
        public void TransposerTest_TableTransposed()
        {
            var transposer = new Transposer();
            var actual = transposer.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

            DataTable expectedResult = new DataTable();

            expectedResult.Columns.Add("recipe");
            expectedResult.Columns.Add("protein");
            expectedResult.Columns.Add("fat");
            expectedResult.Columns.Add("carbohydrate");

            expectedResult.Rows.Add("Fishcakes", "20", "11", "55");
            expectedResult.Rows.Add("Chips", "30", "2", "0");
            expectedResult.Rows.Add("Gateau", "40", "33", "5");

            for (int i = 0; i < actual.Columns.Count; i++)
                Assert.AreEqual(expectedResult.Columns[i].ColumnName, actual.Columns[i].ColumnName);

            for (int i = 0; i < expectedResult.Rows.Count; i++)
                for (int j = 0; j < actual.Columns.Count; j++)
                    Assert.AreEqual(expectedResult.Rows[i][j], actual.Rows[i][j]);

        }

        [Test]
        public void TestTransposerDodgyHeaders()
        {

            var dr = dt.Rows.Add("32 GramMax", "55", "0", "5");

            var transposer = new Transposer();
            transposer.MakeHeaderNamesSane = true;
            var actual = transposer.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());
            
            Assert.IsTrue(actual.Columns.Contains("_32GramMax"));

            dt.Rows.Remove(dr);
            
        }
    }

}
