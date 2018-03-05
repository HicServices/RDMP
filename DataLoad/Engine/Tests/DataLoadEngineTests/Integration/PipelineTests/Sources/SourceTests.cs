using System;
using System.Data;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.DataFlowPipeline.Requirements.Exceptions;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DataFlowPipeline.Sources;
using DataLoadEngine.Job;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration.PipelineTests.Sources
{
    public class SourceTests:DatabaseTests
    {
        [Test]
        public void RetrieveChunks()
        {
            var source = new DbDataCommandDataFlowSource("Select top 3 * from master.sys.tables", "Query Sys tables", DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder, 30);
            Assert.AreEqual(3, source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()).Rows.Count);
        }


        [Test]
        public void TestPipelineContextInitialization()
        {
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.FixedDestination |PipelineUsage.LoadsSingleTableInfo);

            var component = new TestObject_RequiresTableInfo();
            var ti = new TableInfo(CatalogueRepository, "TestTableInfo");
            context.PreInitialize(new ThrowImmediatelyDataLoadEventListener(), component, ti);
            
            Assert.AreEqual(component.PreInitToThis, ti);
            ti.DeleteInDatabase();
        }

        [Test]
        public void TestPipelineContextInitializationNoInterfaces()
        {
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.FixedDestination | PipelineUsage.LoadsSingleTableInfo);
            var ti = new TableInfo(MockRepository.GenerateStub<ICatalogueRepository>(), "Foo");
            var component = new TestObjectNoRequirements();
            Assert.DoesNotThrow(() => context.PreInitialize(new ThrowImmediatelyDataLoadEventListener(), component, ti));
        }

        [Test]
        [ExpectedException(MatchType = MessageMatch.Contains, ExpectedMessage = "The following expected types were not passed to PreInitialize:TableInfo")]
        public void TestPipelineContextInitialization_UnexpectedType()
        {
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.FixedDestination | PipelineUsage.LoadsSingleTableInfo);

            var component = new TestObject_RequiresTableInfo();
            var ti = new TableInfo(MockRepository.GenerateStub<ICatalogueRepository>(), "Foo");
            var ci = new ColumnInfo(MockRepository.GenerateStub<ICatalogueRepository>(), "ColumnInfo", "Type", ti);
            ci.Name = "ColumnInfo"; // because we passed a stubbed repository, the name won't be set

            context.PreInitialize(new ThrowImmediatelyDataLoadEventListener(), component, ci);

            Assert.AreEqual(component.PreInitToThis, ti);
        }

        [Test]
        [ExpectedException(MatchType = MessageMatch.Contains, ExpectedMessage = "Type TableInfo is not an allowable PreInitialize parameters type under the current DataFlowPipelineContext (check which flags you passed to the DataFlowPipelineContextFactory and the interfaces IPipelineRequirement<> that your components implement) ")]
        public void TestPipelineContextInitialization_ForbiddenType()
        {
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.None);

            var component = new TestObject_RequiresTableInfo();
            var ti = new TableInfo(MockRepository.GenerateStub<ICatalogueRepository>(), "Foo");
            context.PreInitialize(new ThrowImmediatelyDataLoadEventListener(), component, ti);

            Assert.AreEqual(component.PreInitToThis, ti);
        }

        [Test]
        public void TestPipelineContextInitialization_UninitializedInterface()
        {
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.FixedDestination | PipelineUsage.LoadsSingleTableInfo);

            //component is both IPipelineRequirement<TableInfo> AND IPipelineRequirement<LoadModuleAssembly> but only TableInfo is passed in params
            var component = new TestObject_RequiresTableInfoAndFreakyObject();

            var testTableInfo = new TableInfo(MockRepository.GenerateStub<ICatalogueRepository>(), "");
            testTableInfo.Name = "Test Table Info";

            var ex = Assert.Throws<Exception>(()=>context.PreInitialize(new ThrowImmediatelyDataLoadEventListener(), component, testTableInfo));
            Assert.IsTrue(ex.Message.Contains(
                "The following expected types were not passed to PreInitialize:LoadModuleAssembly\r\nThe object types passed were:\r\nCatalogueLibrary.Data.TableInfo:Test Table Info"));
        }

        [Test]
        public void TestPipelineContextIsAllowable()
        {
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.FixedSource | PipelineUsage.FixedDestination | PipelineUsage.LoadsSingleTableInfo);

            var pipeline = new CatalogueLibrary.Data.Pipelines.Pipeline(CatalogueRepository, "DeleteMePipeline");
            var component = new PipelineComponent(CatalogueRepository, pipeline, typeof(TestObject_RequiresTableInfo), 0);

            Assert.IsTrue(context.IsAllowable(pipeline));

            pipeline.DeleteInDatabase();
        }


        [Test]
        public void TestPipelineContextIsNOTAllowable()
        {
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.FixedDestination);

            var pipeline = new CatalogueLibrary.Data.Pipelines.Pipeline(CatalogueRepository, "DeleteMePipeline");
            var component = new PipelineComponent(CatalogueRepository, pipeline, typeof(TestObject_RequiresTableInfo), 0);
            component.Name = "TestPipeComponent";
            component.SaveToDatabase();

            string reason;
            bool rejection = context.IsAllowable(pipeline, out reason);

            Console.WriteLine(reason);

            Assert.IsFalse(rejection,reason);

            Assert.AreEqual("Component TestPipeComponent implements a forbidden type (IPipelineRequirement<TableInfo>) under the pipeline usage context",reason);

            pipeline.DeleteInDatabase();
        }

        [Test]
        public void TestSuspiciousPipelineRequirements()
        {
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.FixedDestination);
            
            var suspiciousComponent = new TestObject_Suspicious();
            var ex = Assert.Throws<MultipleMatchingImplmentationException>(() => context.PreInitialize(new ThrowImmediatelyDataLoadJob(), suspiciousComponent, 5, "fish"));

            Console.WriteLine("Exception was:"  + ex.Message);
        }
        [Test]
        public void TestExtraSuspiciousPipelineRequirements()
        {
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.FixedDestination);

            var suspiciousComponent = new TestObject_ExtraSuspicious();
            Assert.Throws<OverlappingImplementationsException>(() => context.PreInitialize(new ThrowImmediatelyDataLoadJob(), suspiciousComponent, "5"));
        }

        #region Test objects that have an assortment of IPipelineRequirement<T>s

        public class TestObject_RequiresTableInfo : IDataFlowComponent<DataTable>, IPipelineRequirement<TableInfo>
        {
            public TableInfo PreInitToThis { get; private set; }
            public DataTable ProcessPipelineData( DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
            {
                throw new NotImplementedException();
            }

            public void Abort(IDataLoadEventListener listener)
            {
                throw new NotImplementedException();
            }

            public void PreInitialize(TableInfo value, IDataLoadEventListener listener)
            {
                PreInitToThis = value;
            }

        }
        public class TestObject_RequiresTableInfoAndFreakyObject : IDataFlowComponent<DataTable>, IPipelineRequirement<TableInfo>, IPipelineRequirement<LoadModuleAssembly>
        {
            public TableInfo PreInitToThis { get; private set; }
            public DataTable ProcessPipelineData( DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
            
            public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
            {
                throw new NotImplementedException();
            }

            public void Abort(IDataLoadEventListener listener)
            {
                throw new NotImplementedException();
            }

            public void PreInitialize(TableInfo value, IDataLoadEventListener listener)
            {
                PreInitToThis = value;
            }


            public void PreInitialize(LoadModuleAssembly value, IDataLoadEventListener listener)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class TestObjectNoRequirements : IDataFlowComponent<DataTable>
    {
        public DataTable ProcessPipelineData( DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            throw new NotImplementedException();
        }

        public void Abort(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }
    }

    public class TestObject_Suspicious : IDataFlowComponent<DataTable>, IPipelineRequirement<object>
    {
        public Object Object { get; set; }
        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
            GracefulCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            throw new NotImplementedException();
        }

        public void Abort(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        public void PreInitialize(object value, IDataLoadEventListener listener)
        {
            Object = value;
        }
    }

    public class TestObject_ExtraSuspicious : IDataFlowComponent<DataTable>, IPipelineRequirement<object>, IPipelineRequirement<string>
    {
        public Object Object { get; set; }
        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
            GracefulCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            throw new NotImplementedException();
        }

        public void Abort(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        public void PreInitialize(object value, IDataLoadEventListener listener)
        {
            Object = value;
        }

        public void PreInitialize(string value, IDataLoadEventListener listener)
        {
            Object = value;
        }
    }
    #endregion
}
