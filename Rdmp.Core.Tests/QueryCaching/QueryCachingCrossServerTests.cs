// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BadMedicine;
using BadMedicine.Datasets;
using FAnsi;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Databases;
using Rdmp.Core.QueryCaching.Aggregation;
using ReusableLibraryCode.Checks;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.QueryCaching
{
    class QueryCachingCrossServerTests: TestsRequiringA
    {
        [TestCase(DatabaseType.MicrosoftSQLServer,typeof(QueryCachingPatcher))]
        [TestCase(DatabaseType.MySql, typeof(QueryCachingPatcher))]
        [TestCase(DatabaseType.Oracle, typeof(QueryCachingPatcher))] 

        [TestCase(DatabaseType.MicrosoftSQLServer, typeof(DataQualityEnginePatcher))]
        public void Create_QueryCache(DatabaseType dbType,Type patcherType)
        {
            var db = GetCleanedServer(dbType);
            
            var patcher = (Patcher)Activator.CreateInstance(patcherType);

            var mds = new MasterDatabaseScriptExecutor(db);
            mds.CreateAndPatchDatabase(patcher, new AcceptAllCheckNotifier());
        }

        /// <summary>
        /// Tests running a cic in which there are 2 aggregates both hospitalizations with 1 EXCEPT the other.  The expected result is 0
        /// patients
        /// </summary>
        /// <param name="dbType"></param>
        [TestCase(DatabaseType.Oracle)]
        [TestCase(DatabaseType.MySql)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void Test_EXCEPT_TwoAggregates(DatabaseType dbType)
        {

            /*
             *           Server1
             *       _____________________
             *        |HospitalAdmissions x2|
             *         ↓ both run into    ↓
             *       ___________________
             *       |       Cache     |
             *
             *
             *      HospitalAdmissions
             *          EXCEPT
             *      HospitalAdmissions (copy)
             *         = 0
             */

            var db = GetCleanedServer(dbType);
            var cache = CreateCache(db);

            var r = new Random(500);
            var people = new PersonCollection();
            people.GeneratePeople(5000, r);

            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "cic");

            var ac1 = SetupAggregateConfiguration(db, people, r, cic);
            var ac2 = SetupAggregateConfiguration(db, people, r, cic);

            cic.CreateRootContainerIfNotExists();
            cic.QueryCachingServer_ID = cache.ID;
            cic.SaveToDatabase();

            var root = cic.RootCohortAggregateContainer;
            root.Operation = SetOperation.EXCEPT;
            root.Name = "EXCEPT";
            root.SaveToDatabase();
            root.AddChild(ac1,0);
            root.AddChild(ac2,1);
            
            var compiler = new CohortCompiler(cic);
            var runner = new CohortCompilerRunner(compiler, 50000);

            runner.Run(new CancellationToken());

            if (dbType == DatabaseType.MySql)
            {
                var crashed = compiler.Tasks.Single(t => t.Key.State == CompilationState.Crashed);    
                StringAssert.Contains("INTERSECT / UNION / EXCEPT are not supported by MySql", crashed.Key.CrashMessage.Message);
                return;
            }
            
            AssertNoErrors(compiler);

            Assert.AreEqual(compiler.Tasks.First().Key.FinalRowCount,0);
            Assert.Greater(compiler.Tasks.Skip(1).First().Key.FinalRowCount, 0); //both ac should have the same total
            Assert.Greater(compiler.Tasks.Skip(2).First().Key.FinalRowCount, 0); // that is not 0
            Assert.AreEqual(compiler.Tasks.Skip(1).First().Key.FinalRowCount,compiler.Tasks.Skip(2).First().Key.FinalRowCount);

            Assert.IsTrue(compiler.Tasks.Any(t => t.Key.GetCachedQueryUseCount().Equals("2/2")), "Expected EXCEPT container to use the cache");
        }

        /// <summary>
        /// Tests the ability to run a cic in which there are 2 tables, one patient index table with the dates of biochemistry test codes
        /// and another which joins and filters on the results of that table to produce a count of patients hospitalised after an NA test.
        ///
        /// <para>The <paramref name="createQueryCache"/> specifies whether a cache should be used</para>
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="createQueryCache"></param>
        [TestCase(DatabaseType.Oracle,true)]
        [TestCase(DatabaseType.MySql,true)]
        [TestCase(DatabaseType.MicrosoftSQLServer,true)]
        [TestCase(DatabaseType.Oracle, false)]
        [TestCase(DatabaseType.MySql, false)]
        [TestCase(DatabaseType.MicrosoftSQLServer, false)]
        public void Join_PatientIndexTable_OptionalCacheOnSameServer(DatabaseType dbType,bool createQueryCache)
        {
            /*
             *           Server1
                       _____________
             *        |Biochemistry|
             *               ↓
             *       ___________________
             *       | Cache (optional) |
             *            ↓ join ↓
             *    _____________________
             *    | Hospital Admissions|
             *
             */

            var db = GetCleanedServer(dbType);
            ExternalDatabaseServer cache = null;

            if (createQueryCache)
                cache = CreateCache(db);

            var r = new Random(500);
            var people = new PersonCollection();
            people.GeneratePeople(5000,r);

            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "cic");

            var joinable = SetupPatientIndexTable(db,people,r,cic);
            
            cic.CreateRootContainerIfNotExists();
            cic.QueryCachingServer_ID = cache?.ID;
            cic.SaveToDatabase();

            var hospitalAdmissions = SetupPatientIndexTableUser(db, people, r, cic, joinable);
            cic.RootCohortAggregateContainer.AddChild(hospitalAdmissions,0);

            var compiler = new CohortCompiler(cic);
            var runner = new CohortCompilerRunner(compiler, 50000);

            runner.Run(new CancellationToken());

            AssertNoErrors(compiler);

            if(createQueryCache)
                Assert.IsTrue(compiler.Tasks.Any(t => t.Key.GetCachedQueryUseCount().Equals("1/1")), "Expected cache to be used for the joinable");
            else
                Assert.IsTrue(compiler.Tasks.Any(t => t.Key.GetCachedQueryUseCount().Equals("0/1")), "Did not create cache so expected cache usage to be 0");
        }
        /// <summary>
        /// Tests the ability to run a cic in which there are 2 tables, one patient index table with the dates of biochemistry test codes
        /// and another which joins and filters on the results of that table to produce a count of patients hospitalised after an NA test.
        ///
        /// <para>In this case the cache is on another server so we cannot use the cached result for a join and should not try</para>
        /// </summary>
        /// <param name="dbType"></param>
        [TestCase(DatabaseType.Oracle)]
        [TestCase(DatabaseType.MySql)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void Join_PatientIndexTable_DoNotUseCacheOnDifferentServer(DatabaseType dbType)
        {
            /*
             *           Server1                    Server 2
             *         _____________                _________
             *        |Biochemistry|    →          | Cache  | (cache is still populated but not used in the resulting join).
             *               
             *            ↓ join ↓    (do not use cache)
             *    _____________________
             *    | Hospital Admissions|
             *
             */

            //get the data database
            var db = GetCleanedServer(dbType);

            //create the cache on the other server type (doesn't matter what type just as long as it's different).
            var dbCache =
                GetCleanedServer(Enum.GetValues(typeof(DatabaseType)).Cast<DatabaseType>().First(t => t != dbType));

            ExternalDatabaseServer cache = CreateCache(dbCache);

            var r = new Random(500);
            var people = new PersonCollection();
            people.GeneratePeople(5000, r);

            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "cic");

            var joinable = SetupPatientIndexTable(db, people, r, cic);

            cic.CreateRootContainerIfNotExists();
            cic.QueryCachingServer_ID = cache?.ID;
            cic.SaveToDatabase();

            var hospitalAdmissions = SetupPatientIndexTableUser(db, people, r, cic, joinable);
            cic.RootCohortAggregateContainer.AddChild(hospitalAdmissions, 0);

            var compiler = new CohortCompiler(cic);
            var runner = new CohortCompilerRunner(compiler, 50000);
            
            runner.Run(new CancellationToken());

            AssertNoErrors(compiler);
            
            Assert.IsTrue(compiler.Tasks.Any(t => t.Key.GetCachedQueryUseCount().Equals("1/1")),"Expected cache to be used only for the final UNION");
        }

        [TestCase(DatabaseType.Oracle)]
        [TestCase(DatabaseType.MySql)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void Join_PatientIndexTable_ThenShipToCacheForSets(DatabaseType dbType)
        {
            /*
            *           Server1                    Server 2
            *         _____________                _________
            *        |Biochemistry|    →          | Cache  | (cache is still populated but not used in the resulting join).
            *               
            *            ↓ join ↓    (do not use cache)
            *    _______________________
            *    | Hospital Admissions 1|        →       results1
            *
            *            EXCEPT
            *    _______________________
            *    | Hospital Admissions 2|        →        results 2
             *                                          ↓ result = 0 records
            */

            //get the data database
            var db = GetCleanedServer(dbType);

            //create the cache on the other server type (either Sql Server or Oracle) since  MySql can't do EXCEPT or UNION etc)
            var dbCache =
                GetCleanedServer(Enum.GetValues(typeof(DatabaseType)).Cast<DatabaseType>().First(t =>
                    t != dbType && t!= DatabaseType.MySql));

            ExternalDatabaseServer cache = CreateCache(dbCache);

            var r = new Random(500);
            var people = new PersonCollection();
            people.GeneratePeople(5000, r);

            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "cic");

            var joinable = SetupPatientIndexTable(db, people, r, cic);

            cic.CreateRootContainerIfNotExists();
            cic.QueryCachingServer_ID = cache?.ID;
            cic.SaveToDatabase();

            var hospitalAdmissions = SetupPatientIndexTableUser(db, people, r, cic, joinable);
            var hospitalAdmissions2 = SetupAggregateConfiguration(db, people,r, cic);

            var root = cic.RootCohortAggregateContainer;
            root.AddChild(hospitalAdmissions, 0);
            root.AddChild(hospitalAdmissions2, 1);

            root.Name = "EXCEPT";
            root.Operation = SetOperation.EXCEPT;
            root.SaveToDatabase();

            var compiler = new CohortCompiler(cic);
            var runner = new CohortCompilerRunner(compiler, 50000);

            runner.Run(new CancellationToken());

            AssertNoErrors(compiler);

            Assert.IsTrue(compiler.Tasks.Any(t => t.Key.GetCachedQueryUseCount().Equals("2/2")), "Expected cache to be used for both top level operations in the EXCEPT");
        }


        #region helper methods

        private ExternalDatabaseServer CreateCache(DiscoveredDatabase db)
        {
            var patcher = new QueryCachingPatcher();
            var mds = new MasterDatabaseScriptExecutor(db);
            mds.CreateAndPatchDatabase(patcher, new AcceptAllCheckNotifier());

            var server = new ExternalDatabaseServer(CatalogueRepository, "Cache", patcher);
            server.SetProperties(db);

            return server;
        }

        /// <summary>
        /// Creates a new patient index table based on Biochemistry which selects the distinct dates of "NA" test results
        /// for every patient
        /// </summary>
        /// <param name="db"></param>
        /// <param name="people"></param>
        /// <param name="r"></param>
        /// <param name="cic"></param>
        /// <returns></returns>
        private JoinableCohortAggregateConfiguration SetupPatientIndexTable(DiscoveredDatabase db, PersonCollection people, Random r, CohortIdentificationConfiguration cic)
        {
            var tbl = CreateDataset<Biochemistry>(db, people, 10000, r);
            var cata = Import(tbl,out _, out _, out _,out ExtractionInformation[] eis);

            var chi = eis.Single(ei => ei.GetRuntimeName().Equals("chi", StringComparison.CurrentCultureIgnoreCase));
            var code = eis.Single(ei => ei.GetRuntimeName().Equals("TestCode", StringComparison.CurrentCultureIgnoreCase));
            var date = eis.Single(ei => ei.GetRuntimeName().Equals("SampleDate", StringComparison.CurrentCultureIgnoreCase));

            chi.IsExtractionIdentifier = true;
            chi.SaveToDatabase();

            var ac = new AggregateConfiguration(CatalogueRepository, cata, "NA by date");
            ac.AddDimension(chi);
            ac.AddDimension(code);
            ac.AddDimension(date);
            ac.CountSQL = null;
            
            cic.EnsureNamingConvention(ac);

            var and = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
            var filter = new AggregateFilter(CatalogueRepository,"TestCode is NA",and);
            filter.WhereSQL = "TestCode = 'NA'";
            filter.SaveToDatabase();

            ac.RootFilterContainer_ID = and.ID;
            ac.SaveToDatabase();

            return new JoinableCohortAggregateConfiguration(CatalogueRepository, cic, ac);
        }

        /// <summary>
        /// Creates a table HospitalAdmissions that uses the patient index table <paramref name="joinable"/> to return distinct patients
        /// who have been hospitalised after receiving an NA test (no time limit)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="people"></param>
        /// <param name="r"></param>
        /// <param name="cic"></param>
        /// <param name="joinable"></param>
        private AggregateConfiguration SetupPatientIndexTableUser(DiscoveredDatabase db, PersonCollection people, Random r, CohortIdentificationConfiguration cic, JoinableCohortAggregateConfiguration joinable)
        {
            var ac = SetupAggregateConfiguration(db,people,r,cic);
            
            var and = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
            var filter = new AggregateFilter(CatalogueRepository, "Hospitalised after an NA", and);
            filter.WhereSQL = "AdmissionDate > SampleDate";
            filter.SaveToDatabase();

            ac.RootFilterContainer_ID = and.ID;
            ac.SaveToDatabase();

            //ac joins against the joinable
            new JoinableCohortAggregateConfigurationUse(CatalogueRepository, ac, joinable);

            return ac;

        }

        /// <summary>
        /// Creates a table HospitalAdmissions with no filters 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="people"></param>
        /// <param name="r"></param>
        /// <param name="cic"></param>
        /// <returns></returns>
        private AggregateConfiguration SetupAggregateConfiguration(DiscoveredDatabase db, PersonCollection people, Random r, CohortIdentificationConfiguration cic)
        {
            var existingTbl = db.ExpectTable("HospitalAdmissions");
            var tbl = existingTbl.Exists() ? existingTbl : CreateDataset<HospitalAdmissions>(db, people, 10000, r);
            var cata = Import(tbl, out _, out _, out _, out ExtractionInformation[] eis);

            var chi = eis.Single(ei => ei.GetRuntimeName().Equals("chi", StringComparison.CurrentCultureIgnoreCase));
            chi.IsExtractionIdentifier = true;
            chi.SaveToDatabase();

            var ac = new AggregateConfiguration(CatalogueRepository, cata, "Hospitalised after NA");
            ac.AddDimension(chi);

            ac.CountSQL = null;
            cic.EnsureNamingConvention(ac);

            return ac;
        }


        /// <summary>
        /// Shows the state of the <paramref name="compiler"/> and asserts that all the jobs are finished
        /// </summary>
        /// <param name="compiler"></param>
        private void AssertNoErrors(CohortCompiler compiler)
        {
            Assert.IsNotEmpty(compiler.Tasks);

            TestContext.WriteLine($"| Task | Type | State | Error | RowCount | CacheUse |");


            int i = 0;
            foreach (var kvp in compiler.Tasks)
                TestContext.WriteLine($"{i++} - {kvp.Key.ToString()} | {kvp.Key.GetType()} | {kvp.Key.State} | {kvp.Key?.CrashMessage} | {kvp.Key.FinalRowCount} | {kvp.Key.GetCachedQueryUseCount()}");
            
            Assert.IsTrue(compiler.Tasks.All(t => t.Key.State == CompilationState.Finished), "Expected all tasks to finish without error");
        }
        #endregion
    }
}
