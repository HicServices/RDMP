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
using NUnit.Framework;
using Rdmp.Core.CohortCreation;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.CohortCreation;

/// <summary>
/// No-database unit tests for the health board lookup, the report projection (ordering, subtotals,
/// unknown bucket, CSV escaping) and the breakdown SQL.
/// </summary>
public class HealthBoardBreakdownReportTests
{
    [Test]
    public void Lookup_KnownAndUnknownRegions()
    {
        Assert.Multiple(() =>
        {
            var tayside = HealthBoardLookup.Resolve("T");
            Assert.That(tayside.Name, Is.EqualTo("Tayside"));
            Assert.That(tayside.HbCode, Is.EqualTo(3));
            Assert.That(tayside.Node, Is.EqualTo("East"));

            // case-insensitive + trimmed
            Assert.That(HealthBoardLookup.Resolve(" g ").Name, Is.EqualTo("Greater Glasgow & Clyde"));

            // legacy board has no numeric code but is still a real board
            var clyde = HealthBoardLookup.Resolve("C");
            Assert.That(clyde.Name, Is.EqualTo("Clyde"));
            Assert.That(clyde.HbCode, Is.Null);
            Assert.That(clyde.Node, Is.EqualTo("West"));

            // unmapped + null both land in the Unknown node, never null
            Assert.That(HealthBoardLookup.Resolve("Q").Node, Is.EqualTo(HealthBoardLookup.UnknownNode));
            Assert.That(HealthBoardLookup.Resolve(null).Node, Is.EqualTo(HealthBoardLookup.UnknownNode));
        });
    }

    [Test]
    public void BuildRecords_GroupsByNode_Subtotals_UnknownLast_AndGrandTotal()
    {
        var dt = new DataTable();
        dt.Columns.Add("Region");
        dt.Columns.Add("n", typeof(int));
        dt.Rows.Add("T", 3); // Tayside  (East)
        dt.Rows.Add("F", 2); // Fife     (East)
        dt.Rows.Add("G", 4); // Glasgow  (West)
        dt.Rows.Add("Q", 1); // unknown cipher
        dt.Rows.Add(DBNull.Value, 2); // NULL region -> Unknown (folds with Q)

        var records = HealthBoardBreakdownReport.BuildRecords(dt);

        // Unknown node must come last
        var nodeOrder = records
            .Where(r => r.Kind == HealthBoardBreakdownReport.RowKind.NodeSubtotal)
            .Select(r => r.Node)
            .ToList();
        Assert.That(nodeOrder, Is.EqualTo(new[] { "East", "West", HealthBoardLookup.UnknownNode }));

        int Subtotal(string node) => records.Single(r =>
            r.Kind == HealthBoardBreakdownReport.RowKind.NodeSubtotal && r.Node == node).Count;
        int Board(string name) => records.Single(r =>
            r.Kind == HealthBoardBreakdownReport.RowKind.Board && r.HbName == name).Count;

        Assert.Multiple(() =>
        {
            Assert.That(Board("Tayside"), Is.EqualTo(3));
            Assert.That(Board("Fife"), Is.EqualTo(2));
            Assert.That(Board("Greater Glasgow & Clyde"), Is.EqualTo(4));

            Assert.That(Subtotal("East"), Is.EqualTo(5));
            Assert.That(Subtotal("West"), Is.EqualTo(4));
            // unmapped 'Q' (1) + NULL (2) fold into a single Unknown bucket
            Assert.That(Subtotal(HealthBoardLookup.UnknownNode), Is.EqualTo(3));

            // grand total = sum of all
            var total = records.Single(r => r.Kind == HealthBoardBreakdownReport.RowKind.GrandTotal);
            Assert.That(total.Count, Is.EqualTo(12));

            // a single Unknown board row, not one per cipher
            Assert.That(records.Count(r => r.Kind == HealthBoardBreakdownReport.RowKind.Board
                                           && r.Node == HealthBoardLookup.UnknownNode), Is.EqualTo(1));
        });
    }

    [Test]
    public void ToCsv_HeaderAndEscaping()
    {
        var records = new List<HealthBoardBreakdownReport.BreakdownRecord>
        {
            new() { Kind = HealthBoardBreakdownReport.RowKind.Board, Region = "T", HbCode = 3, HbName = "Tayside", Node = "East", Count = 3 },
            new() { Kind = HealthBoardBreakdownReport.RowKind.Board, Region = "C", HbCode = null, HbName = "Clyde", Node = "West", Count = 1 }
        };

        var csv = HealthBoardBreakdownReport.ToCsv(records);
        var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        Assert.Multiple(() =>
        {
            Assert.That(lines[0], Is.EqualTo("Region,HBCode,HBName,Node,Count"));
            Assert.That(lines[1], Is.EqualTo("T,3,Tayside,East,3"));
            // null HbCode renders as an empty field, not "0"
            Assert.That(lines[2], Is.EqualTo("C,,Clyde,West,1"));
        });
    }

