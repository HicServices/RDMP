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
using RdmpCohortBuildBreakdownByGroups;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation;
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
public class CohortBuildBreakdownByGroupsTests : FromToDatabaseTests
{
    private static IEnumerable<string> Ids(int from, int to) =>
        Enumerable.Range(from, to - from + 1).Select(i => $"P{i:000}");

    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.PostgreSql)] // requires mac-test-env/multidb.sh up (skips if no PostgreSql line)
    [Retry(3)] // DB integration test: absorb the occasional transient SQL error under local load
    public void BuildBreakdown_Fixture_NationalAndThreeBoards(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);

        // ---- query cache: created IN THE SAME database as the data. Works on every DBMS and is
        // REQUIRED on PostgreSQL, where one connection cannot access another database ----
        var patcher = new QueryCachingPatcher();
        new MasterDatabaseScriptExecutor(db).CreateAndPatchDatabase(patcher, new AcceptAllCheckNotifier());
        var cacheServer = new ExternalDatabaseServer(CatalogueRepository, "HBQueryCache", patcher);
        cacheServer.SetProperties(db);

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

        // enabled-but-EMPTY container: under NON-strict validation RDMP's builder skips these
        // (strict validation would fail the whole build instead); our walk must match the skip
        var priorStrict = Rdmp.Core.ReusableLibraryCode.Settings.UserSettings.StrictValidationForCohortBuilderContainers;
        FileInfo file = null;
        try
        {
        Rdmp.Core.ReusableLibraryCode.Settings.UserSettings.StrictValidationForCohortBuilderContainers = false;
        var emptyC = new CohortAggregateContainer(CatalogueRepository, SetOperation.UNION) { Name = "EmptyC" };
        emptyC.SaveToDatabase();
        root.AddChild(emptyC);
        emptyC.Order = 5;

        foreach (var a in new[] { aggRegistry, aggDemography, aggExcl1, aggExcl2, aggExcl3, aggExcl4 })
            cic.EnsureNamingConvention(a);

        // ---- region lookup table (z_hb_lookup), in the same db as demography ----
        var lk = new DataTable();
        foreach (var c in new[] { "HB_9_DIGIT", "HB_Name", "HB_Code", "Region", "SafeHaven_Region" })
            lk.Columns.Add(c);
        void L(string nine, string name, object code, string region, object node) =>
            lk.Rows.Add(nine, name, code, region, node);
        L("S08000001", "Ayrshire & Arran", 11, "A", "West");
        L("S08000002", "Borders", 6, "B", "South East");
        L("S08000003", "Dumfries & Galloway", 12, "Y", "West");
        L("S08000004", "Fife", 4, "F", "East");
        L("S08000005", "Forth Valley", 7, "V", "East");
        L("S08000006", "Grampian", 2, "N", "North");
        L("S08000007", "Greater Glasgow & Clyde", 16, "G", "West");
        L("S08000008", "Highland", 17, "H", "North");
        L("S08000009", "Lanarkshire", 10, "L", "West");
        L("S08000010", "Lothian", 5, "S", "South East");
        L("S08000011", "Orkney", 13, "R", "North");
        L("S08000012", "Shetland", 14, "Z", "North");
        L("S08000013", "Tayside", 3, "T", "East");
        L("S08000014", "Western Isles", 15, "W", "North");
        L("S08200001", "England/Wales/Northern Ireland", DBNull.Value, "E", DBNull.Value);
        L("S08200002", "No Fixed Abode", DBNull.Value, "O", DBNull.Value);
        L("S08200003", "Not Known", DBNull.Value, "K", DBNull.Value);
        L("S08200004", "OutsideUK", DBNull.Value, "X", DBNull.Value);
        var lookupTbl = db.CreateTable("z_hb_lookup", lk);
        new TableInfoImporter(CatalogueRepository, lookupTbl).DoImport(out _, out var lookupCols);

        ColumnInfo LookupCol(string name) => lookupCols.Single(c =>
            c.GetRuntimeName().Equals(name, StringComparison.OrdinalIgnoreCase));

        var groupColumn = demogCata.GetAllExtractionInformation(ExtractionCategory.Any)
            .Single(e => e.GetRuntimeName().Equals("Region", StringComparison.OrdinalIgnoreCase)).ColumnInfo;

        // ---- run the command (4 columns; tables derived) ----
        file = new FileInfo(Path.GetTempFileName());
            var cmd = new ExecuteCommandExportCohortBuildBreakDownByGroups(
                new ThrowImmediatelyActivator(RepositoryLocator, null), cic, groupColumn,
                LookupCol("Region"), LookupCol("HB_Name"), file, LookupCol("SafeHaven_Region"));
            Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);
            cmd.Execute();

            // the build can be marked impossible mid-Execute if a task crashed (transient DB issue);
            // surface that clearly instead of failing later on an empty file
            Assert.That(cmd.IsImpossible, Is.False, $"build did not complete: {cmd.ReasonCommandImpossible}");
            Assert.That(file.Exists && new FileInfo(file.FullName).Length > 0, Is.True,
                "no breakdown was written (the cohort build did not finish)");

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

                // % of reference population row (BB_Demography = 100 people: T50 G30 F20) - the sanity-check reference
                Assert.That(w.DemogPercent(T), Is.EqualTo("50.0"));
                Assert.That(w.DemogPercent(G), Is.EqualTo("30.0"));
                Assert.That(w.DemogPercent(F), Is.EqualTo("20.0"));
            });

            // the enabled-but-empty container must not appear in the output
            Assert.That(w.Keys.Any(k => k.name.Contains("EmptyC", StringComparison.Ordinal)), Is.False,
                "enabled-but-empty container should be skipped like RDMP's builder does");

            // ---- partition: every data row's columns after Total sum back to Total ----
            foreach (var (name, metric) in w.Keys)
                Assert.That(w.SumAfterTotal(name, metric), Is.EqualTo(w.Cell(name, metric, "Total")),
                    $"partition mismatch at {name}/{metric}");
        }
        finally
        {
            Rdmp.Core.ReusableLibraryCode.Settings.UserSettings.StrictValidationForCohortBuilderContainers = priorStrict;
            file?.Delete();
        }
    }

    [Test]
    public void GroupLookup_LoadFrom_RejectsDuplicateKeys()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        var dt = new DataTable();
        dt.Columns.Add("code");
        dt.Columns.Add("label");
        dt.Rows.Add("T", "Tayside");
        dt.Rows.Add("t", "Tayside duplicate"); // same key differing only by case
        var tbl = db.CreateTable("dup_lookup", dt);

        Assert.That(() => GroupLookup.LoadFrom(tbl, "code", "label", null, 30),
            Throws.ArgumentException.With.Message.Contain("duplicate key"));
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
        private string[] _demogPercent;

        public Wide(string csv)
        {
            var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var header = lines[0].Split(',');
            _col = header.Select((h, i) => (h, i)).ToDictionary(x => x.h, x => x.i);
            foreach (var line in lines.Skip(1))
            {
                var c = line.Split(',');
                if (c[0] == "Order")
                    continue; // the header is repeated just above the percentage rows
                if (c[_col["Metric"]] == CohortBuildBreakdownByGroupsReport.PercentMetric)
                    _percent = c;
                else if (c[_col["Metric"]] == CohortBuildBreakdownByGroupsReport.ReferencePercentMetric)
                    _demogPercent = c;
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

        public string DemogPercent(string column) => _demogPercent[_col[column]];
    }

    private static Wide ParseWide(string csv) => new(csv);
}

