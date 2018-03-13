using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using NUnit.Framework;
using RDMPAutomationService;
using RDMPAutomationServiceTests.AutomationLoopTests.FictionalAutomationPlugin;
using RDMPAutomationServiceTests.AutomationLoopTests.FictionalCache;
using Tests.Common;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class EndToEndPluginTest:AutomationTests
    {
        private AutomationServiceSlot _slot;
        private AutomateablePipeline _automatablePipeline;
        private PipelineComponent _source;
        private Pipeline _pipeline;

        
        [SetUp]
        public void SetupDatabaseObjects()
        {
            _slot = new AutomationServiceSlot(CatalogueRepository);

            _automatablePipeline = new AutomateablePipeline(CatalogueRepository, _slot);
            _pipeline = _automatablePipeline.Pipeline;
            _source = new PipelineComponent(CatalogueRepository, _pipeline, typeof(TestAutomationPluginSource), 0, "TestAutomationPluginSource");
            
            var arguments = _source.CreateArgumentsForClassIfNotExists<TestAutomationPluginSource>().ToArray();

            Assert.AreEqual("NumberOfSecondsToWaitBetweenCreatingSkullImages",arguments[0].Name);
            arguments[0].SetValue(1);
            arguments[0].SaveToDatabase();

            Assert.AreEqual("MaxNumberOfSimultaneousSkullImageJobs", arguments[1].Name);
            arguments[1].SetValue(5);
            arguments[1].SaveToDatabase();

            _pipeline.SourcePipelineComponent_ID = _source.ID;
            _pipeline.SaveToDatabase();
        }


        [Test]
        public void TestEndToEndPlugin()
        {
            RDMPAutomationLoop loop = new RDMPAutomationLoop(mockOptions, logAction);
            loop.Start();

            Task.Delay(10000).Wait();

            loop.Stop = true;
            
            loop.AutomationDestination.WaitTillStoppable(30000);
            
            var dir = new DirectoryInfo(Environment.CurrentDirectory);
            
            int atEnd = dir.EnumerateFiles("*_skull.png").Count();
            Assert.GreaterOrEqual(atEnd,2);

            Task.Delay(3000).Wait();

            Task.Delay(2000).Wait();

            //no new ones should be appearing
            Assert.AreEqual(atEnd, dir.EnumerateFiles("*_skull.png").Count());

            
            var exceptions = CatalogueRepository.GetAllObjects<AutomationServiceException>();
            var jobs = _slot.AutomationJobs;
            
            Assert.AreEqual(0,exceptions.Count(),"Encountered the following exceptions during execution:" + string.Join(Environment.NewLine,exceptions.Select(t=>t.ToString())));
            Assert.AreEqual(0, jobs.Count());
        }

        [TearDown]
        public void TearDownDatabaseObjects()
        {

            var pipe = _automatablePipeline.Pipeline;
            _automatablePipeline.DeleteInDatabase();
            pipe.DeleteInDatabase();

            _slot.Unlock();
            _slot.DeleteInDatabase();

        }
    }
}
