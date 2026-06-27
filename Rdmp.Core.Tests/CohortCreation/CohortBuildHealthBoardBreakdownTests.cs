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

            var w = ParseWide(File.ReadAllText(file.FullName));

            const string T = "Tayside", G = "Greater Glasgow & Clyde", F = "Fife";

            Assert.Multiple(() =>
            {
                // Total column == RDMP's own (national, non-breakdown) counts
                Assert.That(w.Cell("Root", "Final", "Total"), Is.EqualTo(58));
                Assert.That(w.Cell("Inclusion", "Final", "Total"), Is.EqualTo(100));
                Assert.That(w.Cell("Registry", "Final", "Total"), Is.EqualTo(120));
                Assert.That(w.Cell("Demography", "Final", "Total"), Is.EqualTo(100));
                Assert.That(w.Cell("Demography", "Cumulative", "Total"), Is.EqualTo(100)); // INTERSECT cumulative
                Assert.That(w.Cell("Excl1", "Cumulative", "Total"), Is.EqualTo(80));
                Assert.That(w.Cell("Excl2", "Cumulative", "Total"), Is.EqualTo(65));
                Assert.That(w.Cell("Excl3", "Cumulative", "Total"), Is.EqualTo(60));
                Assert.That(w.Cell("Excl4", "Cumulative", "Total"), Is.EqualTo(58));

                // Tayside running total down the EXCEPT chain
                Assert.That(w.Cell("Inclusion", "Final", T), Is.EqualTo(50));
                Assert.That(w.Cell("Excl1", "Cumulative", T), Is.EqualTo(40));
                Assert.That(w.Cell("Excl2", "Cumulative", T), Is.EqualTo(32));
                Assert.That(w.Cell("Excl3", "Cumulative", T), Is.EqualTo(30));
                Assert.That(w.Cell("Excl4", "Cumulative", T), Is.EqualTo(29));
                Assert.That(w.Cell("Root", "Final", T), Is.EqualTo(29));

                // Glasgow + Fife endpoints
                Assert.That(w.Cell("Root", "Final", G), Is.EqualTo(17));
                Assert.That(w.Cell("Excl4", "Cumulative", G), Is.EqualTo(17));
                Assert.That(w.Cell("Root", "Final", F), Is.EqualTo(12));
                Assert.That(w.Cell("Excl4", "Cumulative", F), Is.EqualTo(12));

                // 20 registry-only people (no demography row) -> NotKnown; Other stays 0 (no non-Scottish codes)
                Assert.That(w.Cell("Registry", "Final", "NotKnown"), Is.EqualTo(20));
                Assert.That(w.Cell("Registry", "Final", "Other"), Is.EqualTo(0));

                // % of final cohort row (root final = 58)
                Assert.That(w.Percent(T), Is.EqualTo("50.0"));
                Assert.That(w.Percent(G), Is.EqualTo("29.3"));
                Assert.That(w.Percent(F), Is.EqualTo("20.7"));
            });

            // ---- partition: every data row's columns after Total sum back to Total ----
            foreach (var (name, metric) in w.Keys)
                Assert.That(w.SumAfterTotal(name, metric), Is.EqualTo(w.Cell(name, metric, "Total")),
                    $"partition mismatch at {name}/{metric}");
        }
        finally
        {
            file.Delete();
        }
    }

    [Test]
    public void Report_Split_SeparatesScottishOtherAndNotKnown()
    {
        // Total 100; T=50, G=30 (Scottish), X=15 (present but not a Scottish board) -> Other 15;
        // NotKnown = 100 - 80 - 15 = 5 (not in demography / null region)
        var b = CohortBuildHealthBoardBreakdownReport.Split(100,
            new Dictionary<string, int> { ["T"] = 50, ["G"] = 30, ["X"] = 15 });

        Assert.Multiple(() =>
        {
            Assert.That(b.Total, Is.EqualTo(100));
            Assert.That(b.Boards["T"], Is.EqualTo(50));
            Assert.That(b.Boards["G"], Is.EqualTo(30));
            Assert.That(b.Boards.ContainsKey("X"), Is.False); // non-Scottish code is NOT a board
            Assert.That(b.Other, Is.EqualTo(15));             // it lands in Other
            Assert.That(b.NotKnown, Is.EqualTo(5));           // residual
            Assert.That(b.Boards.Values.Sum() + b.Other + b.NotKnown, Is.EqualTo(b.Total));
        });
    }

    [Test]
    public void Report_ToCsv_WideHeaderAndMetricRows()
    {
        var nodes = new List<CohortBuildHealthBoardBreakdownReport.NodeBreakdown>
        {
            new()
            {
                Seq = 0, Type = "Container", Name = "Root", Container = "", SetOperation = "EXCEPT",
                FinalUnfiltered = 80, CumulativeUnfiltered = null,
                FinalByRegion = new Dictionary<string, int> { ["T"] = 50, ["G"] = 30 }
            }
        };

        var csv = CohortBuildHealthBoardBreakdownReport.ToCsv(nodes);
        var header = csv.Split('\n')[0].Trim();

        Assert.Multiple(() =>
        {
            Assert.That(header, Does.StartWith("Order,Type,Name,Container,SetOperation,Metric,Total,"));
            Assert.That(header, Does.Contain("Tayside"));
            Assert.That(header, Does.EndWith("Other,NotKnown"));
            Assert.That(csv, Does.Contain("% of final cohort"));
        });
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

    /// <summary>Parsed wide CSV: data rows keyed by (Name-token, Metric), plus the percent row.</summary>
    private sealed class Wide
    {
        private readonly Dictionary<string, int> _col;
        private readonly List<string[]> _data = new();
        private string[] _percent;

        public Wide(string csv)
        {
            var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var header = lines[0].Split(',');
            _col = header.Select((h, i) => (h, i)).ToDictionary(x => x.h, x => x.i);
            foreach (var line in lines.Skip(1))
            {
                var c = line.Split(',');
                if (c[_col["Metric"]] == CohortBuildHealthBoardBreakdownReport.PercentMetric)
                    _percent = c;
                else
                    _data.Add(c);
            }
        }

        public IEnumerable<(string name, string metric)> Keys =>
            _data.Select(c => (c[_col["Name"]], c[_col["Metric"]]));

        private string[] Find(string nameToken, string metric) =>
            _data.Single(c => c[_col["Name"]].Contains(nameToken, StringComparison.Ordinal)
                              && c[_col["Metric"]] == metric);

        public int Cell(string nameToken, string metric, string column) =>
            int.Parse(Find(nameToken, metric)[_col[column]]);

        /// <summary>Sum of every column after Total (boards + Other + NotKnown) for a row.</summary>
        public int SumAfterTotal(string name, string metric)
        {
            var row = _data.Single(c => c[_col["Name"]] == name && c[_col["Metric"]] == metric);
            return Enumerable.Range(_col["Total"] + 1, row.Length - _col["Total"] - 1).Sum(i => int.Parse(row[i]));
        }

        public string Percent(string column) => _percent[_col[column]];
    }

    private static Wide ParseWide(string csv) => new(csv);
}