    [Test]
    public void BuildBreakdownSql_ContainsJoinAndGroupBy()
    {
        var sql = ExecuteCommandExportCohortHealthBoardBreakdown.BuildBreakdownSql(
            regionSelect: "[d]..[Demography].[Region]",
            chiSelect: "[d]..[Demography].[chi]",
            demographyTable: "[d]..[Demography]",
            cohortIdSubquery: "SELECT [c]..[Cohort].[PrivateID] FROM [c]..[Cohort] WHERE [c]..[Cohort].[cohortDefinition_id]=42");

        Assert.Multiple(() =>
        {
            Assert.That(sql, Does.Contain("COUNT(DISTINCT [d]..[Demography].[chi])"));
            Assert.That(sql, Does.Contain("AS Region"));
            Assert.That(sql, Does.Contain("AS n"));
            Assert.That(sql, Does.Contain("[d]..[Demography].[chi] IN ("));
            Assert.That(sql, Does.Contain("WHERE [c]..[Cohort].[cohortDefinition_id]=42"));
            Assert.That(sql, Does.Contain("GROUP BY [d]..[Demography].[Region]"));
        });
    }
}

/// <summary>
/// End-to-end against the test SQL Server: a synthetic SHARE_Demography table is joined to a real
/// committed cohort and the exported breakdown is checked for distinct-patient counts, node
/// subtotals, the Unknown bucket, exclusion of non-cohort patients and total reconciliation.
/// Requires the test SQL Server (see mac-test-env).
/// </summary>
public class HealthBoardBreakdownDatabaseTests : TestsRequiringACohort
{
    [Test]
    public void Breakdown_RealCohort_CountsPerBoardNodeAndReconciles()
    {
        // cohort private ids seeded by TestsRequiringACohort:
        //   Priv_12345, Priv_66666, Priv_54321, Priv_66999, Priv_14722, Priv_wtf11  (6 patients)
        var demo = new DataTable();
        demo.Columns.Add("chi");
        demo.Columns.Add("Region");
        demo.Rows.Add("Priv_12345", "T"); // Tayside (East)
        demo.Rows.Add("Priv_12345", "T"); // duplicate -> proves COUNT(DISTINCT)
        demo.Rows.Add("Priv_66666", "T"); // Tayside
        demo.Rows.Add("Priv_54321", "G"); // Glasgow (West)
        demo.Rows.Add("Priv_66999", "G"); // Glasgow
        demo.Rows.Add("Priv_14722", "Q"); // unmapped cipher -> Unknown
        demo.Rows.Add("OUTSIDER_1", "T"); // not in the cohort -> must be excluded
        // Priv_wtf11 deliberately absent from demography -> reconciliation shortfall of 1

        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var demoTbl = db.CreateTable("SHARE_Demography", demo);

        var cata = Import(demoTbl);
        var chiEi = cata.GetAllExtractionInformation(ExtractionCategory.Any)
            .Single(e => e.GetRuntimeName().Equals("chi", StringComparison.OrdinalIgnoreCase));
        chiEi.IsExtractionIdentifier = true;
        chiEi.SaveToDatabase();

        var file = new FileInfo(Path.GetTempFileName());
        try
        {
            var cmd = new ExecuteCommandExportCohortHealthBoardBreakdown(
                new ThrowImmediatelyActivator(RepositoryLocator, null),
                (ExtractableCohort_DataExport())!, file, cata.Name, "Region");

            Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);
            cmd.Execute();

            var counts = ParseCsv(File.ReadAllText(file.FullName));

            Assert.Multiple(() =>
            {
                Assert.That(counts["Tayside"], Is.EqualTo(2)); // distinct: dup folded
                Assert.That(counts["Greater Glasgow & Clyde"], Is.EqualTo(2));
                Assert.That(counts["East - total"], Is.EqualTo(2));
                Assert.That(counts["West - total"], Is.EqualTo(2));
                Assert.That(counts[$"{HealthBoardLookup.UnknownNode} - total"], Is.EqualTo(1)); // 'Q'
                Assert.That(counts["TOTAL"], Is.EqualTo(5)); // 5 of 6 matched; OUTSIDER excluded
            });
        }
        finally
        {
            file.Delete();
        }
    }

    // small accessor to the protected IExtractableCohort as a concrete ExtractableCohort
    private global::Rdmp.Core.DataExport.Data.ExtractableCohort ExtractableCohort_DataExport() =>
        _extractableCohort as global::Rdmp.Core.DataExport.Data.ExtractableCohort;

    /// <summary>Parses the breakdown CSV into HBName -> Count (board names carry no commas).</summary>
    internal static Dictionary<string, int> ParseCsv(string csv)
    {
        var result = new Dictionary<string, int>();
        var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines.Skip(1)) // skip header
        {
            var cells = line.Split(',');
            // Region,HBCode,HBName,Node,Count
            result[cells[2]] = int.Parse(cells[4]);
        }

        return result;
    }
}

