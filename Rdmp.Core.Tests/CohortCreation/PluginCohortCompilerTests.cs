﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Tests.CohortCreation.QueryTests;
using System;
using System.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.CohortCreation;

public class PluginCohortCompilerTests : CohortQueryBuilderWithCacheTests
{
    [Test]
    public void TestIPluginCohortCompiler_PopulatesCacheCorrectly()
    {
        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
            { DisallowInput = true };

        // create a cohort config
        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic")
        {
            QueryCachingServer_ID = externalDatabaseServer.ID
        };
        cic.SaveToDatabase();

        // this special Catalogue will be detected by ExamplePluginCohortCompiler and interpreted as an API call
        var myApi = new Catalogue(CatalogueRepository, ExamplePluginCohortCompiler.ExampleAPIName);

        // add it to the cohort config
        cic.CreateRootContainerIfNotExists();

        // create a use of the API as an AggregateConfiguration
        var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(activator,
            new CatalogueCombineable(myApi), cic.RootCohortAggregateContainer);

        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);
        cmd.Execute();

        // run the cic
        var source = new CohortIdentificationConfigurationSource();
        source.PreInitialize(cic, ThrowImmediatelyDataLoadEventListener.Quiet);
        var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        // 5 random chi numbers
        Assert.That(dt.Rows, Has.Count.EqualTo(5));

        // test stale
        cmd.AggregateCreatedIfAny.Description = "2";
        cmd.AggregateCreatedIfAny.SaveToDatabase();

        // run the cic again
        source = new CohortIdentificationConfigurationSource();
        source.PreInitialize(cic, ThrowImmediatelyDataLoadEventListener.Quiet);
        dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        // because the rules changed to generate 2 chis only there should be a new result
        Assert.That(dt.Rows, Has.Count.EqualTo(2));

        var results = new[] { (string)dt.Rows[0][0], (string)dt.Rows[1][0] };

        // run the cic again with no changes, the results should be unchanged since there is no config changed
        // I.e. no new chis should be generated and the cached values returned
        source = new CohortIdentificationConfigurationSource();
        source.PreInitialize(cic, ThrowImmediatelyDataLoadEventListener.Quiet);
        dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        Assert.That(dt.Rows, Has.Count.EqualTo(2));
        var results2 = new[] { (string)dt.Rows[0][0], (string)dt.Rows[1][0] };

        Assert.Multiple(() =>
        {
            Assert.That(results2[0], Is.EqualTo(results[0]));
            Assert.That(results2[1], Is.EqualTo(results[1]));
        });
    }

    [Test]
    public void TestIPluginCohortCompiler_TestCloneCic()
    {
        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
            { DisallowInput = true };

        // create a cohort config
        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic")
        {
            QueryCachingServer_ID = externalDatabaseServer.ID
        };
        cic.SaveToDatabase();

        // this special Catalogue will be detected by ExamplePluginCohortCompiler and interpreted as an API call
        var myApi = new Catalogue(CatalogueRepository, ExamplePluginCohortCompiler.ExampleAPIName);

        // add it to the cohort config
        cic.CreateRootContainerIfNotExists();

        // create a use of the API as an AggregateConfiguration
        var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(activator,
            new CatalogueCombineable(myApi), cic.RootCohortAggregateContainer);
        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);
        cmd.Execute();
        cmd.AggregateCreatedIfAny.Description = "33";
        cmd.AggregateCreatedIfAny.SaveToDatabase();

        // clone the cic
        var cmd2 = new ExecuteCommandCloneCohortIdentificationConfiguration(activator, cic);
        Assert.That(cmd2.IsImpossible, Is.False, cmd2.ReasonCommandImpossible);
        cmd2.Execute();

        var cloneAc = cmd2.CloneCreatedIfAny.RootCohortAggregateContainer.GetAggregateConfigurations()[0];
        Assert.That(cloneAc.Description, Is.EqualTo("33"));
    }

    [Test]
    public void TestIPluginCohortCompiler_APIsCantHavePatientIndexTables()
    {
        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
            { DisallowInput = true };

        // create a cohort config
        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic")
        {
            QueryCachingServer_ID = externalDatabaseServer.ID
        };
        cic.SaveToDatabase();

        // this special Catalogue will be detected by ExamplePluginCohortCompiler and interpreted as an API call
        var myApi = new Catalogue(CatalogueRepository, ExamplePluginCohortCompiler.ExampleAPIName);

        // add it to the cohort config
        cic.CreateRootContainerIfNotExists();

        // We need something in the root container otherwise the cic won't build
        var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(activator,
            new CatalogueCombineable(myApi), cic.RootCohortAggregateContainer);
        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);
        cmd.Execute();
        var regularAggregate = cmd.AggregateCreatedIfAny;

        // The thing we are wanting to test - creating a use of the API as a patient index table
        var cmd2 = new ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(
            activator, new CatalogueCombineable(myApi), cic);

        Assert.That(cmd2.IsImpossible, Is.False, cmd2.ReasonCommandImpossible);
        cmd2.Execute();

        var joinables = cic.GetAllJoinables();

        // make them join one another
        var ex = Assert.Throws<NotSupportedException>(() =>
            new JoinableCohortAggregateConfigurationUse(CatalogueRepository, regularAggregate, joinables[0]));

        Assert.That(ex.Message, Is.EqualTo("API calls cannot join with PatientIndexTables (The API call must be self contained)"));
    }


    [Test]
    public void TestIPluginCohortCompiler_AsPatientIndexTable()
    {
        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
            { DisallowInput = true };

        // Create a regular normal boring old table that will join into the results of the API call
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        using var dt = new DataTable();
        dt.Columns.Add("chi");
        dt.Rows.Add("0101010101");

        var tbl = db.CreateTable("RegularBoringOldTable", dt);
        var cata = (Catalogue)Import(tbl);
        var eiChi = cata.GetAllExtractionInformation()[0];

        eiChi.IsExtractionIdentifier = true;
        eiChi.SaveToDatabase();

        // create a cohort config
        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic")
        {
            QueryCachingServer_ID = externalDatabaseServer.ID
        };
        cic.SaveToDatabase();

        // this special Catalogue will be detected by ExamplePluginCohortCompiler and interpreted as an API call
        var myApi = new Catalogue(CatalogueRepository, ExamplePluginCohortCompiler.ExampleAPIName);

        // add it to the cohort config
        cic.CreateRootContainerIfNotExists();

        // Add the regular table
        var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(activator,
            new CatalogueCombineable(cata), cic.RootCohortAggregateContainer);
        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);
        cmd.Execute();
        var regularAggregate = cmd.AggregateCreatedIfAny;

        // The thing we are wanting to test - creating a use of the API as a patient index table
        var cmd2 = new ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(
            activator, new CatalogueCombineable(myApi), cic);

        Assert.That(cmd2.IsImpossible, Is.False, cmd2.ReasonCommandImpossible);
        cmd2.Execute();

        var joinables = cic.GetAllJoinables();

        Assert.That(joinables, Has.Length.EqualTo(1));

        // make them join one another
        new JoinableCohortAggregateConfigurationUse(CatalogueRepository, regularAggregate, joinables[0]);

        // run the cic again
        var source = new CohortIdentificationConfigurationSource();
        source.PreInitialize(cic, ThrowImmediatelyDataLoadEventListener.Quiet);
        var result = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
        Assert.That(result.Rows, Has.Count.EqualTo(1));
    }
}