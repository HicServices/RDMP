using System.Linq;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;
using NUnit.Framework;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    class ExecutableProcessTaskTests : DatabaseTests
    {
        [Test]
        public void TestConstructionFromProcessTaskUsingDatabase()
        {
            const string expectedPath = @"\\a\fake\path.exe";
            
            var loadMetadata = new LoadMetadata(CatalogueRepository);
            var processTask = new ProcessTask(CatalogueRepository, loadMetadata, LoadStage.Mounting)
            {
                Name = "Test process task",
                Path = expectedPath
            };
            processTask.SaveToDatabase();

            var argument = new ProcessTaskArgument(CatalogueRepository, processTask)
            {
                Name = "DatabaseName",
                Value = @"Foo_STAGING"
            };
            argument.SaveToDatabase();

            try
            {
                var args =
                    new RuntimeArgumentCollection(processTask.ProcessTaskArguments.Cast<IArgument>().ToArray(), null);

                var runtimeTask = new ExecutableRuntimeTask(processTask, args);
                Assert.AreEqual(expectedPath, runtimeTask.ExeFilepath);
                
                Assert.AreEqual(1, runtimeTask.RuntimeArguments.GetAllArgumentsOfType<string>().Count());

                var dictionaryOfStringArguments = runtimeTask.RuntimeArguments.GetAllArgumentsOfType<string>().ToDictionary(kvp => kvp.Key, kvp => kvp.Value); ;
                Assert.IsNotNull(dictionaryOfStringArguments["DatabaseName"]);
                Assert.AreEqual("Foo_STAGING", dictionaryOfStringArguments["DatabaseName"]);
            }
            finally
            {
                loadMetadata.DeleteInDatabase();
            }
        }
    }
}