/// <summary>No-database tests for the wide report projection and the name cleaner.</summary>
public class CohortBuildBreakdownByGroupsReportTests
{
    // lookup recognising T and G but not X (so X -> Other)
    private static GroupLookup Lookup() => new(new Dictionary<string, (string, string)>
    {
        ["T"] = ("Tayside", "East"),
        ["G"] = ("Greater Glasgow & Clyde", "West")
    });

    [Test]
    public void Split_SeparatesRecognisedOtherAndNotKnown()
    {
        // Total 100; T=50, G=30 (recognised), X=15 (present but not in the lookup) -> Other 15;
        // NotKnown = 100 - 80 - 15 = 5 (not in demography / null region)
        var b = CohortBuildBreakdownByGroupsReport.Split(100,
            new Dictionary<string, int> { ["T"] = 50, ["G"] = 30, ["X"] = 15 }, Lookup());

        Assert.Multiple(() =>
        {
            Assert.That(b.Total, Is.EqualTo(100));
            Assert.That(b.Groups["T"], Is.EqualTo(50));
            Assert.That(b.Groups["G"], Is.EqualTo(30));
            Assert.That(b.Groups.ContainsKey("X"), Is.False); // unrecognised code is NOT a region column
            Assert.That(b.Other, Is.EqualTo(15));              // it lands in Other
            Assert.That(b.NotKnown, Is.EqualTo(5));            // residual
            Assert.That(b.Groups.Values.Sum() + b.Other + b.NotKnown, Is.EqualTo(b.Total));
        });
    }

