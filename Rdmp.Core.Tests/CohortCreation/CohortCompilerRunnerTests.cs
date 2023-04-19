// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Linq;
using System.Threading;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCreation;

public class CohortCompilerRunnerTests:DatabaseTests
{
    [Test]
    public void CacheIdentifierListWithRunner_SimpleCase()
    {
        DiscoveredDatabase db;
        CohortIdentificationConfiguration cic;
        DataTable dt;

        SetupCohort(out db,out cic,out dt);

        var compiler = new CohortCompiler(cic);

        var runner = new CohortCompilerRunner(compiler, 5000);
        runner.Run(new CancellationToken());

        Assert.AreEqual(CohortCompilerRunner.Phase.Finished, runner.ExecutionPhase);

        var rootTask = runner.Compiler.Tasks.Single(t => t.Key is AggregationContainerTask);
            
        Assert.IsTrue(rootTask.Value.IsResultsForRootContainer);
        Assert.IsNull(rootTask.Key.CrashMessage);
        Assert.AreEqual(CompilationState.Finished, rootTask.Key.State);

        Assert.AreEqual(dt.Rows.Count,rootTask.Value.Identifiers.Rows.Count);
    }

    [Test]
    public void CacheIdentifierListWithRunner_WithCaching()
    {
        DiscoveredDatabase db;
        CohortIdentificationConfiguration cic;
        DataTable dt;

        SetupCohort(out db, out cic, out dt);
            
        MasterDatabaseScriptExecutor e = new MasterDatabaseScriptExecutor(db);
        var p = new QueryCachingPatcher();
        e.CreateAndPatchDatabase(p,new AcceptAllCheckNotifier());
            
        var serverReference = new ExternalDatabaseServer(CatalogueRepository, "Cache", p);
        serverReference.SetProperties(db);

        cic.QueryCachingServer_ID = serverReference.ID;
        cic.SaveToDatabase();

        var compiler = new CohortCompiler(cic);

        var runner = new CohortCompilerRunner(compiler, 5000);
        runner.Run(new CancellationToken());

        Assert.AreEqual(CohortCompilerRunner.Phase.Finished, runner.ExecutionPhase);

        var rootTask = runner.Compiler.Tasks.Single(t => t.Key is AggregationContainerTask);

        Assert.IsTrue(rootTask.Value.IsResultsForRootContainer);
        Assert.IsNull(rootTask.Key.CrashMessage);
        Assert.AreEqual(CompilationState.Finished, rootTask.Key.State);

        Assert.IsTrue(runner.Compiler.AreaAllQueriesCached(rootTask.Key));

        Assert.AreEqual(dt.Rows.Count, rootTask.Value.Identifiers.Rows.Count);
    }
    private void SetupCohort(out DiscoveredDatabase db, out CohortIdentificationConfiguration cic, out DataTable dt)
    {
        dt = new DataTable();
        dt.Columns.Add("PK");

        //add lots of rows
        for (int i = 0; i < 100000; i++)
            dt.Rows.Add(i);

        db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        var tbl = db.CreateTable("CohortCompilerRunnerTestsTable", dt);

        var cata = Import(tbl);

        var ei = cata.CatalogueItems[0].ExtractionInformation;
        ei.IsExtractionIdentifier = true;
        ei.SaveToDatabase();

        var agg = new AggregateConfiguration(CatalogueRepository, cata, "MyAgg");
        agg.CountSQL = null;
        agg.SaveToDatabase();
        var dimension = new AggregateDimension(CatalogueRepository, ei, agg);

        cic = new CohortIdentificationConfiguration(CatalogueRepository, "MyCic");
        cic.CreateRootContainerIfNotExists();
        cic.RootCohortAggregateContainer.AddChild(agg, 0);

        cic.EnsureNamingConvention(agg);
    }
}