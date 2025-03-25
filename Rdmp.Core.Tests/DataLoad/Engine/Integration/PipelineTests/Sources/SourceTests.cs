// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataFlowPipeline.Requirements.Exceptions;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Pipeline.Sources;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Sources;

public class SourceTests : DatabaseTests
{
    private readonly ICatalogueRepository mockRepo = new MemoryCatalogueRepository();

    [Test]
    public void RetrieveChunks()
    {
        var source = new DbDataCommandDataFlowSource("Select top 3 * from master.sys.tables", "Query Sys tables",
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder, 30);
        Assert.That(source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()).Rows,
            Has.Count.EqualTo(3));
    }


    [Test]
    public void TestPipelineContextInitialization()
    {
        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.FixedDestination | PipelineUsage.LoadsSingleTableInfo);

        var component = new TestObject_RequiresTableInfo();
        var ti = new TableInfo(CatalogueRepository, "TestTableInfo");
        context.PreInitialize(ThrowImmediatelyDataLoadEventListener.Quiet, component, ti);

        Assert.That(ti, Is.EqualTo(component.PreInitToThis));
        ti.DeleteInDatabase();
    }

    [Test]
    public void TestPipelineContextInitializationNoInterfaces()
    {
        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.FixedDestination | PipelineUsage.LoadsSingleTableInfo);
        var ti = new TableInfo(mockRepo, "Foo");
        var component = new TestObjectNoRequirements();
        Assert.DoesNotThrow(() => context.PreInitialize(ThrowImmediatelyDataLoadEventListener.Quiet, component, ti));
    }

    [Test]
    public void TestPipelineContextInitialization_UnexpectedType()
    {
        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.FixedDestination | PipelineUsage.LoadsSingleTableInfo);

        var component = new TestObject_RequiresTableInfo();
        var ti = new TableInfo(mockRepo, "Foo");
        var ci = new ColumnInfo(mockRepo, "ColumnInfo", "Type", ti)
        {
            Name = "ColumnInfo" // because we passed a stubbed repository, the name won't be set
        };

        var ex = Assert.Throws<Exception>(() =>
            context.PreInitialize(ThrowImmediatelyDataLoadEventListener.Quiet, component, ci));
        Assert.That(ex.Message,
            Does.Contain("The following expected types were not passed to PreInitialize:TableInfo"));
    }

    [Test]
    public void TestPipelineContextInitialization_ForbiddenType()
    {
        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.None);

        var component = new TestObject_RequiresTableInfo();
        var ti = new TableInfo(new MemoryCatalogueRepository(), "Foo");
        var ex = Assert.Throws<Exception>(() =>
            context.PreInitialize(ThrowImmediatelyDataLoadEventListener.Quiet, component, ti));
        Assert.That(
            ex.Message,
            Does.Contain(
                "Type TableInfo is not an allowable PreInitialize parameters type under the current DataFlowPipelineContext (check which flags you passed to the DataFlowPipelineContextFactory and the interfaces IPipelineRequirement<> that your components implement) "));
    }

    [Test]
    public void TestPipelineContextInitialization_UninitializedInterface()
    {
        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.FixedDestination | PipelineUsage.LoadsSingleTableInfo);

        //component is both IPipelineRequirement<TableInfo> AND IPipelineRequirement<LoadModuleAssembly> but only TableInfo is passed in params
        var component = new TestObject_RequiresTableInfoAndFreakyObject();

        var testTableInfo = new TableInfo(mockRepo, "")
        {
            Name = "Test Table Info"
        };

        var ex = Assert.Throws<Exception>(() =>
            context.PreInitialize(ThrowImmediatelyDataLoadEventListener.Quiet, component, testTableInfo));
        Assert.That(
            ex.Message,
            Does.Contain(
                $"The following expected types were not passed to PreInitialize:LoadModuleAssembly{Environment.NewLine}The object types passed were:{Environment.NewLine}Rdmp.Core.Curation.Data.TableInfo:Test Table Info"));
    }

    [Test]
    public void TestPipelineContextIsAllowable()
    {
        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.FixedSource | PipelineUsage.FixedDestination |
                                            PipelineUsage.LoadsSingleTableInfo);

        var pipeline = new Pipeline(CatalogueRepository, "DeleteMePipeline");

        Assert.That(context.IsAllowable(pipeline));

        pipeline.DeleteInDatabase();
    }


    [Test]
    public void TestPipelineContextIsNOTAllowable()
    {
        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.FixedDestination);

        var pipeline = new Pipeline(CatalogueRepository, "DeleteMePipeline");
        var component = new PipelineComponent(CatalogueRepository, pipeline, typeof(TestObject_RequiresTableInfo), 0)
        {
            Name = "TestPipeComponent"
        };
        component.SaveToDatabase();

        var rejection = context.IsAllowable(pipeline, out var reason);

        Console.WriteLine(reason);

        Assert.Multiple(() =>
        {
            Assert.That(rejection, Is.False, reason);

            Assert.That(
                reason,
                Is.EqualTo(
                    "Component TestPipeComponent implements a forbidden type (IPipelineRequirement<TableInfo>) under the pipeline usage context"));
        });

        pipeline.DeleteInDatabase();
    }

    [Test]
    public void TestSuspiciousPipelineRequirements()
    {
        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.FixedDestination);

        var suspiciousComponent = new TestObject_Suspicious();
        var ex = Assert.Throws<MultipleMatchingImplementationException>(() =>
            context.PreInitialize(new ThrowImmediatelyDataLoadJob(), suspiciousComponent, 5, "fish"));

        Console.WriteLine($"Exception was:{ex.Message}");
    }

    [Test]
    public void TestExtraSuspiciousPipelineRequirements()
    {
        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.FixedDestination);

        var suspiciousComponent = new TestObject_ExtraSuspicious();
        Assert.Throws<OverlappingImplementationsException>(() =>
            context.PreInitialize(new ThrowImmediatelyDataLoadJob(), suspiciousComponent, "5"));
    }

    #region Test objects that have an assortment of IPipelineRequirement<T>s

    public class TestObject_RequiresTableInfo : IDataFlowComponent<DataTable>, IPipelineRequirement<TableInfo>
    {
        public TableInfo PreInitToThis { get; private set; }

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

        public void PreInitialize(TableInfo value, IDataLoadEventListener listener)
        {
            PreInitToThis = value;
        }
    }

    public class TestObject_RequiresTableInfoAndFreakyObject : IDataFlowComponent<DataTable>,
        IPipelineRequirement<TableInfo>, IPipelineRequirement<LoadModuleAssembly>
    {
        public TableInfo PreInitToThis { get; private set; }

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
}

public class TestObject_Suspicious : IDataFlowComponent<DataTable>, IPipelineRequirement<object>
{
    public object Object { get; set; }

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

public class TestObject_ExtraSuspicious : IDataFlowComponent<DataTable>, IPipelineRequirement<object>,
    IPipelineRequirement<string>
{
    public object Object { get; set; }

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