// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Threading;
using SynthEHR;
using SynthEHR.Datasets;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;
using Tests.Common.Scenarios;
using TypeGuesser;

namespace Rdmp.Core.Tests.QueryCaching;

internal class QueryCachingCrossServerTests : TestsRequiringA
{
    [TestCase(DatabaseType.MicrosoftSQLServer, typeof(QueryCachingPatcher))]
    [TestCase(DatabaseType.MySql, typeof(QueryCachingPatcher))]
    [TestCase(DatabaseType.Oracle, typeof(QueryCachingPatcher))]
    [TestCase(DatabaseType.PostgreSql, typeof(QueryCachingPatcher))]
    [TestCase(DatabaseType.MicrosoftSQLServer, typeof(DataQualityEnginePatcher))]
    public void Create_QueryCache(DatabaseType dbType, Type patcherType)
    {
        var db = GetCleanedServer(dbType);

        var patcher = (Patcher)Activator.CreateInstance(patcherType);

        var mds = new MasterDatabaseScriptExecutor(db);
        mds.CreateAndPatchDatabase(patcher, new AcceptAllCheckNotifier());
    }


    /// <summary>
    /// Tests running a simple cic with a single UNION container holding an aggregate.  The aggregate optionally has a parameter on it.  The
    /// second server is the cache server
    /// </summary>
    /// <param name="dbType"></param>
    [TestCase(DatabaseType.MySql, true)]
    [TestCase(DatabaseType.MySql, false)]
    [TestCase(DatabaseType.MicrosoftSQLServer, true)]
    [TestCase(DatabaseType.MicrosoftSQLServer, false)]
    //[TestCase(DatabaseType.Oracle,true)] //Oracle FAnsi doesn't currently support parameters
    //[TestCase(DatabaseType.Oracle,false)]
    public void Test_SingleServer_WithOneParameter(DatabaseType dbType, bool useParameter)
    {
        /*
         *           Server1                        Server2
         *       _____________________         _____________________
         *        |HospitalAdmissions |   →    |      Cache         |
         *           @date_of_max
         *
         */

        var server1 = GetCleanedServer(dbType);
        var server2 = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var cache = CreateCache(server2);

        var r = new Random(500);
        var people = new PersonCollection();
        people.GeneratePeople(5000, r);

        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "cic");
        var ac1 = SetupAggregateConfigurationWithFilter(server1, people, r, cic, useParameter, "@date_of_max",
            "'2001-01-01'");

        cic.CreateRootContainerIfNotExists();
        cic.QueryCachingServer_ID = cache.ID;
        cic.SaveToDatabase();

        var root = cic.RootCohortAggregateContainer;
        root.AddChild(ac1, 0);

        var compiler = new CohortCompiler(cic);
        var runner = new CohortCompilerRunner(compiler, 50000);
        runner.Run(new CancellationToken());

        AssertNoErrors(compiler);

