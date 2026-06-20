// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortScript;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

/// <summary>
/// Round-trips a <see cref="CohortIdentificationConfiguration"/> entirely in-memory:
/// build a cohort, ExportCohortAsScript it to a build.script.yaml, BuildCohortFromScript a
/// fresh copy, and assert the rebuilt object graph matches. Exercises set operations, nested
/// AND/OR filter containers, aggregate-level parameters and patient index tables (joinables) -
/// all without a database (uses the in-memory MemoryDataExportRepository).
/// </summary>
internal class TestCohortScriptRoundTrip : UnitTests
{
    [Test]
    public void ExportThenRebuild_ComplexCohort_ProducesEquivalentGraph()
    {
        // --- arrange: three cohort-ready catalogues (each has a patient identifier + a date col)
        var cataA = MakeCohortCatalogue("CataA", out var idA, out _);
        var cataB = MakeCohortCatalogue("CataB", out var idB, out _);
        var cataC = MakeCohortCatalogue("CataC", out var idC, out var dateC);
        // a catalogue the cohort does NOT use - must be excluded from the scoped manifest
        MakeCohortCatalogue("CataUnused", out _, out _);

        // --- arrange: the original cohort
        var cic = new CohortIdentificationConfiguration(Repository, "RoundTrip Original");
        var root = new CohortAggregateContainer(Repository, SetOperation.INTERSECT);
        cic.RootCohortAggregateContainer_ID = root.ID;
        cic.SaveToDatabase();

        // a cohort-level (global) parameter
        new AnyTableSqlParameter(Repository, cic, "DECLARE @studyStart as datetime2") { Value = "'2019-01-01'" }
            .SaveToDatabase();

        // a patient index table on CataC (exposes the date column), joined by set B below.
        // The PIT carries its OWN parameter and filter (must round-trip too).
        var pit = new AggregateConfiguration(Repository, cataC, "PIT on C");
        _ = new AggregateDimension(Repository, idC, pit);
        // the date column is ALIASED - the join filter would reference ix<id>.event_date, so the
        // alias must round-trip (and the dimension must not be dropped because its runtime name
        // no longer matches any ExtractionInformation)
        var pitDateDim = new AggregateDimension(Repository, dateC, pit) { Alias = "event_date" };
        pitDateDim.SaveToDatabase();
        cic.EnsureNamingConvention(pit);
        pit.HavingSQL = "count(*) >= 1";
        new AnyTableSqlParameter(Repository, pit, "DECLARE @minStay as int") { Value = "2" }.SaveToDatabase();
        var pitFc = new AggregateFilterContainer(Repository, FilterContainerOperation.AND);
        pit.RootFilterContainer_ID = pitFc.ID;
        pit.SaveToDatabase();
        new AggregateFilter(Repository, "EmergencyOnly", pitFc) { WhereSQL = "admission_type = 'E'" }.SaveToDatabase();
        var joinable = new JoinableCohortAggregateConfiguration(Repository, cic, pit);

        // set A: CataA with a nested filter tree  (DateFilter AND (CatA OR CatB))
        var aggA = new AggregateConfiguration(Repository, cataA, "People in A");
        _ = new AggregateDimension(Repository, idA, aggA);
        cic.EnsureNamingConvention(aggA);
        root.AddChild(aggA, 0);
        var fcA = new AggregateFilterContainer(Repository, FilterContainerOperation.AND);
        aggA.RootFilterContainer_ID = fcA.ID;
        aggA.SaveToDatabase();
        _ = new AggregateFilter(Repository, "DateFilter", fcA) { WhereSQL = "MyDateCol >= '2020-01-01'" };
        var orSub = new AggregateFilterContainer(Repository, FilterContainerOperation.OR);
        fcA.AddChild(orSub);
        _ = new AggregateFilter(Repository, "CatA", orSub) { WhereSQL = "MyOtherCol LIKE 'A%'" };
        // a DISABLED filter: kept in the tree but excluded from the WHERE clause
        _ = new AggregateFilter(Repository, "CatB", orSub) { WhereSQL = "MyOtherCol LIKE 'B%'", IsDisabled = true };
        foreach (var f in fcA.GetFilters().Concat(orSub.GetFilters()))
            ((AggregateFilter)f).SaveToDatabase();
        // ...and the OR sub-container itself is DISABLED too
        orSub.IsDisabled = true;
        orSub.SaveToDatabase();
        // a filter that references a parameter. It is marked as imported from a published filter
        // (ClonedFromExtractionFilter_ID) but its WhereSQL has diverged from that master - the
        // export must reproduce this ACTUAL text + parameter, not re-import the (drifted) master.
        var codeFilter = new AggregateFilter(Repository, "CodeList", fcA)
        {
            WhereSQL = "MyOtherCol IN (@codes)",
            ClonedFromExtractionFilter_ID = 999 // a published filter whose current text differs
        };
        codeFilter.SaveToDatabase();
        var codeParam = (AggregateFilterParameter)codeFilter.GetFilterFactory()
            .CreateNewParameter(codeFilter, "DECLARE @codes AS varchar(50)");
        codeParam.Value = "'A1','B2'";
        codeParam.SaveToDatabase();

        // set B: CataB with an aggregate-level parameter and a join-use to the index table
        var aggB = new AggregateConfiguration(Repository, cataB, "People in B");
        _ = new AggregateDimension(Repository, idB, aggB);
        cic.EnsureNamingConvention(aggB);
        root.AddChild(aggB, 1);
        aggB.HavingSQL = "count(*) >= 2"; // patients appearing at least twice
        aggB.SaveToDatabase();
        new AnyTableSqlParameter(Repository, aggB, "DECLARE @window as int") { Value = "90" }.SaveToDatabase();
        var use = joinable.AddUser(aggB);
        use.JoinType = ExtractionJoinType.Inner;
        use.SaveToDatabase();

        // a forced join on set B (force a table into its query)
        var forcedTable = WhenIHaveA<TableInfo>();
        forcedTable.Name = "[ForcedDb]..[ForcedTable]";
        forcedTable.SaveToDatabase();
        Repository.AggregateForcedJoinManager.CreateLinkBetween(aggB, forcedTable);

        // a DISABLED sub-container (excluded from the query but kept in the tree), holding a set
        var cataD = MakeCohortCatalogue("CataD", out var idD, out _);
        var disabledBranch = new CohortAggregateContainer(Repository, SetOperation.UNION)
        {
            Name = "Optional extras" // custom container name - must round-trip
        };
        disabledBranch.SaveToDatabase();
        root.AddChild(disabledBranch);
        var aggD = new AggregateConfiguration(Repository, cataD, "People in D");
        _ = new AggregateDimension(Repository, idD, aggD);
        cic.EnsureNamingConvention(aggD);
        disabledBranch.AddChild(aggD, 0);
        disabledBranch.IsDisabled = true;
        disabledBranch.SaveToDatabase();

        // ...and a DISABLED direct set
        aggA.IsDisabled = true;
        aggA.SaveToDatabase();

        // --- act: export to a script, then rebuild a fresh cohort from it
        var activator = (ConsoleInputManager)GetActivator();
        activator.DisallowInput = true;

        var outDir = new DirectoryInfo(Path.Join(Path.GetTempPath(), "rdmp-cohortscript-" + System.Guid.NewGuid()));
        new ExecuteCommandExportCohortAsScript(activator, cic, outDir).Execute();

        var scriptFile = new FileInfo(Path.Join(outDir.FullName, "RoundTrip Original", "build.script.yaml"));
        Assert.That(scriptFile.Exists, $"export did not produce {scriptFile.FullName}");

        new ExecuteCommandBuildCohortFromScript(activator, scriptFile, "RoundTrip Rebuilt").Execute();

        // --- assert: the rebuilt cohort matches the original, structurally
        var rebuilt = Repository.GetAllObjectsWhere<CohortIdentificationConfiguration>("Name", "RoundTrip Rebuilt")
            .Single();
        var rebuiltRoot = rebuilt.RootCohortAggregateContainer;

        Assert.That(rebuiltRoot.Operation, Is.EqualTo(SetOperation.INTERSECT));

        var sets = rebuiltRoot.GetAggregateConfigurations();
        Assert.That(sets.Select(a => a.Catalogue.Name).OrderBy(x => x),
            Is.EquivalentTo(new[] { "CataA", "CataB" }));

        // set A: nested filter tree round-tripped
        var rA = sets.Single(a => a.Catalogue.Name == "CataA");
        var rootFc = rA.RootFilterContainer;
        Assert.Multiple(() =>
        {
            Assert.That(rootFc.Operation, Is.EqualTo(FilterContainerOperation.AND));
            Assert.That(rootFc.GetFilters().Select(f => f.WhereSQL),
                Is.EquivalentTo(new[] { "MyDateCol >= '2020-01-01'", "MyOtherCol IN (@codes)" }));
            Assert.That(rootFc.GetSubContainers(), Has.Length.EqualTo(1));
        });

        // the hand-written filter's parameter (@codes) was re-created with its value
        var rCodeFilter = rootFc.GetFilters().Single(f => f.WhereSQL == "MyOtherCol IN (@codes)");
        var rCodeParams = rCodeFilter.GetAllParameters().OfType<AggregateFilterParameter>().ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(rCodeParams.Select(p => p.ParameterName), Has.Member("@codes"));
            Assert.That(rCodeParams.Single(p => p.ParameterName == "@codes").Value, Is.EqualTo("'A1','B2'"));
        });
        var sub = rootFc.GetSubContainers().Single();
        Assert.Multiple(() =>
        {
            Assert.That(sub.Operation, Is.EqualTo(FilterContainerOperation.OR));
            Assert.That(sub.GetFilters().Select(f => f.WhereSQL),
                Is.EquivalentTo(new[] { "MyOtherCol LIKE 'A%'", "MyOtherCol LIKE 'B%'" }));
        });

        // disabled FILTER and disabled filter SUB-CONTAINER round-tripped (both are excluded from
        // the WHERE clause when disabled - losing the flag would silently change membership)
        Assert.Multiple(() =>
        {
            Assert.That(((AggregateFilterContainer)sub).IsDisabled, Is.True,
                "disabled filter sub-container should round-trip disabled");
            Assert.That(((AggregateFilterContainer)rootFc).IsDisabled, Is.False,
                "enabled root filter container should stay enabled");
            Assert.That(((AggregateFilter)sub.GetFilters().Single(f => f.Name == "CatB")).IsDisabled, Is.True,
                "disabled filter should round-trip disabled");
            Assert.That(((AggregateFilter)sub.GetFilters().Single(f => f.Name == "CatA")).IsDisabled, Is.False,
                "enabled filter should stay enabled");
        });

        // set B: aggregate parameter and join-use round-tripped
        var rB = sets.Single(a => a.Catalogue.Name == "CataB");
        var bParams = Repository.GetAllObjects<AnyTableSqlParameter>()
            .Where(p => p.ReferencedObjectType == nameof(AggregateConfiguration) && p.ReferencedObjectID == rB.ID)
            .ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(bParams, Has.Length.EqualTo(1));
            Assert.That(bParams[0].ParameterName, Is.EqualTo("@window"));
            Assert.That(bParams[0].Value, Is.EqualTo("90"));
        });

        // HAVING clause round-tripped (changes which patients the set matches)
        Assert.That(rB.HavingSQL, Is.EqualTo("count(*) >= 2"));

        // patient index table round-tripped (one joinable, used by set B with an Inner join)
        var joinables = rebuilt.GetAllJoinables();
        Assert.That(joinables, Has.Length.EqualTo(1));
        var usedBy = rB.PatientIndexJoinablesUsed;
        Assert.Multiple(() =>
        {
            Assert.That(usedBy, Has.Length.EqualTo(1));
            Assert.That(usedBy[0].JoinType, Is.EqualTo(ExtractionJoinType.Inner));
            Assert.That(usedBy[0].JoinableCohortAggregateConfiguration_ID, Is.EqualTo(joinables[0].ID));
        });

        // disabled states round-tripped: the disabled direct set, and the disabled sub-container
        // (with its set still inside it). Enabled set B stays enabled.
        var rDisabledBranch = rebuiltRoot.GetSubContainers().Single();
        Assert.Multiple(() =>
        {
            Assert.That(rA.IsDisabled, Is.True, "disabled set A should round-trip disabled");
            Assert.That(rB.IsDisabled, Is.False, "enabled set B should stay enabled");
            Assert.That(rDisabledBranch.IsDisabled, Is.True, "disabled sub-container should round-trip disabled");
            Assert.That(rDisabledBranch.GetAggregateConfigurations().Single().Catalogue.Name, Is.EqualTo("CataD"));
        });

        // container NAMES round-tripped: the custom name, and the root's name (a rebuilt root is
        // auto-named "Root Container" - the original's name must win)
        Assert.Multiple(() =>
        {
            Assert.That(rDisabledBranch.Name, Is.EqualTo("Optional extras"), "custom container name should round-trip");
            Assert.That(rebuiltRoot.Name, Is.EqualTo(root.Name), "root container name should round-trip");
        });

        // global (cohort-level) parameter round-tripped
        var globals = rebuilt.GetAllParameters();
        Assert.Multiple(() =>
        {
            Assert.That(globals.Select(p => p.ParameterName), Has.Member("@studyStart"));
            Assert.That(globals.Single(p => p.ParameterName == "@studyStart").Value, Is.EqualTo("'2019-01-01'"));
        });

        // the patient index table's OWN parameter and filter round-tripped
        var rebuiltPit = joinables[0].AggregateConfiguration;
        var pitParams = Repository.GetAllObjects<AnyTableSqlParameter>()
            .Where(p => p.ReferencedObjectType == nameof(AggregateConfiguration) && p.ReferencedObjectID == rebuiltPit.ID)
            .ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(pitParams.Select(p => p.ParameterName), Has.Member("@minStay"));
            Assert.That(rebuiltPit.RootFilterContainer, Is.Not.Null, "PIT filter container should round-trip");
            Assert.That(rebuiltPit.RootFilterContainer.GetFilters().Select(f => f.WhereSQL),
                Is.EquivalentTo(new[] { "admission_type = 'E'" }));
        });

        // the PIT's HAVING clause and its ALIASED date dimension round-tripped (the aliased
        // dimension must exist - it was previously dropped because its runtime name didn't match
        // any ExtractionInformation - and carry the alias the join filter references)
        Assert.That(rebuiltPit.HavingSQL, Is.EqualTo("count(*) >= 1"));
        var rPitDims = rebuiltPit.AggregateDimensions;
        Assert.Multiple(() =>
        {
            Assert.That(rPitDims, Has.Length.EqualTo(2), "PIT should have identifier + aliased date dimension");
            Assert.That(rPitDims.Select(d => d.GetRuntimeName()), Has.Member("event_date"));
            Assert.That(rPitDims.Single(d => d.GetRuntimeName() == "event_date").ExtractionInformation.GetRuntimeName(),
                Is.EqualTo("MyDateCol"), "alias should sit on the original underlying column");
        });

        // forced join round-tripped on set B
        Assert.That(rB.ForcedJoins.Select(t => t.Name), Has.Member("[ForcedDb]..[ForcedTable]"));

        // catalogue-manifest.yaml is scoped to ONLY the catalogues this cohort uses
        var manifest = File.ReadAllText(Path.Join(outDir.FullName, "RoundTrip Original", "catalogue-manifest.yaml"));
        Assert.Multiple(() =>
        {
            Assert.That(manifest, Does.Contain("CataA"));
            Assert.That(manifest, Does.Contain("CataB"));
            Assert.That(manifest, Does.Contain("CataC"));
            Assert.That(manifest, Does.Contain("CataD"));
            Assert.That(manifest, Does.Not.Contain("CataUnused"), "manifest must exclude catalogues the cohort doesn't use");
        });

        outDir.Delete(true);
    }

    /// <summary>
    /// When the whole-cohort SQL cannot be generated (in-memory there is no real server, which is
    /// the same failure a real cross-server cohort with no QueryCache hits), query.sql must fall
    /// back to a best-effort document: a header, each cohort set listed, and a notes footer -
    /// instead of losing everything to one "SQL generation failed" line.
    /// </summary>
    [Test]
    public void Export_WhenFullSqlCannotBeGenerated_WritesBestEffortQuery()
    {
        var cata1 = MakeCohortCatalogue("BE_One", out var id1, out _);
        var cata2 = MakeCohortCatalogue("BE_Two", out var id2, out _);

        var cic = new CohortIdentificationConfiguration(Repository, "BestEffort CIC");
        var root = new CohortAggregateContainer(Repository, SetOperation.UNION);
        cic.RootCohortAggregateContainer_ID = root.ID;
        cic.SaveToDatabase();

        var s1 = new AggregateConfiguration(Repository, cata1, "People in One");
        _ = new AggregateDimension(Repository, id1, s1);
        cic.EnsureNamingConvention(s1);
        root.AddChild(s1, 0);

        var s2 = new AggregateConfiguration(Repository, cata2, "People in Two");
        _ = new AggregateDimension(Repository, id2, s2);
        cic.EnsureNamingConvention(s2);
        root.AddChild(s2, 1);

        var activator = (ConsoleInputManager)GetActivator();
        activator.DisallowInput = true;
        var outDir = new DirectoryInfo(Path.Join(Path.GetTempPath(), "rdmp-besteffort-" + System.Guid.NewGuid()));
        new ExecuteCommandExportCohortAsScript(activator, cic, outDir).Execute();

        var sql = File.ReadAllText(Path.Join(outDir.FullName, "BestEffort CIC", "query.sql"));
        Assert.Multiple(() =>
        {
            Assert.That(sql, Does.Contain("BEST-EFFORT"), "should fall back to a best-effort document");
            Assert.That(sql, Does.Contain("People in One"), "each cohort set should be listed");
            Assert.That(sql, Does.Contain("People in Two"));
            Assert.That(sql, Does.Contain("UNION"), "the set operation joining the sets should be shown");
            Assert.That(sql, Does.Contain("COULD NOT BE CONVERTED"), "should end with the could-not-convert notes");
        });

        outDir.Delete(true);
    }

    /// <summary>
    /// A catalogue with a patient-identifier column (<paramref name="idEi"/>) and a date column
    /// (<paramref name="dateEi"/>), suitable for use as a cohort identification set.
    /// </summary>
    private Catalogue MakeCohortCatalogue(string name, out ExtractionInformation idEi, out ExtractionInformation dateEi)
    {
        var throwaway = WhenIHaveA(Repository, out dateEi, out var otherEi);
        var cata = throwaway.Catalogue;
        cata.Name = name;
        cata.SaveToDatabase();

        otherEi.IsExtractionIdentifier = true;
        otherEi.SaveToDatabase();
        idEi = otherEi;

        throwaway.DeleteInDatabase(); // we only wanted the catalogue + its two ExtractionInformations
        return cata;
    }
}
