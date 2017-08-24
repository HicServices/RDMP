using System;
using System.Collections.Generic;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;
using NUnit.Framework;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class RuntimeTaskFactoryTests : DatabaseTests
    {
        [Test]
        [TestCase("LoadModules.Generic.Web.WebFileDownloader")]
        [TestCase("LoadModules.Generic.FileOperations.ExcelToCsvConverter")]
        public void RuntimeTaskFactoryTest(string className)
        {

            var lmd = new LoadMetadata(CatalogueRepository);
            var task = new ProcessTask(CatalogueRepository, lmd,LoadStage.GetFiles);

            var f = new RuntimeTaskFactory(CatalogueRepository);

            task.Path = className;
            task.ProcessTaskType = ProcessTaskType.DataProvider;
            task.SaveToDatabase();

            try
            {
                var ex = Assert.Throws<Exception>(()=>f.Create(task, new StageArgs(LoadStage.AdjustRaw,true)));
                Assert.IsTrue(ex.InnerException.Message.Contains("marked with DemandsInitialization but no corresponding argument was provided in ArgumentCollection"));
            }
            finally 
            {
                task.DeleteInDatabase();
                lmd.DeleteInDatabase();
            }


        }
    }
}