        Assert.That(
            compiler.Tasks.Where(t => t.Key is AggregationContainerTask)
                .Any(t => t.Key.GetCachedQueryUseCount().Equals("1/1")), "Expected UNION container to use the cache");
    }

    /// <summary>
    /// Tests running a simple cic with a single UNION container holding two aggregates.  The aggregates both have their own parameter
    /// called @date_of_max but they have different values.  This means the SQL generated needs to be adjusted so they don't collide /
    /// overwrite one another.
    /// </summary>
    /// <param name="dbType"></param>
    [TestCase(DatabaseType.MySql, true)]
    //[TestCase(DatabaseType.MySql,false)] //only works with cache because container operations (UNION) are not supported on MySql
    [TestCase(DatabaseType.MicrosoftSQLServer, true)]
    [TestCase(DatabaseType.MicrosoftSQLServer, false)]
    //[TestCase(DatabaseType.Oracle)] //Oracle FAnsi doesn't currently support parameters
    public void Test_SingleServer_WithTwoParameters(DatabaseType dbType, bool useCache)
    {
        /*
         *           Server1                        Server2
         *        ____________________         _____________________
         *        |HospitalAdmissions |   →    |      Cache         |
         *           @date_of_max
         *        ____________________     ↗
         *        |HospitalAdmissions |
         *           @date_of_max
         *
         *   (has different value so rename operations come into effect)
         */

        var server1 = GetCleanedServer(dbType);
        var server2 = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var cache = useCache ? CreateCache(server2) : null;

        var r = new Random(500);
        var people = new PersonCollection();
        people.GeneratePeople(5000, r);

        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "cic");

        var ac1 = SetupAggregateConfigurationWithFilter(server1, people, r, cic, true, "@date_of_max", "'2001-01-01'");
        var ac2 = SetupAggregateConfigurationWithFilter(server1, people, r, cic, true, "@date_of_max", "'2005-01-01'");

        cic.CreateRootContainerIfNotExists();
        cic.QueryCachingServer_ID = cache?.ID;
        cic.SaveToDatabase();

        var root = cic.RootCohortAggregateContainer;
        root.AddChild(ac1, 0);
        root.AddChild(ac2, 1);

        var compiler = new CohortCompiler(cic);
        var runner = new CohortCompilerRunner(compiler, 50000);
        runner.Run(new CancellationToken());

        AssertNoErrors(compiler);

        //each container on its own ran with the normal SQL
        AssertNoErrors(compiler, ac1, "@date_of_max='2001-01-01'");
        AssertNoErrors(compiler, ac2, "@date_of_max='2005-01-01'");


        if (useCache)
        {
            //the root container run with dual cache fetch and no parameters
            AssertNoErrors(compiler, root, "IndexedExtractionIdentifierList");
            AssertCacheUsed(compiler, root, "2/2");
        }
        else
        {
            //the root container ran with a rename operations (no cache available)
            AssertNoErrors(compiler, root, "@date_of_max='2001-01-01'", "@date_of_max_2='2005-01-01'");
            AssertCacheUsed(compiler, root, "0/2");
        }
    }

    /// <summary>
    /// Tests running a simple cic a cohort that has a parameter and a patient index table which also has a parameter.
    /// </summary>
    /// <param name="dbType"></param>
    [TestCase(DatabaseType.MySql, true, true)]
    [TestCase(DatabaseType.MySql, true, false)]
    [TestCase(DatabaseType.MySql, false, true)]
    [TestCase(DatabaseType.MySql, false, false)]
    [TestCase(DatabaseType.MicrosoftSQLServer, true, true)]
    [TestCase(DatabaseType.MicrosoftSQLServer, true, false)]
    [TestCase(DatabaseType.MicrosoftSQLServer, false, true)]
    [TestCase(DatabaseType.MicrosoftSQLServer, false, false)]
    public void Test_SingleServerPatientIndexTable_WithTwoParameters(DatabaseType dbType, bool useSameName,
        bool useCache)
    {
        /*
         *           Server1                                  (Also Server1 - if useCache is true)
         *        ______________________________            __________________________
         *        |   Patient Index Table       |     →    |         Cache           |
         *        |  (NA by date )              |         ↗
         *           @date_of_max '2001-01-01'
         *                                              ↗
         *            JOIN (should use cache)
         *         ___________________________       ↗
         *        | Hospitalised after NA (ac) |
         *          @date_of_max (or @maximum) '2005-01-01'
         *
         *   (has different value so rename operations come into effect)
         */

        var server1 = GetCleanedServer(dbType);

        var cache = useCache ? CreateCache(server1) : null;

        var r = new Random(500);
        var people = new PersonCollection();
        people.GeneratePeople(5000, r);

        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "cic");

        var patientIndexTable =
            SetupPatientIndexTableWithFilter(server1, people, r, cic, true, "@date_of_max", "'2001-01-01'");
        var ac = SetupPatientIndexTableUserWithFilter(server1, people, r, cic, patientIndexTable, true,
            useSameName ? "@date_of_max" : "@maximum", "'2005-01-01'");

        cic.CreateRootContainerIfNotExists();
        cic.QueryCachingServer_ID = cache?.ID;
        cic.SaveToDatabase();

        var root = cic.RootCohortAggregateContainer;
        root.AddChild(ac, 0);

        var compiler = new CohortCompiler(cic);
        var runner = new CohortCompilerRunner(compiler, 50000);
        runner.Run(new CancellationToken());

        if (useSameName)
        {
            AssertCrashed(compiler, ac,
                "PatientIndexTables cannot have parameters with the same name as their users.  Offending parameter(s) were @date_of_max");
        }
        else
        {
            if (useCache)
            {
                //we should hit up the cache for the interior of the query and therefore not need the parameter
                AssertNoErrors(compiler, ac, "@maximum='2005-01-01'", "JoinableInceptionQuery_AggregateConfiguration",
                    "AdmissionDate. < @maximum");

                AssertCacheUsed(compiler, root, "1/1");
            }
            else
            {
                AssertNoErrors(compiler, ac, "@date_of_max='2001-01-01'", "@maximum='2005-01-01'",
                    "SampleDate. < @date_of_max", "AdmissionDate. < @maximum");
            }

            AssertNoErrors(compiler);
        }
    }


    /// <summary>
    /// Tests running a cic in which there are 2 aggregates both hospitalizations with 1 EXCEPT the other.  The expected result is 0
    /// patients
    /// </summary>
    /// <param name="dbType"></param>
    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
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
        root.AddChild(ac1, 0);
        root.AddChild(ac2, 1);

        var compiler = new CohortCompiler(cic);
        var runner = new CohortCompilerRunner(compiler, 50000);

        runner.Run(new CancellationToken());

        AssertNoErrors(compiler);

        Assert.Multiple(() =>
        {
            Assert.That(
    compiler.Tasks.Single(static t => t.Value is { IsResultsForRootContainer: true }).Key.FinalRowCount, Is.EqualTo(0));
            Assert.That(
                compiler.Tasks.Single(t => t.Key is AggregationTask at && at.Aggregate.Equals(ac1)).Key.FinalRowCount, Is.GreaterThan(0)); //both ac should have the same total
            Assert.That(
                compiler.Tasks.Single(t => t.Key is AggregationTask at && at.Aggregate.Equals(ac2)).Key.FinalRowCount, Is.GreaterThan(0)); // that is not 0

            Assert.That(compiler.Tasks.Any(static t => t.Key.GetCachedQueryUseCount().Equals("2/2")),
                "Expected EXCEPT container to use the cache");
        });
    }

    /// <summary>
    /// Tests the ability to run a cic in which there are 2 tables, one patient index table with the dates of biochemistry test codes
    /// and another which joins and filters on the results of that table to produce a count of patients hospitalised after an NA test.
    ///
    /// <para>The <paramref name="createQueryCache"/> specifies whether a cache should be used</para>
    /// </summary>
    /// <param name="dbType"></param>
    /// <param name="createQueryCache"></param>
    [TestCase(DatabaseType.Oracle, true)]
    [TestCase(DatabaseType.MySql, true)]
    [TestCase(DatabaseType.MicrosoftSQLServer, true)]
    [TestCase(DatabaseType.Oracle, false)]
    [TestCase(DatabaseType.MySql, false)]
    [TestCase(DatabaseType.MicrosoftSQLServer, false)]
    public void Join_PatientIndexTable_OptionalCacheOnSameServer(DatabaseType dbType, bool createQueryCache)
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

        if (createQueryCache)
            Assert.That(compiler.Tasks.Any(t => t.Key.GetCachedQueryUseCount().Equals("1/1")),
                "Expected cache to be used for the joinable");
        else
            Assert.That(compiler.Tasks.Any(t => t.Key.GetCachedQueryUseCount().Equals("0/1")),
                "Did not create cache so expected cache usage to be 0");
    }

    /// <summary>
    /// Tests the ability to run a cic in which there are 2 tables, one patient index table with the dates of biochemistry test codes
    /// and another which joins and filters on the results of that table to produce a count of patients hospitalised after an NA test.
    ///
    /// <para>In this case the cache is on another server so we cannot use the cached result for a join and should not try</para>
    /// </summary>
    /// <param name="dbType"></param>
    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
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

        //create the cache on the other server type (doesn't matter what type just as long as its different).
        var dbCache =
            GetCleanedServer(Enum.GetValues(typeof(DatabaseType)).Cast<DatabaseType>().First(t => t != dbType));

        var cache = CreateCache(dbCache);

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

        Assert.That(compiler.Tasks.Any(t => t.Key.GetCachedQueryUseCount().Equals("1/1")),
            "Expected cache to be used only for the final UNION");
    }

    /// <summary>
    /// Tests the ability to run a cic in which there are 2 tables that must be joined but are on separate servers.
    /// A patient index table on server 1 and a table on server 2 to which it must be joined.
    ///
    /// <para>This only works if there is a cache on server 2</para>
    ///
    /// </summary>
    [Test]
    public void Join_PatientIndexTable_NotOnCacheServer()
    {
        /*
         *           Server1                    Server 2
         *         _____________                _________
         *        |Biochemistry|    →          | Cache  |  (cache must first be populated)
         *
         *                        ↘ join   ↘ (must use cache)
         *                                  _____________________
         *                                  | Hospital Admissions|
         *
         */

        var server1 = GetCleanedServer(DatabaseType.MySql);
        var server2 = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var cache = CreateCache(server2);

        var r = new Random(500);
        var people = new PersonCollection();
        people.GeneratePeople(5000, r);

        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "cic");

        var joinable = SetupPatientIndexTable(server1, people, r, cic);

        cic.CreateRootContainerIfNotExists();
        cic.QueryCachingServer_ID = cache?.ID;
        cic.SaveToDatabase();

        var hospitalAdmissions = SetupPatientIndexTableUser(server2, people, r, cic, joinable);
        cic.RootCohortAggregateContainer.AddChild(hospitalAdmissions, 0);

        var compiler = new CohortCompiler(cic);
        var runner = new CohortCompilerRunner(compiler, 50000);

        runner.Run(new CancellationToken());

        AssertNoErrors(compiler);

        Assert.That(compiler.Tasks.Any(t => t.Key.GetCachedQueryUseCount().Equals("1/1")),
            "Expected cache to be used only for the final UNION");
    }


    /// <summary>
    /// Tests the ability to run a cic in which there are 2 tables that must be joined but are on separate servers.
    /// A patient index table on server 1 and a table on server 2 to which it must be joined.
    ///
    /// <para>This should fail because the only way to combine the two datasets is with a join and they are on separate servers
    /// The cache cannot be used at all during the join because it's on a separate server.'
    /// </para>
    ///
    /// </summary>
    [Test]
    public void Join_PatientIndexTable_ThreeServers()
    {
        /*
         *           Server1                    Server 2                                Server 3
         *         _____________                                                       _________
         *        |Biochemistry|    →          (successfully caches joinable bit)      | Cache  |
         *
         *                        ↘ join   ↘ (should crash)
         *                                  _____________________
         *                                  | Hospital Admissions|
         *
         */

        var server1 = GetCleanedServer(DatabaseType.MySql);
        var server2 = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        var server3 = GetCleanedServer(DatabaseType.Oracle);

        var cache = CreateCache(server3);

        var r = new Random(500);
        var people = new PersonCollection();
        people.GeneratePeople(5000, r);

        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "cic");

        var joinable = SetupPatientIndexTable(server1, people, r, cic);

        cic.CreateRootContainerIfNotExists();
        cic.QueryCachingServer_ID = cache?.ID;
        cic.SaveToDatabase();

        var hospitalAdmissions = SetupPatientIndexTableUser(server2, people, r, cic, joinable);
        cic.RootCohortAggregateContainer.AddChild(hospitalAdmissions, 0);

        var compiler = new CohortCompiler(cic);
        var runner = new CohortCompilerRunner(compiler, 50000);

        runner.Run(new CancellationToken());

        var hospitalAdmissionsTask = compiler.Tasks.Keys.OfType<AggregationTask>()
            .Single(t => t.Aggregate.Equals(hospitalAdmissions));

        Assert.Multiple(() =>
        {
            Assert.That(hospitalAdmissionsTask.State, Is.EqualTo(CompilationState.Crashed));

            Assert.That(hospitalAdmissionsTask.CrashMessage.ToString(), Does.Contain("is not fully cached and CacheUsageDecision is MustUse"));
        });
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
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
                t != dbType && t != DatabaseType.MySql));

        var cache = CreateCache(dbCache);

        var r = new Random(500);
        var people = new PersonCollection();
        people.GeneratePeople(5000, r);

        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "cic");

        var joinable = SetupPatientIndexTable(db, people, r, cic);

        cic.CreateRootContainerIfNotExists();
        cic.QueryCachingServer_ID = cache?.ID;
        cic.SaveToDatabase();

        var hospitalAdmissions = SetupPatientIndexTableUser(db, people, r, cic, joinable);
        var hospitalAdmissions2 = SetupAggregateConfiguration(db, people, r, cic);

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

        Assert.That(compiler.Tasks.Any(t => t.Key.GetCachedQueryUseCount().Equals("2/2")),
            "Expected cache to be used for both top level operations in the EXCEPT");
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
    private JoinableCohortAggregateConfiguration SetupPatientIndexTable(DiscoveredDatabase db, PersonCollection people,
        Random r, CohortIdentificationConfiguration cic)
    {
        var syntax = db.Server.GetQuerySyntaxHelper();

        var tbl = CreateDataset<Biochemistry>(db, people, 10000, r);
        var cata = Import(tbl, out _, out _, out _, out var eis);

        var chi = eis.Single(ei => ei.GetRuntimeName().Equals("chi", StringComparison.CurrentCultureIgnoreCase));
        var code = eis.Single(ei => ei.GetRuntimeName().Equals("TestCode", StringComparison.CurrentCultureIgnoreCase));
        var date = eis.Single(ei =>
            ei.GetRuntimeName().Equals("SampleDate", StringComparison.CurrentCultureIgnoreCase));

        chi.IsExtractionIdentifier = true;
        chi.SaveToDatabase();

        var ac = new AggregateConfiguration(CatalogueRepository, cata, "NA by date");
        ac.AddDimension(chi);
        ac.AddDimension(code);
        ac.AddDimension(date);
        ac.CountSQL = null;

        cic.EnsureNamingConvention(ac);

        var and = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
        var filter = new AggregateFilter(CatalogueRepository, "TestCode is NA", and)
        {
            WhereSQL = $"{syntax.EnsureWrapped("TestCode")} = 'NA'"
        };
        filter.SaveToDatabase();

        ac.RootFilterContainer_ID = and.ID;
        ac.SaveToDatabase();

        return new JoinableCohortAggregateConfiguration(CatalogueRepository, cic, ac);
    }

    private JoinableCohortAggregateConfiguration SetupPatientIndexTableWithFilter(DiscoveredDatabase db,
        PersonCollection people, Random r, CohortIdentificationConfiguration cic, bool useParameter, string paramName,
        string paramValue)
    {
        var syntax = db.Server.GetQuerySyntaxHelper();

        var joinable = SetupPatientIndexTable(db, people, r, cic);
        AddFilter(db, "Test After", $"{syntax.EnsureWrapped("SampleDate")} < ", joinable.AggregateConfiguration,
            useParameter, paramName, paramValue);
        return joinable;
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
    private AggregateConfiguration SetupPatientIndexTableUser(DiscoveredDatabase db, PersonCollection people, Random r,
        CohortIdentificationConfiguration cic, JoinableCohortAggregateConfiguration joinable)
    {
        var syntax = db.Server.GetQuerySyntaxHelper();

        var ac = SetupAggregateConfiguration(db, people, r, cic);

        var and = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
        var filter = new AggregateFilter(CatalogueRepository, "Hospitalised after an NA", and)
        {
            WhereSQL = $"{syntax.EnsureWrapped("AdmissionDate")} > {syntax.EnsureWrapped("SampleDate")}"
        };
        filter.SaveToDatabase();

        ac.RootFilterContainer_ID = and.ID;
        ac.SaveToDatabase();

        //ac joins against the joinable
        new JoinableCohortAggregateConfigurationUse(CatalogueRepository, ac, joinable);

        return ac;
    }

    private AggregateConfiguration SetupPatientIndexTableUserWithFilter(DiscoveredDatabase db, PersonCollection people,
        Random r, CohortIdentificationConfiguration cic, JoinableCohortAggregateConfiguration joinable,
        bool useParameter, string paramName, string paramValue)
    {
        var syntax = db.Server.GetQuerySyntaxHelper();

        var ac1 = SetupPatientIndexTableUser(db, people, r, cic, joinable);

        AddFilter(db, "My Filter", $"{syntax.EnsureWrapped("AdmissionDate")} < ", ac1, useParameter, paramName,
            paramValue);

        return ac1;
    }

    private void AddFilter(DiscoveredDatabase db, string filterName, string whereSqlFirstHalf,
        AggregateConfiguration ac, bool useParameter, string paramName, string paramValue)
    {
        var container = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
        ac.RootFilterContainer_ID = container.ID;
        ac.SaveToDatabase();

        //create a filter
        var filter = new AggregateFilter(CatalogueRepository, filterName, container);

        if (useParameter)
        {
            filter.WhereSQL = whereSqlFirstHalf + paramName;

            var parameterSql = db.Server.GetQuerySyntaxHelper()
                .GetParameterDeclaration(paramName, new DatabaseTypeRequest(typeof(DateTime)));

            var parameter = filter.GetFilterFactory().CreateNewParameter(filter, parameterSql);
            parameter.Value = paramValue;
            parameter.SaveToDatabase();
        }
        else
        {
            filter.WhereSQL = whereSqlFirstHalf + paramValue;
        }

        filter.SaveToDatabase();
    }

    /// <summary>
    /// Creates a table HospitalAdmissions with no filters
    /// </summary>
    /// <param name="db"></param>
    /// <param name="people"></param>
    /// <param name="r"></param>
    /// <param name="cic"></param>
    /// <returns></returns>
    private AggregateConfiguration SetupAggregateConfiguration(DiscoveredDatabase db, PersonCollection people, Random r,
        CohortIdentificationConfiguration cic)
    {
        var existingTbl = db.ExpectTable("HospitalAdmissions");
        var tbl = existingTbl.Exists() ? existingTbl : CreateDataset<HospitalAdmissions>(db, people, 10000, r);
        var cata = Import(tbl, out _, out _, out _, out var eis);

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
    /// Creates an AggregateConfiguration with a filter on date that optionally uses a parameter e.g. @example
    /// </summary>
    /// <param name="db"></param>
    /// <param name="people"></param>
    /// <param name="r"></param>
    /// <param name="cic"></param>
    /// <param name="useParameter"></param>
    /// <param name="paramName"></param>
    /// <param name="paramValue"></param>
    /// <returns></returns>
    private AggregateConfiguration SetupAggregateConfigurationWithFilter(DiscoveredDatabase db, PersonCollection people,
        Random r, CohortIdentificationConfiguration cic, bool useParameter, string paramName, string paramValue)
    {
        var syntax = db.Server.GetQuerySyntaxHelper();
        var ac1 = SetupAggregateConfiguration(db, people, r, cic);
        AddFilter(db, "My Filter", $"{syntax.EnsureWrapped("AdmissionDate")} < ", ac1, useParameter, paramName,
            paramValue);
        return ac1;
    }

    /// <summary>
    /// Shows the state of the <paramref name="compiler"/> and asserts that all the jobs are finished
    /// </summary>
    /// <param name="compiler"></param>
    private static void AssertNoErrors(CohortCompiler compiler)
    {
        Assert.That(compiler.Tasks, Is.Not.Empty);

        TestContext.WriteLine($"| Task | Type | State | Error | RowCount | CacheUse |");


        var i = 0;
        foreach (var kvp in compiler.Tasks.Keys)
            TestContext.WriteLine(
                $"{i++} - {kvp.ToString()} | {kvp.GetType()} | {kvp.State} | {kvp.CrashMessage} | {kvp.FinalRowCount} | {kvp.GetCachedQueryUseCount()}");

        Assert.That(compiler.Tasks.All(static t => t.Key.State == CompilationState.Finished),
            "Expected all tasks to finish without error");
    }

    /// <summary>
    /// Asserts that the given <paramref name="task"/> (when run on its own) crashed with the given
    /// <see cref="expectedErrorMessageToContain"/>
    /// </summary>
    private static void AssertCrashed(CohortCompiler compiler, AggregateConfiguration task,
        string expectedErrorMessageToContain)
    {
        var acResult = compiler.Tasks.Single(t => t.Key is AggregationTask a && a.Aggregate.Equals(task));
        Assert.Multiple(() =>
        {
            Assert.That(acResult.Key.State, Is.EqualTo(CompilationState.Crashed));
            Assert.That(acResult.Key.CrashMessage.Message, Does.Contain(expectedErrorMessageToContain));
        });
    }

    /// <summary>
    /// Asserts that the given <paramref name="task"/> (when run on its own) completed successfully and that the SQL executed
    /// included all the regex patterns <see cref="expectedSqlBits"/>
    /// </summary>
    /// <param name="compiler"></param>
    /// <param name="task"></param>
    /// <param name="expectedSqlBits">regex patterns you expect to be in the sql executed</param>
    private static void AssertNoErrors(CohortCompiler compiler, AggregateConfiguration task,
        params string[] expectedSqlBits)
    {
        var acResult = compiler.Tasks.Single(t => t.Key is AggregationTask a && a.Aggregate.Equals(task));

        Console.WriteLine($"Build Log For '{task}':");
        Console.WriteLine(acResult.Key.Log);

        Assert.That(acResult.Key.State, Is.EqualTo(CompilationState.Finished));

        foreach (var s in expectedSqlBits)
            Assert.That(acResult.Value.CountSQL, Does.Match(s));
    }

    /// <summary>
    /// Asserts that the given <paramref name="task"/> (when run on its own) completed successfully and that the SQL executed
    /// included all the regex patterns <see cref="expectedSqlBits"/>
    /// </summary>
    /// <param name="compiler"></param>
    /// <param name="task"></param>
    /// <param name="expectedSqlBits">regex patterns you expect to be in the sql executed</param>
    private static void AssertNoErrors(CohortCompiler compiler, CohortAggregateContainer task,
        params string[] expectedSqlBits)
    {
        var acResult = compiler.Tasks.Single(t => t.Key is AggregationContainerTask a && a.Container.Equals(task));
        Assert.That(acResult.Key.State, Is.EqualTo(CompilationState.Finished));

        foreach (var s in expectedSqlBits)
            Assert.That(acResult.Value.CountSQL, Does.Contain(s));
    }

    /// <summary>
    /// Asserts that the given <paramref name="container"/> ran successfully and fetched its results from the cache (if any) with the
    /// given <paramref name="expectedCacheUsageCount"/>
    /// </summary>
    /// <param name="compiler"></param>
    /// <param name="container"></param>
    /// <param name="expectedCacheUsageCount">The amount you expect to be used of the cache e.g. "2/2" </param>
    private static void AssertCacheUsed(CohortCompiler compiler, CohortAggregateContainer container,
        string expectedCacheUsageCount)
    {
        //cache should have been used
        var containerResult =
            compiler.Tasks.Single(t => t.Key is AggregationContainerTask c && c.Container.Equals(container));

        Assert.Multiple(() =>
        {
            Assert.That(containerResult.Key.State, Is.EqualTo(CompilationState.Finished));
            Assert.That(containerResult.Key.GetCachedQueryUseCount(), Is.EqualTo(expectedCacheUsageCount));
        });
    }

    #endregion
}