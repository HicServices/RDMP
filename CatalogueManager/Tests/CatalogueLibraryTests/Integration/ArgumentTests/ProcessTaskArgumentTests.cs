using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.DataRelease;
using DataExportLibrary.DataRelease.ReleasePipeline;
using NUnit.Framework;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.ArgumentTests
{
    public class ProcessTaskArgumentTests:DatabaseTests
    {
        [Test]
        public void TypeOfTableInfo()
        {
            string tableInfoName = "TableInfoFor_" + new StackTrace().GetFrame(0).GetMethod().Name;

            TableInfo toCleanup = CatalogueRepository.GetAllObjects<TableInfo>().SingleOrDefault(t => t.Name.Equals(tableInfoName));
            
            if(toCleanup != null)
                toCleanup.DeleteInDatabase();

            var loadMetadata = new LoadMetadata(CatalogueRepository);

            try
            { 
                var pt = new ProcessTask(CatalogueRepository, loadMetadata, LoadStage.AdjustStaging);
                var pta = new ProcessTaskArgument(CatalogueRepository, pt);

                pta.SetType(typeof (TableInfo));
                var tableInfo = new TableInfo(CatalogueRepository, tableInfoName);
                try
                {
                    pta.SetValue(tableInfo);
                    pta.SaveToDatabase();

                    var newInstanceOfPTA = CatalogueRepository.GetObjectByID<ProcessTaskArgument>(pta.ID);

                    Assert.AreEqual(newInstanceOfPTA.Value,pta.Value);

                    TableInfo t1 = (TableInfo) pta.GetValueAsSystemType();
                    TableInfo t2 = (TableInfo)newInstanceOfPTA.GetValueAsSystemType();

                    Assert.AreEqual(t1.ID,t2.ID);
                }
                finally
                {
                    tableInfo.DeleteInDatabase();
                }
            }
            finally
            {
                loadMetadata.DeleteInDatabase();
            }
        }
        [Test]
        public void TypeOfPreLoadDiscardedColumn()
        {
            string methodName = new StackTrace().GetFrame(0).GetMethod().Name;
            string tableInfoName = "TableInfoFor_" + methodName;
            string preLoadDiscardedColumnName = "PreLoadDiscardedColumnFor_" + methodName; 

            TableInfo toCleanup = CatalogueRepository.GetAllObjects<TableInfo>().SingleOrDefault(t => t.Name.Equals(tableInfoName));
            PreLoadDiscardedColumn toCleanupCol = CatalogueRepository.GetAllObjects<PreLoadDiscardedColumn>()
                    .SingleOrDefault(c => c.RuntimeColumnName.Equals(preLoadDiscardedColumnName));
            
            //must delete pre load discarded first
            if (toCleanupCol != null)
                toCleanupCol.DeleteInDatabase();

            if (toCleanup != null)
                toCleanup.DeleteInDatabase();

            var lmd = new LoadMetadata(CatalogueRepository);

            try
            {
                var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustStaging);
                var pta = new ProcessTaskArgument(CatalogueRepository, pt);

                pta.SetType(typeof(PreLoadDiscardedColumn));

                var tableInfo = new TableInfo(CatalogueRepository, tableInfoName);

                PreLoadDiscardedColumn preloadDiscardedColumn = new PreLoadDiscardedColumn(CatalogueRepository, tableInfo, preLoadDiscardedColumnName);
                try
                {
                    pta.SetValue(preloadDiscardedColumn);
                    pta.SaveToDatabase();

                    var newInstanceOfPTA = CatalogueRepository.GetObjectByID<ProcessTaskArgument>(pta.ID);
                    Assert.AreEqual(newInstanceOfPTA.Value, pta.Value);

                    PreLoadDiscardedColumn p1 = (PreLoadDiscardedColumn)pta.GetValueAsSystemType();
                    PreLoadDiscardedColumn p2 = (PreLoadDiscardedColumn)newInstanceOfPTA.GetValueAsSystemType();

                    Assert.AreEqual(p1.ID, p2.ID);
                }
                finally
                {
                    preloadDiscardedColumn.DeleteInDatabase();
                    tableInfo.DeleteInDatabase();
                }
            }
            finally
            {
                lmd.DeleteInDatabase();
            }
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException),ExpectedMessage = "Could not find TableInfo with ID",MatchType = MessageMatch.Contains)]
        public void TableInfoType_FetchAfterDelete_Throws()
        {
            string tableInfoName = "TableInfoFor_" + new StackTrace().GetFrame(0).GetMethod().Name;

            TableInfo toCleanup = CatalogueRepository.GetAllObjects<TableInfo>().SingleOrDefault(t => t.Name.Equals(tableInfoName));

            if (toCleanup != null)
                toCleanup.DeleteInDatabase();

            var lmd = new LoadMetadata(CatalogueRepository);

            try
            {
                var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustStaging);
                var pta = new ProcessTaskArgument(CatalogueRepository, pt);

                //Prepare to receive a TableInfo object
                pta.SetType(typeof(TableInfo));

                var tableInfo = new TableInfo(CatalogueRepository, tableInfoName);
              
                //Heres the TableInfo object
                pta.SetValue(tableInfo);
                pta.SaveToDatabase();

                //Lolz I just deleted it out of the database
                tableInfo.DeleteInDatabase();

                //give the object back now please? - fails here because it's gone - like a rabit in a hat!
                pta.GetValueAsSystemType();

                Assert.Fail("Should have thrown already");
              
            }
            finally
            {
                lmd.DeleteInDatabase();
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "has an incompatible Type specified (CatalogueLibrary.Data.DataLoad.PreLoadDiscardedColumn)", MatchType = MessageMatch.Contains)]
        public void LieToProcessTaskArgumentAboutWhatTypeIs_Throws()
        {
            string tableInfoName = "TableInfoFor_" + new StackTrace().GetFrame(0).GetMethod().Name;

            TableInfo toCleanup = CatalogueRepository.GetAllObjects<TableInfo>().SingleOrDefault(t => t.Name.Equals(tableInfoName));

            if (toCleanup != null)
                toCleanup.DeleteInDatabase();

            var lmd = new LoadMetadata(CatalogueRepository);

            try
            {
                var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustStaging);
                var pta = new ProcessTaskArgument(CatalogueRepository, pt);
                var tableInfo = new TableInfo(CatalogueRepository, tableInfoName);
                try
                {
                    //tell it that we are going to give it a PreLoadDiscardedColumn
                    pta.SetType(typeof(PreLoadDiscardedColumn));
                    //then surprise! heres a TableInfo!
                    pta.SetValue(tableInfo);

                    Assert.Fail("Should have thrown already");
                }
                finally
                {
                    tableInfo.DeleteInDatabase();
                }
            }
            finally
            {
                
                lmd.DeleteInDatabase();
            }
        }

        private ProcessTaskArgument CreateNewProcessTaskArgumentInDatabase(out LoadMetadata lmd)
        {
            lmd = new LoadMetadata(CatalogueRepository);

            var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustStaging);
            return new ProcessTaskArgument(CatalogueRepository, pt);
        }

        [Test]
        public void TestEncryptedPasswordHostArgumentType()
        {
            LoadMetadata lmd = null;
            ProcessTaskArgument pta = null;

            try
            {
                pta = CreateNewProcessTaskArgumentInDatabase(out lmd);
                pta.SetType(typeof(EncryptedString));
                pta.SetValue(new EncryptedString(CatalogueRepository) { Value = "test123" });
                pta.SaveToDatabase();

                var loadedPta = CatalogueRepository.GetObjectByID<ProcessTaskArgument>(pta.ID);
                var value = loadedPta.GetValueAsSystemType() as EncryptedString;
                Assert.NotNull(value);
                Assert.AreEqual("test123", value.GetDecryptedValue());
            }
            finally
            {
                if (pta != null)
                {
                    var processTask = CatalogueRepository.GetObjectByID<ProcessTask>(pta.ProcessTask_ID);
                    processTask.DeleteInDatabase();
                }

                if (lmd != null)
                    lmd.DeleteInDatabase();
            }
        }

        [Test]
        public void TestArgumentCreation()
        {

            LoadMetadata lmd = new LoadMetadata(CatalogueRepository,"TestArgumentCreation");
            var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustRaw);
            pt.CreateArgumentsForClassIfNotExists<TestArgumentedClass>();
            try
            {
                var arg = pt.ProcessTaskArguments.Single();

                Assert.AreEqual("MyBool", arg.Name);
                Assert.AreEqual("System.Boolean", arg.Type);
                Assert.AreEqual("Fishes", arg.Description);
                Assert.AreEqual("True",arg.Value);
                Assert.AreEqual(true, arg.GetValueAsSystemType());

            }
            finally
            {
                pt.DeleteInDatabase();
                lmd.DeleteInDatabase();
            }
        }

        [Test]
        public void TestNestedDemandsGetPutIntoDatabaseAndCanBeBroughtBack()
        {
            var pipe = new Pipeline(CatalogueRepository, "NestedPipe");
            var pc = new PipelineComponent(CatalogueRepository, pipe, typeof (BasicDataReleaseDestination), -1,
                "Coconuts");
            pipe.DestinationPipelineComponent_ID = pc.ID;
            pipe.SaveToDatabase();

            //some of the DemandsInitialization on BasicDataReleaseDestination should be nested
            var f = new ArgumentFactory();
            Assert.True(
                f.GetRequiredProperties(typeof(BasicDataReleaseDestination)).Any(r => r.ParentPropertyInfo != null));

            //new pc should have no arguments
            Assert.That(pc.GetAllArguments(), Is.Empty);

            //we create them (the root and nested ones!)
            var args = pc.CreateArgumentsForClassIfNotExists<BasicDataReleaseDestination>();
            
            //and get all arguments / create arguments for class should have handled that 
            Assert.That(pc.GetAllArguments().Count(), Is.GreaterThan(1));

            var match = args.Single(a => a.Name == "ReleaseSettings.CustomReleaseFolder");
            match.Value = "coconuts";
            match.SaveToDatabase();

            var context = new ReleaseUseCase(null, null).GetContext();

            var factory = new DataFlowPipelineEngineFactory<ReleaseData>(RepositoryLocator.CatalogueRepository.MEF, (DataFlowPipelineContext<ReleaseData>) context);
            var destInstance = factory.CreateDestinationIfExists(pipe);

            Assert.AreEqual("coconuts", ((BasicDataReleaseDestination)destInstance).ReleaseSettings.CustomReleaseFolder.Name);
            
        }
    }
}