    [Test]
    public void ToCsv_WideHeaderAndMetricRows()
    {
        var nodes = new List<CohortBuildBreakdownNode>
        {
            new(0, "Container", "Root", "", "EXCEPT", 0, 80, null,
                new Dictionary<string, int> { ["T"] = 50, ["G"] = 30 }, null)
        };

        var csv = CohortBuildBreakdownByGroupsReport.ToCsv(nodes, Lookup());
        var header = csv.Split('\n')[0].Trim();

        Assert.Multiple(() =>
        {
            Assert.That(header, Does.StartWith("Order,Type,Name,Container,SetOperation,Metric,Total,"));
            Assert.That(header, Does.Contain("Tayside"));
            Assert.That(header, Does.EndWith("Other,NotKnown"));
            Assert.That(csv, Does.Contain("% of final cohort"));
        });
    }

    [Test]
    public void PostgreSqlSameDatabaseError_NormalizesWrappedNames()
    {
        var syntax = FAnsi.Implementations.PostgreSql.PostgreSqlSyntaxHelper.Instance;
        string E(string cache, string reference) => ExecuteCommandExportCohortBuildBreakDownByGroups
            .PostgreSqlSameDatabaseError(cache, reference, syntax);

        Assert.Multiple(() =>
        {
            // RDMP stores TableInfo.Database wrapped ("mydb") but the cache database unwrapped
            Assert.That(E("mydb", "\"mydb\""), Is.Null, "wrapped vs plain same name must be accepted");
            Assert.That(E("mydb", "mydb"), Is.Null);
            Assert.That(E("cache_db", "\"other_db\""), Is.Not.Null, "different databases must be rejected");
            Assert.That(E("Foo", "foo"), Is.Not.Null, "quoted PostgreSQL identifiers are case-sensitive");
            Assert.That(E("", "mydb"), Is.Not.Null, "blank database name must be rejected clearly");
            Assert.That(E("mydb", null), Is.Not.Null);
        });
    }

