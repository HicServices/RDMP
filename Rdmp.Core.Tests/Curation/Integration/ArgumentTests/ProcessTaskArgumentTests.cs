// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.ArgumentTests;

public class ProcessTaskArgumentTests : DatabaseTests
{
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TypeOfTableInfo(bool declareAsInterface)
    {
        var tableInfoName = $"TableInfoFor_{new StackTrace().GetFrame(0).GetMethod().Name}";

        var toCleanup = CatalogueRepository.GetAllObjects<TableInfo>()
            .SingleOrDefault(t => t.Name.Equals(tableInfoName));

        toCleanup?.DeleteInDatabase();

        var loadMetadata = new LoadMetadata(CatalogueRepository);

        try
        {
            var pt = new ProcessTask(CatalogueRepository, loadMetadata, LoadStage.AdjustStaging);
            var pta = new ProcessTaskArgument(CatalogueRepository, pt);

            pta.SetType(declareAsInterface ? typeof(ITableInfo) : typeof(TableInfo));

            var tableInfo = new TableInfo(CatalogueRepository, tableInfoName);
            try
            {
                pta.SetValue(tableInfo);
                pta.SaveToDatabase();

                var newInstanceOfPTA = CatalogueRepository.GetObjectByID<ProcessTaskArgument>(pta.ID);

                Assert.That(pta.Value, Is.EqualTo(newInstanceOfPTA.Value));

                var t1 = (TableInfo)pta.GetValueAsSystemType();
                var t2 = (TableInfo)newInstanceOfPTA.GetValueAsSystemType();

                Assert.That(t2.ID, Is.EqualTo(t1.ID));
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
        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        var tableInfoName = $"TableInfoFor_{methodName}";
        var preLoadDiscardedColumnName = $"PreLoadDiscardedColumnFor_{methodName}";

        var toCleanup = CatalogueRepository.GetAllObjects<TableInfo>()
            .SingleOrDefault(t => t.Name.Equals(tableInfoName));
        var toCleanupCol = CatalogueRepository.GetAllObjects<PreLoadDiscardedColumn>()
            .SingleOrDefault(c => c.RuntimeColumnName.Equals(preLoadDiscardedColumnName));

        //must delete pre load discarded first
        toCleanupCol?.DeleteInDatabase();

        toCleanup?.DeleteInDatabase();

        var lmd = new LoadMetadata(CatalogueRepository);

        try
        {
            var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustStaging);
            var pta = new ProcessTaskArgument(CatalogueRepository, pt);

            pta.SetType(typeof(PreLoadDiscardedColumn));

            var tableInfo = new TableInfo(CatalogueRepository, tableInfoName);

            var preloadDiscardedColumn =
                new PreLoadDiscardedColumn(CatalogueRepository, tableInfo, preLoadDiscardedColumnName);
            try
            {
                pta.SetValue(preloadDiscardedColumn);
                pta.SaveToDatabase();

                var newInstanceOfPTA = CatalogueRepository.GetObjectByID<ProcessTaskArgument>(pta.ID);
                Assert.That(pta.Value, Is.EqualTo(newInstanceOfPTA.Value));

                var p1 = (PreLoadDiscardedColumn)pta.GetValueAsSystemType();
                var p2 = (PreLoadDiscardedColumn)newInstanceOfPTA.GetValueAsSystemType();

                Assert.That(p2.ID, Is.EqualTo(p1.ID));
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
    public void TableInfoType_FetchAfterDelete_ReturnsNull()
    {
        var tableInfoName = $"TableInfoFor_{new StackTrace().GetFrame(0).GetMethod().Name}";

        var toCleanup = CatalogueRepository.GetAllObjects<TableInfo>()
            .SingleOrDefault(t => t.Name.Equals(tableInfoName));

        toCleanup?.DeleteInDatabase();

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

            //give the object back now please? - returns null because it's gone (new behaviour)
            Assert.That(pta.GetValueAsSystemType(), Is.Null);

            //old behaviour
            /*var ex = Assert.Throws<KeyNotFoundException>(()=>pta.GetValueAsSystemType());
            StringAssert.Contains("Could not find TableInfo with ID",ex.Message);*/
        }
        finally
        {
            lmd.DeleteInDatabase();
        }
    }

    [Test]
    public void LieToProcessTaskArgumentAboutWhatTypeIs_Throws()
    {
        var tableInfoName = $"TableInfoFor_{new StackTrace().GetFrame(0).GetMethod().Name}";

        var toCleanup = CatalogueRepository.GetAllObjects<TableInfo>()
            .SingleOrDefault(t => t.Name.Equals(tableInfoName));

        toCleanup?.DeleteInDatabase();

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
                var ex = Assert.Throws<Exception>(() => pta.SetValue(tableInfo));
                Assert.That(
                    ex.Message,
                    Does.Contain(
                        "has an incompatible Type specified (Rdmp.Core.Curation.Data.DataLoad.PreLoadDiscardedColumn)"));
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
            Assert.That(value, Is.Not.Null);
            Assert.That(value.GetDecryptedValue(), Is.EqualTo("test123"));
        }
        finally
        {
            if (pta != null)
            {
                var processTask = CatalogueRepository.GetObjectByID<ProcessTask>(pta.ProcessTask_ID);
                processTask.DeleteInDatabase();
            }

            lmd?.DeleteInDatabase();
        }
    }

    [Test]
    public void TestArgumentCreation()
    {
        var lmd = new LoadMetadata(CatalogueRepository, "TestArgumentCreation");
        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustRaw);
        pt.CreateArgumentsForClassIfNotExists<TestArgumentedClass>();
        try
        {
            var arg = pt.ProcessTaskArguments.Single();

            Assert.Multiple(() =>
            {
                Assert.That(arg.Name, Is.EqualTo("MyBool"));
                Assert.That(arg.Type, Is.EqualTo("System.Boolean"));
                Assert.That(arg.Description, Is.EqualTo("Fishes"));
                Assert.That(arg.Value, Is.EqualTo("True"));
                Assert.That(arg.GetValueAsSystemType(), Is.EqualTo(true));
            });
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
        var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(BasicDataReleaseDestination), -1,
            "Coconuts");
        pipe.DestinationPipelineComponent_ID = pc.ID;
        pipe.SaveToDatabase();

        //some of the DemandsInitialization on BasicDataReleaseDestination should be nested
        Assert.Multiple(() =>
        {
            Assert.That(
                ArgumentFactory.GetRequiredProperties(typeof(BasicDataReleaseDestination))
                    .Any(r => r.ParentPropertyInfo != null));

            //new pc should have no arguments
            Assert.That(pc.GetAllArguments(), Is.Empty);
        });

        //we create them (the root and nested ones!)
        var args = pc.CreateArgumentsForClassIfNotExists<BasicDataReleaseDestination>();

        //and get all arguments / create arguments for class should have handled that
        Assert.That(pc.GetAllArguments().Any());

        var match = args.Single(a => a.Name == "ReleaseSettings.DeleteFilesOnSuccess");
        match.SetValue(true);
        match.SaveToDatabase();

        ReleaseUseCase.DesignTime();

        var destInstance = DataFlowPipelineEngineFactory.CreateDestinationIfExists(pipe);

        Assert.That(((BasicDataReleaseDestination)destInstance).ReleaseSettings.DeleteFilesOnSuccess, Is.EqualTo(true));
    }


    [Test]
    public void TestArgumentWithTypeThatIsEnum()
    {
        var pipe = new Pipeline(CatalogueRepository, "p");
        var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(BasicDataReleaseDestination), -1,
            "c");

        var arg = new PipelineComponentArgument(CatalogueRepository, pc);

        try
        {
            arg.SetType(typeof(ExitCodeType));
            arg.SetValue(ExitCodeType.OperationNotRequired);

            //should have set Value string to the ID of the object
            Assert.That(arg.Value, Is.EqualTo(ExitCodeType.OperationNotRequired.ToString()));

            arg.SaveToDatabase();

            //but as system Type should return the server
            Assert.That(arg.GetValueAsSystemType(), Is.EqualTo(ExitCodeType.OperationNotRequired));
        }
        finally
        {
            pipe.DeleteInDatabase();
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TestArgumentWithTypeThatIsInterface(bool useInterfaceDeclaration)
    {
        var pipe = new Pipeline(CatalogueRepository, "p");
        var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(BasicDataReleaseDestination), -1,
            "c");

        var arg = new PipelineComponentArgument(CatalogueRepository, pc);

        var server = new ExternalDatabaseServer(CatalogueRepository, "fish", null);

        try
        {
            arg.SetType(useInterfaceDeclaration ? typeof(IExternalDatabaseServer) : typeof(ExternalDatabaseServer));

            arg.SetValue(server);

            //should have set Value string to the ID of the object
            Assert.That(server.ID.ToString(), Is.EqualTo(arg.Value));

            arg.SaveToDatabase();

            //but as system Type should return the server
            Assert.That(server, Is.EqualTo(arg.GetValueAsSystemType()));
        }
        finally
        {
            pipe.DeleteInDatabase();
            server.DeleteInDatabase();
        }
    }

    [Test]
    public void TestArgumentThatIsDictionary()
    {
        var pipe = new Pipeline(CatalogueRepository, "p");
        var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(BasicDataReleaseDestination), -1,
            "c");

        try
        {
            var arg = new PipelineComponentArgument(CatalogueRepository, pc)
            {
                Name = "MyNames"
            };
            arg.SetType(typeof(Dictionary<TableInfo, string>));
            arg.SaveToDatabase();

            Assert.That(arg.GetConcreteSystemType(), Is.EqualTo(typeof(Dictionary<TableInfo, string>)));

            var ti1 = new TableInfo(CatalogueRepository, "test1");
            var ti2 = new TableInfo(CatalogueRepository, "test2");

            var val = new Dictionary<TableInfo, string>
            {
                { ti1, "Fish" },
                { ti2, "Fish" }
            };

            arg.SetValue(val);

            arg.SaveToDatabase();

            var val2 = (Dictionary<TableInfo, string>)arg.GetValueAsSystemType();
            Assert.That(val2, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(val2[ti1], Is.EqualTo("Fish"));
                Assert.That(val2[ti2], Is.EqualTo("Fish"));
            });
        }
        finally
        {
            pipe.DeleteInDatabase();
        }
    }
}