/// <summary>
/// End-to-end for the live <see cref="CohortIdentificationConfiguration"/> input: a cohort is built
/// over a synthetic patients catalogue and broken down against a separate synthetic SHARE_Demography
/// table on the same server. Requires the test SQL Server (see mac-test-env).
/// </summary>
public class HealthBoardBreakdownCicDatabaseTests : DatabaseTests
{
    [Test]
    public void Breakdown_FromCic_CountsPerBoardAndReconciles()
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);

        // patients catalogue = the cohort source (everyone here is in the cohort)
        var patients = new DataTable();
        patients.Columns.Add("chi");
        foreach (var chi in new[] { "c1", "c2", "c3", "c4", "c6" })
            patients.Rows.Add(chi);
        var patientsTbl = db.CreateTable("HBPatients", patients);
        var patientsCata = Import(patientsTbl);
        var patientChi = patientsCata.GetAllExtractionInformation(ExtractionCategory.Any)
            .Single(e => e.GetRuntimeName().Equals("chi", StringComparison.OrdinalIgnoreCase));
        patientChi.IsExtractionIdentifier = true;
        patientChi.SaveToDatabase();

        // demography: maps some patients to boards; c6 absent (-> shortfall), cX not in cohort
        var demo = new DataTable();
        demo.Columns.Add("chi");
        demo.Columns.Add("Region");
        demo.Rows.Add("c1", "T"); // Tayside (East)
        demo.Rows.Add("c2", "T"); // Tayside
        demo.Rows.Add("c3", "G"); // Glasgow (West)
        demo.Rows.Add("c4", "Q"); // unmapped -> Unknown
        demo.Rows.Add("cX", "T"); // not in cohort -> excluded
        var demoTbl = db.CreateTable("SHARE_Demography", demo);
        var demoCata = Import(demoTbl);
        demoCata.GetAllExtractionInformation(ExtractionCategory.Any)
            .Single(e => e.GetRuntimeName().Equals("chi", StringComparison.OrdinalIgnoreCase))
            .IsExtractionIdentifier = true;
        foreach (var e in demoCata.GetAllExtractionInformation(ExtractionCategory.Any)
                     .Where(e => e.GetRuntimeName().Equals("chi", StringComparison.OrdinalIgnoreCase)))
            e.SaveToDatabase();

        // build a single-set CIC selecting chi from the patients catalogue
        var agg = new AggregateConfiguration(CatalogueRepository, patientsCata, "HBCohortSet") { CountSQL = null };
        agg.SaveToDatabase();
        _ = new AggregateDimension(CatalogueRepository, patientChi, agg);

        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "HBCic");
        cic.CreateRootContainerIfNotExists();
        cic.RootCohortAggregateContainer.AddChild(agg, 0);
        cic.EnsureNamingConvention(agg);

        var file = new FileInfo(Path.GetTempFileName());
        try
        {
            var cmd = new ExecuteCommandExportCicHealthBoardBreakdown(
                new ThrowImmediatelyActivator(RepositoryLocator, null),
                cic, file, demoCata.Name, "Region");

            Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);
            cmd.Execute();

            var counts = HealthBoardBreakdownDatabaseTests.ParseCsv(File.ReadAllText(file.FullName));
            Assert.Multiple(() =>
            {
                Assert.That(counts["Tayside"], Is.EqualTo(2));
                Assert.That(counts["Greater Glasgow & Clyde"], Is.EqualTo(1));
                Assert.That(counts["East - total"], Is.EqualTo(2));
                Assert.That(counts["West - total"], Is.EqualTo(1));
                Assert.That(counts[$"{HealthBoardLookup.UnknownNode} - total"], Is.EqualTo(1)); // c4='Q'
                Assert.That(counts["TOTAL"], Is.EqualTo(4)); // c6 absent from demography, cX excluded
            });
        }
        finally
        {
            file.Delete();
        }
    }
}
