using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.ExternalDatabaseServerPatching;
using CohortManagerLibrary.Execution;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CohortManagerTests
{
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
            var patcher = new QueryCachingDatabasePatcher();
            e.CreateAndPatchDatabaseWithDotDatabaseAssembly(patcher.GetDbAssembly(),new AcceptAllCheckNotifier());
            
            var serverReference = new ExternalDatabaseServer(CatalogueRepository, "Cache", patcher.GetDbAssembly());
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

            db = GetCleanedServer(DatabaseType.MicrosoftSQLServer,true);
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
        }
    }
}