    [Test]
    public void CleanName_StripsStackedCicPrefixes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ExecuteCommandExportCohortBuildBreakDownByGroups.CleanName(
                    "cic_18286_cic_18284_cic_17950_cic_16459_People in SHARE Current For Contact [Recruitment]"),
                Is.EqualTo("People in SHARE Current For Contact [Recruitment]"));
            Assert.That(ExecuteCommandExportCohortBuildBreakDownByGroups.CleanName(
                    "cic_18286_cic_18284_cic_17950_Excl Grp 1 and 2"),
                Is.EqualTo("Excl Grp 1 and 2"));
            Assert.That(ExecuteCommandExportCohortBuildBreakDownByGroups.CleanName("Root"), Is.EqualTo("Root"));
            Assert.That(ExecuteCommandExportCohortBuildBreakDownByGroups.CleanName(null), Is.EqualTo(""));
        });
    }

    [Test]
    public void SetOperationSql_MapsExceptToMinusOnOracle()
    {
        Assert.Multiple(() =>
        {
            // Oracle spells EXCEPT as MINUS; everything else is the ANSI keyword
            Assert.That(ExecuteCommandExportCohortBuildBreakDownByGroups.SetOperationSql(
                SetOperation.EXCEPT, DatabaseType.Oracle), Is.EqualTo("MINUS"));
            Assert.That(ExecuteCommandExportCohortBuildBreakDownByGroups.SetOperationSql(
                SetOperation.EXCEPT, DatabaseType.MicrosoftSQLServer), Is.EqualTo("EXCEPT"));
            Assert.That(ExecuteCommandExportCohortBuildBreakDownByGroups.SetOperationSql(
                SetOperation.EXCEPT, DatabaseType.PostgreSql), Is.EqualTo("EXCEPT"));
            Assert.That(ExecuteCommandExportCohortBuildBreakDownByGroups.SetOperationSql(
                SetOperation.UNION, DatabaseType.Oracle), Is.EqualTo("UNION"));
            Assert.That(ExecuteCommandExportCohortBuildBreakDownByGroups.SetOperationSql(
                SetOperation.INTERSECT, DatabaseType.Oracle), Is.EqualTo("INTERSECT"));
        });
    }

    [Test]
    public void GroupLookup_RejectsDuplicateAndReservedLabels()
    {
        // duplicate label: two codes would render as identically named columns
        var dup = new Dictionary<string, (string, string)> { ["A"] = ("Same", "X"), ["B"] = ("same", "Y") };
        Assert.That(() => new GroupLookup(dup), Throws.ArgumentException.With.Message.Contain("duplicate label"));

        // reserved label collides with a fixed report header
        var reserved = new Dictionary<string, (string, string)> { ["A"] = ("Total", null) };
        Assert.That(() => new GroupLookup(reserved), Throws.ArgumentException.With.Message.Contain("collides"));

        // clean lookup constructs fine, NULL grouping allowed
        Assert.That(() => new GroupLookup(new Dictionary<string, (string, string)>
            { ["A"] = ("Ayrshire", "West"), ["E"] = ("England", null) }), Throws.Nothing);
    }

    [Test]
    public void IsTransformedIdentifier_WhitelistsPlainColumnReferencesOnly()
    {
        var syntax = FAnsi.Implementations.MicrosoftSQL.MicrosoftQuerySyntaxHelper.Instance;
        bool T(string sql) => ExecuteCommandExportCohortBuildBreakDownByGroups
            .IsTransformedIdentifier(sql, "chi", syntax);

        Assert.Multiple(() =>
        {
            // anything that is not a plain reference to the physical column is transformed
            Assert.That(T("UPPER(chi)"), Is.True);
            Assert.That(T("chi + '0'"), Is.True);
            Assert.That(T("chi - 1"), Is.True);
            Assert.That(T("chi * 1"), Is.True);
            Assert.That(T("CASE WHEN x THEN y END"), Is.True);
            Assert.That(T("other_column"), Is.True);
            // an alias only renames the output column - judge the UNDERLYING expression
            Assert.That(T("UPPER(chi) AS chi"), Is.True,   "transform hidden behind an alias must be rejected");
            Assert.That(T("[db]..[tbl].[chi] AS PatientId"), Is.False, "aliased plain column is safe (values are raw chi)");
            // plain (possibly qualified) references pass
            Assert.That(T("[db]..[tbl].[chi]"), Is.False);
            Assert.That(T("chi"), Is.False);
            Assert.That(T(null), Is.False);
        });
    }
}
