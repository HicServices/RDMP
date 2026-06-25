// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CohortCreation;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.CohortCreation;

/// <summary>
/// End-to-end validation of the per-health-board build breakdown on the deterministic fixture in
/// proposals/cohort-healthboard-breakdown/BUILD-BREAKDOWN-TEST-FIXTURE.md: a top EXCEPT over an
/// inclusion INTERSECT (Registry 120 ∩ Demography 100 = 100) minus four exclusion sets, with the
/// cohort partitioned across 3 boards (1 patient ↔ 1 board). Requires the test SQL Server + a query
/// cache (see mac-test-env).
/// </summary>
public class CohortBuildHealthBoardBreakdownTests : FromToDatabaseTests
{
    private static IEnumerable<string> Ids(int from, int to) =>
        Enumerable.Range(from, to - from + 1).Select(i => $"P{i:000}");

    [Test]
    public void BuildBreakdown_Fixture_NationalAndThreeBoards()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        // ---- query cache ----
        var cacheDb = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(
            $"{TestDatabaseNames.Prefix}QueryCacheHB");
        if (cacheDb.Exists())
            DeleteTables(cacheDb);
        var patcher = new QueryCachingPatcher();
        new MasterDatabaseScriptExecutor(cacheDb).CreateAndPatchDatabase(patcher, new AcceptAllCheckNotifier());
        var cacheServer = new ExternalDatabaseServer(CatalogueRepository, "HBQueryCache", patcher);
        cacheServer.SetProperties(cacheDb);

        // ---- synthetic data (see fixture doc) ----
        var demog = new DataTable();
        demog.Columns.Add("chi");
        demog.Columns.Add("Region");
        foreach (var chi in Ids(1, 50)) demog.Rows.Add(chi, "T");   // Tayside
        foreach (var chi in Ids(51, 80)) demog.Rows.Add(chi, "G");  // Greater Glasgow & Clyde
        foreach (var chi in Ids(81, 100)) demog.Rows.Add(chi, "F"); // Fife

        var registry = OneCol("chi", Ids(1, 120));                  // 100 + 20 registry-only (no region)
        var excl1 = OneCol("chi", Ids(1, 10).Concat(Ids(51, 56)).Concat(Ids(81, 84)));   // 10/6/4 = 20
        var excl2 = OneCol("chi", Ids(11, 18).Concat(Ids(57, 60)).Concat(Ids(85, 87)));  // 8/4/3 = 15
        var excl3 = OneCol("chi", Ids(19, 20).Concat(Ids(61, 62)).Concat(Ids(88, 88)));  // 2/2/1 = 5
        var excl4 = OneCol("chi", Ids(21, 21).Concat(Ids(63, 63)));                        // 1/1/0 = 2

        var aggDemography = MakeSet(db, "BB_Demography", demog, out var demogCata);
        var aggRegistry = MakeSet(db, "BB_Registry", registry, out _);
        var aggExcl1 = MakeSet(db, "BB_Excl1", excl1, out _);
        var aggExcl2 = MakeSet(db, "BB_Excl2", excl2, out _);
        var aggExcl3 = MakeSet(db, "BB_Excl3", excl3, out _);
        var aggExcl4 = MakeSet(db, "BB_Excl4", excl4, out _);

