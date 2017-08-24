using System.Collections.Generic;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Spontaneous;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;
using NUnit.Framework;
using Rhino.Mocks;

namespace DataLoadEngineTests.Unit
{
    class ExecutableProcessTaskTests
    {
        [Test]
        public void TestCreateArgString()
        {
            const string db = "my-db";

            var customArgs = new List<SpontaneouslyInventedArgument>();
            customArgs.Add(new SpontaneouslyInventedArgument("DatabaseName", db));

            var processTask = MockRepository.GenerateStub<IProcessTask>();
            var task = new ExecutableRuntimeTask(processTask, new RuntimeArgumentCollection(customArgs.ToArray(), null));
            
            var argString = task.CreateArgString();
            var expectedArgString = "--database-name=" + db;

            Assert.AreEqual(expectedArgString, argString);
        }

        [Test]
        public void TestConstructionFromProcessTask()
        {
            var processTask = MockRepository.GenerateStub<IProcessTask>();
            processTask.Stub(pt => pt.Path).Return("path");

            var runtimeTask = new ExecutableRuntimeTask(processTask, null);
            Assert.AreEqual("path", runtimeTask.ExeFilepath);
        }
    }
}
