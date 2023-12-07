// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCreation.QueryTests;

public class CohortQueryBuilderWithCacheTests : CohortIdentificationTests
{
    protected DiscoveredDatabase queryCacheDatabase;
    protected ExternalDatabaseServer externalDatabaseServer;
    protected DatabaseColumnRequest _chiColumnSpecification = new("chi", "varchar(10)");

    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        queryCacheDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(
            $"{TestDatabaseNames.Prefix}QueryCache");

        if (queryCacheDatabase.Exists())
            DeleteTables(queryCacheDatabase);

        var executor = new MasterDatabaseScriptExecutor(queryCacheDatabase);

        var p = new QueryCachingPatcher();
        executor.CreateAndPatchDatabase(p, new AcceptAllCheckNotifier());

        externalDatabaseServer = new ExternalDatabaseServer(CatalogueRepository, "QueryCacheForUnitTests", p);
        externalDatabaseServer.SetProperties(queryCacheDatabase);
    }

    [Test]
    public void TestGettingAggregateJustFromConfig_DistinctCHISelect()
    {
        var manager = new CachedAggregateConfigurationResultsManager(externalDatabaseServer);

        cohortIdentificationConfiguration.QueryCachingServer_ID = externalDatabaseServer.ID;
        cohortIdentificationConfiguration.SaveToDatabase();


        cohortIdentificationConfiguration.CreateRootContainerIfNotExists();
        cohortIdentificationConfiguration.RootCohortAggregateContainer.AddChild(aggregate1, 0);

        var builder = new CohortQueryBuilder(cohortIdentificationConfiguration, null);
        try
        {
            Assert.That(
                CollapseWhitespace(builder.SQL), Is.EqualTo(CollapseWhitespace(
                    string.Format(
                        @"
(
	/*cic_{0}_UnitTestAggregate1*/
	SELECT
	distinct
	[" + TestDatabaseNames.Prefix + @"ScratchArea].[dbo].[BulkData].[chi]
	FROM 
	[" + TestDatabaseNames.Prefix + @"ScratchArea].[dbo].[BulkData]
)
", cohortIdentificationConfiguration.ID))));

            var server = queryCacheDatabase.Server;
            using (var con = server.GetConnection())
            {
                con.Open();

                var da = server.GetDataAdapter(builder.SQL, con);
                var dt = new DataTable();
                da.Fill(dt);

                manager.CommitResults(new CacheCommitIdentifierList(aggregate1,
                    string.Format(@"/*cic_{0}_UnitTestAggregate1*/
SELECT
distinct
[" + TestDatabaseNames.Prefix + @"ScratchArea].[dbo].[BulkData].[chi]
FROM 
[" + TestDatabaseNames.Prefix + @"ScratchArea].[dbo].[BulkData]", cohortIdentificationConfiguration.ID), dt,
                    _chiColumnSpecification, 30));
            }


            var builderCached = new CohortQueryBuilder(cohortIdentificationConfiguration, null);

            Assert.That(
                CollapseWhitespace(builderCached.SQL), Is.EqualTo(CollapseWhitespace(
                    string.Format(
                        @"
(
	/*Cached:cic_{0}_UnitTestAggregate1*/
	select * from [" + queryCacheDatabase.GetRuntimeName() +
                        "]..[IndexedExtractionIdentifierList_AggregateConfiguration" + aggregate1.ID + @"]

)
", cohortIdentificationConfiguration.ID))));
        }
        finally
        {
            cohortIdentificationConfiguration.RootCohortAggregateContainer.RemoveChild(aggregate1);
        }
    }
}