        // ---- CIC tree: ROOT(EXCEPT)[ Inclusion(INTERSECT)[Registry, Demography], Excl1..4 ] ----
        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "BB_BuildTest")
        {
            QueryCachingServer_ID = cacheServer.ID
        };
        var root = new CohortAggregateContainer(CatalogueRepository, SetOperation.EXCEPT) { Name = "Root" };
        root.SaveToDatabase();
        cic.RootCohortAggregateContainer_ID = root.ID;
        cic.SaveToDatabase();

        var incl = new CohortAggregateContainer(CatalogueRepository, SetOperation.INTERSECT) { Name = "Inclusion", Order = 0 };
        incl.SaveToDatabase();
        root.AddChild(incl);
        incl.AddChild(aggRegistry, 0);
        incl.AddChild(aggDemography, 1);
        root.AddChild(aggExcl1, 1);
        root.AddChild(aggExcl2, 2);
        root.AddChild(aggExcl3, 3);
        root.AddChild(aggExcl4, 4);

        // AddChild inserts at the top, so set the intended order explicitly (persists via SetOrder)
        incl.Order = 0;
        aggExcl1.Order = 1;
        aggExcl2.Order = 2;
        aggExcl3.Order = 3;
        aggExcl4.Order = 4;
        aggRegistry.Order = 0;
        aggDemography.Order = 1;

        foreach (var a in new[] { aggRegistry, aggDemography, aggExcl1, aggExcl2, aggExcl3, aggExcl4 })
            cic.EnsureNamingConvention(a);

        // ---- run the command ----
        var file = new FileInfo(Path.GetTempFileName());
        try
        {
            var cmd = new ExecuteCommandExportCohortBuildHealthBoardBreakdown(
                new ThrowImmediatelyActivator(RepositoryLocator, null), cic, file, demogCata.Name, "Region");
            Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);
            cmd.Execute();

            var rows = ParseLong(File.ReadAllText(file.FullName));

            LongRow Row(string board, string token) =>
                rows.Single(r => r.Board == board && r.Name.Contains(token, StringComparison.Ordinal));

            const string T = "Tayside", G = "Greater Glasgow & Clyde", F = "Fife";

            Assert.Multiple(() =>
            {
                // unfiltered tree (must equal the UI / RDMP's own counts)
                Assert.That(Row("Unfiltered", "Root").Final, Is.EqualTo(58));
                Assert.That(Row("Unfiltered", "Inclusion").Final, Is.EqualTo(100));
                Assert.That(Row("Unfiltered", "Registry").Final, Is.EqualTo(120));
                Assert.That(Row("Unfiltered", "Demography").Final, Is.EqualTo(100));
                Assert.That(Row("Unfiltered", "Demography").Cum, Is.EqualTo(100));   // INTERSECT cumulative
                Assert.That(Row("Unfiltered", "Excl1").Cum, Is.EqualTo(80));
                Assert.That(Row("Unfiltered", "Excl2").Cum, Is.EqualTo(65));
                Assert.That(Row("Unfiltered", "Excl3").Cum, Is.EqualTo(60));
                Assert.That(Row("Unfiltered", "Excl4").Cum, Is.EqualTo(58));

                // Tayside cumulative through the tree
                Assert.That(Row(T, "Inclusion").Final, Is.EqualTo(50));
                Assert.That(Row(T, "Excl1").Cum, Is.EqualTo(40));
                Assert.That(Row(T, "Excl2").Cum, Is.EqualTo(32));
                Assert.That(Row(T, "Excl3").Cum, Is.EqualTo(30));
                Assert.That(Row(T, "Excl4").Cum, Is.EqualTo(29));
                Assert.That(Row(T, "Root").Final, Is.EqualTo(29));

                // Glasgow + Fife endpoints
                Assert.That(Row(G, "Root").Final, Is.EqualTo(17));
                Assert.That(Row(G, "Excl4").Cum, Is.EqualTo(17));
                Assert.That(Row(F, "Root").Final, Is.EqualTo(12));
                Assert.That(Row(F, "Excl4").Cum, Is.EqualTo(12));

                // the 20 registry-only people (no demography row) land in Unknown on the Registry set
                Assert.That(Row("Unknown", "Registry").Final, Is.EqualTo(20));
            });

            // ---- partition: at EVERY node, the boards (+Unknown) sum to the unfiltered total ----
            foreach (var node in rows.GroupBy(r => (r.Name, r.Order, r.Container)))
            {
                var unfiltered = node.Single(r => r.Board == "Unfiltered");
                var boardFinal = node.Where(r => r.Board != "Unfiltered").Sum(r => r.Final ?? 0);
                Assert.That(boardFinal, Is.EqualTo(unfiltered.Final),
                    $"Final partition mismatch at node '{unfiltered.Name}'");

                if (unfiltered.Cum.HasValue)
                {
                    var boardCum = node.Where(r => r.Board != "Unfiltered").Sum(r => r.Cum ?? 0);
                    Assert.That(boardCum, Is.EqualTo(unfiltered.Cum),
                        $"Cumulative partition mismatch at node '{unfiltered.Name}'");
                }
            }
        }
        finally
        {
            file.Delete();
        }
    }

    [Test]
    public void Report_BuildRows_UnfilteredFirst_UnknownDerived_BoardMajor()
    {
        // one node: unfiltered 100, T=50 G=30 (known), so Unknown = 100 - 80 = 20
        var nodes = new List<CohortBuildHealthBoardBreakdownReport.NodeBreakdown>
        {
            new()
            {
                Seq = 0, Type = "Cohort Set", Name = "Set1", Container = "Root", SetOperation = "",
                FinalUnfiltered = 100, CumulativeUnfiltered = null,
                FinalByRegion = new Dictionary<string, int> { ["T"] = 50, ["G"] = 30 }
            }
        };

        var rows = CohortBuildHealthBoardBreakdownReport.BuildRows(nodes);

        Assert.Multiple(() =>
        {
            Assert.That(rows[0].Board, Is.EqualTo("Unfiltered"));
            Assert.That(rows[0].FinalCount, Is.EqualTo(100));
            Assert.That(rows.Single(r => r.Board == "Tayside").FinalCount, Is.EqualTo(50));
            Assert.That(rows.Single(r => r.Board == "Greater Glasgow & Clyde").FinalCount, Is.EqualTo(30));
            Assert.That(rows.Last().Board, Is.EqualTo("Unknown"));
            Assert.That(rows.Last().FinalCount, Is.EqualTo(20)); // 100 - (50+30)
            // partition holds
            Assert.That(rows.Where(r => r.Board != "Unfiltered").Sum(r => r.FinalCount ?? 0), Is.EqualTo(100));
        });

        var csv = CohortBuildHealthBoardBreakdownReport.ToCsv(rows);
        Assert.That(csv.Split('\n')[0].Trim(),
            Is.EqualTo("Board,Node,Order,Type,Name,Container,SetOperation,FinalCount,CumulativeCount"));
    }

    private static DataTable OneCol(string col, IEnumerable<string> values)
    {
        var dt = new DataTable();
        dt.Columns.Add(col);
        foreach (var v in values) dt.Rows.Add(v);
        return dt;
    }

    private AggregateConfiguration MakeSet(DiscoveredDatabase db, string name, DataTable data, out ICatalogue cata)
    {
        var tbl = db.CreateTable(name, data);
        cata = Import(tbl);
        var chi = cata.GetAllExtractionInformation(ExtractionCategory.Any)
            .Single(e => e.GetRuntimeName().Equals("chi", StringComparison.OrdinalIgnoreCase));
        chi.IsExtractionIdentifier = true;
        chi.SaveToDatabase();

        var agg = new AggregateConfiguration(CatalogueRepository, cata, name) { CountSQL = null };
        agg.SaveToDatabase();
        _ = new AggregateDimension(CatalogueRepository, chi, agg);
        return agg;
    }

    private sealed record LongRow(string Board, string Node, int Order, string Type, string Name,
        string Container, string SetOp, int? Final, int? Cum);

    private static List<LongRow> ParseLong(string csv)
    {
        var rows = new List<LongRow>();
        foreach (var line in csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
        {
            var c = line.Split(',');
            int? N(string s) => string.IsNullOrEmpty(s) ? null : int.Parse(s);
            rows.Add(new LongRow(c[0], c[1], int.Parse(c[2]), c[3], c[4], c[5], c[6], N(c[7]), N(c[8])));
        }

        return rows;
    }
}
