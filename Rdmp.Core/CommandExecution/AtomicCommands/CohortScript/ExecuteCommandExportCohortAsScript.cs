// Exports a Cohort Identification Configuration (CIC) as a portable, data-free triple of text files:
//   <out>/<cic>/requirement.md   <out>/<cic>/build.script.yaml   <out>/<cic>/query.sql
//
// build.script.yaml is a runnable command script that ExecuteCommandBuildCohortFromScript replays
// to recreate an identical cohort; query.sql is the SQL RDMP would run. No patient data leaves -
// only catalogue/table/column names and the cohort's filter logic.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.QueryBuilding;
using YamlDotNet.Serialization;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortScript;

public class ExecuteCommandExportCohortAsScript : BasicCommandExecution
{
    private readonly CohortIdentificationConfiguration _cic;
    private readonly DirectoryInfo _outDir;

    public ExecuteCommandExportCohortAsScript(IBasicActivateItems activator,
        [DemandsInitialization("The cohort to export")]
        CohortIdentificationConfiguration cic,
        [DemandsInitialization("Folder to write the export into")]
        DirectoryInfo toDir = null) : base(activator)
    {
        _cic = cic;
        _outDir = toDir ?? new DirectoryInfo(Environment.CurrentDirectory);

        if (_cic == null)
            SetImpossible("No CohortIdentificationConfiguration was supplied");
    }

    public override void Execute()
    {
        base.Execute();

        var dir = new DirectoryInfo(Path.Join(_outDir.FullName, Sanitise(_cic.Name)));
        dir.Create();

        // requirement.md is an intentionally-empty placeholder. The natural-language
        // requirement is added by hand later (extracted from the request form) - it is NOT
        // taken from CIC.Description, which may be unrelated. Never overwrite a requirement
        // that has already been filled in, so re-exporting is safe.
        var reqPath = Path.Join(dir.FullName, "requirement.md");
        if (!File.Exists(reqPath))
            File.WriteAllText(reqPath, $"<!-- Paste the natural-language requirement for '{_cic.Name}' here. -->\n");

        File.WriteAllText(Path.Join(dir.FullName, "build.script.yaml"), BuildScript());

        // query.sql: the SQL as RDMP would run it - this uses the cohort's QueryCache if one
        // is configured (so it references cache tables). SQL generation is best-effort.
        File.WriteAllText(Path.Join(dir.FullName, "query.sql"), BuildSql(useCache: true));

        // query.uncached.sql: the full query against the raw tables, cache bypassed. Only emitted
        // when a cache is configured (otherwise query.sql is already the un-cached query).
        if (_cic.QueryCachingServer_ID.HasValue)
            File.WriteAllText(Path.Join(dir.FullName, "query.uncached.sql"), BuildSql(useCache: false));

        // catalogue-manifest.yaml: the "menu" of building blocks - but ONLY the catalogues this
        // cohort actually uses (its cohort sets + patient index tables), with their extractable
        // columns, patient-identifier column(s) and published filters. Scoped this way it stays
        // relevant and avoids dumping the whole platform's catalogues.
        File.WriteAllText(Path.Join(dir.FullName, "catalogue-manifest.yaml"), BuildCatalogueManifest());

        BasicActivator.Show($"Exported '{_cic.Name}' to {dir.FullName}");
    }

    private string BuildSql(bool useCache)
    {
        try
        {
            var builder = new CohortQueryBuilder(_cic, null);
            if (!useCache && _cic.QueryCachingServer_ID.HasValue)
                builder.CacheServer = null; // force the query to run against the raw tables
            return builder.SQL ?? "";
        }
        catch (Exception e)
        {
            // The whole cohort could not be assembled into a single runnable statement - most often
            // because its sets are on different servers / use different credentials and no QueryCache
            // is configured (RDMP cannot UNION/INTERSECT/EXCEPT across servers). Rather than lose
            // everything to one error line, emit each set's SQL individually plus notes on what could
            // not be combined.
            return BuildBestEffortSql(e);
        }
    }

    // Best-effort SQL when the whole-cohort query cannot be generated: emit each cohort set's SQL
    // (each set is single-server so it generates fine), mirror the set-operation tree as comments,
    // and end with a notes block describing what could not be converted.
    private string BuildBestEffortSql(Exception fullBuildError)
    {
        var lines = new List<string>();
        var failures = new List<string>();
        var servers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        lines.Add("-- ============================================================================");
        lines.Add("-- BEST-EFFORT SQL");
        lines.Add("-- The full cohort query could not be assembled into a single runnable statement:");
        lines.Add($"--   {OneLine(fullBuildError.Message)}");
        lines.Add("-- This usually means the cohort's sets are on different servers / use different");
        lines.Add("-- credentials and no QueryCache is configured, so they cannot be combined in one");
        lines.Add("-- query. Each cohort set's SQL is emitted individually below; to run the whole");
        lines.Add("-- cohort configure a QueryCache on the CohortIdentificationConfiguration (or stage");
        lines.Add("-- the per-set results and combine them with the set operations shown as comments).");
        lines.Add("-- ============================================================================");
        lines.Add("");

        if (_cic.RootCohortAggregateContainer is { } root)
            EmitBestEffortContainer(root, 0, lines, failures, servers);
        else
            lines.Add("-- (this cohort has no root container)");

        lines.Add("");
        lines.Add("-- ============================================================================");
        lines.Add("-- COULD NOT BE CONVERTED TO A SINGLE SQL QUERY:");
        if (servers.Count > 1)
            lines.Add($"--   * The sets above span {servers.Count} server(s)/credential(s): {string.Join(", ", servers.OrderBy(s => s))}.");
        lines.Add("--   * SQL Server cannot UNION/INTERSECT/EXCEPT across servers without a QueryCache");
        lines.Add("--     (or linked servers). Apply the set operations shown as comments via a QueryCache");
        lines.Add("--     or by combining the per-set results manually.");
        foreach (var f in failures)
            lines.Add($"--   * {f}");
        lines.Add("-- ============================================================================");

        return string.Join("\n", lines) + "\n";
    }

    private void EmitBestEffortContainer(CohortAggregateContainer container, int depth,
        List<string> lines, List<string> failures, HashSet<string> servers)
    {
        var indent = new string(' ', depth * 2);
        lines.Add($"{indent}/* container \"{container.Name}\"  [{container.Operation}] */");

        var first = true;
        foreach (var content in container.GetOrderedContents())
        {
            if (!first)
                lines.Add($"{indent}-- {container.Operation}");
            first = false;

            switch (content)
            {
                case AggregateConfiguration agg:
                    var server = ServerOf(agg);
                    if (server != null) servers.Add(server);
                    lines.Add($"{indent}/* set: \"{agg.Name}\"   (server: {server ?? "unknown"}) */");
                    var sql = SingleSetSql(agg, out var err);
                    if (err != null)
                    {
                        lines.Add($"{indent}-- (this set's SQL could not be generated: {OneLine(err)})");
                        failures.Add($"Set \"{agg.Name}\" could not be generated: {OneLine(err)}");
                    }
                    else
                    {
                        foreach (var sqlLine in sql.Replace("\r", "").Split('\n'))
                            lines.Add(indent + sqlLine.TrimEnd());
                    }

                    break;
                case CohortAggregateContainer sub:
                    EmitBestEffortContainer(sub, depth + 1, lines, failures, servers);
                    break;
            }
        }
    }

    // A single cohort set is always on one server, so its SQL generates even when the whole cohort
    // (which may span servers) cannot. Still guarded: a set could itself be cross-server (e.g. a
    // patient index table join to another server).
    private string SingleSetSql(AggregateConfiguration agg, out string error)
    {
        try
        {
            error = null;
            return new CohortQueryBuilder(agg, _cic.GetAllParameters(), null).SQL ?? "";
        }
        catch (Exception e)
        {
            error = e.Message;
            return "";
        }
    }

    private static string ServerOf(AggregateConfiguration agg)
    {
        try
        {
            var ti = agg.Catalogue?.GetTableInfoList(false).FirstOrDefault();
            return ti == null ? null : $"{ti.Server}/{ti.Database}";
        }
        catch
        {
            return null;
        }
    }

    // The distinct Catalogues used by this cohort: those behind its cohort sets and its
    // patient index tables. Only these are dumped to the manifest.
    private IEnumerable<Catalogue> CohortCatalogues()
    {
        var seen = new HashSet<int>();
        var result = new List<Catalogue>();

        void Add(Catalogue c)
        {
            if (c != null && seen.Add(c.ID)) result.Add(c);
        }

        if (_cic.RootCohortAggregateContainer is { } root)
            foreach (var agg in root.GetAllAggregateConfigurationsRecursively())
                Add(agg.Catalogue);

        foreach (var j in _cic.GetAllJoinables())
            Add(j.AggregateConfiguration?.Catalogue);

        return result;
    }

    private string BuildCatalogueManifest()
    {
        var catalogues = CohortCatalogues()
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .Select(c =>
            {
                var eis = c.GetAllExtractionInformation();
                return (object)new Dictionary<string, object>
                {
                    ["id"] = c.ID,
                    ["name"] = c.Name,
                    ["identifier_columns"] = eis.Where(e => e.IsExtractionIdentifier)
                        .Select(e => e.GetRuntimeName()).ToList(),
                    ["columns"] = eis.Select(e => e.GetRuntimeName()).ToList(),
                    ["filters"] = c.GetAllFilters()
                        .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                        .Select(f => (object)new Dictionary<string, object>
                        {
                            ["id"] = f.ID,
                            ["name"] = f.Name,
                            ["where"] = OneLine(f.WhereSQL),
                            ["parameters"] = f.GetAllParameters().Select(p => (object)new Dictionary<string, object>
                            {
                                ["name"] = p.ParameterName,
                                ["declare"] = OneLine(p.ParameterSQL),
                                ["value"] = OneLine(p.Value),
                                ["comment"] = OneLine(p.Comment)
                            }).ToList()
                        }).ToList()
                };
            })
            .ToList();

        return new SerializerBuilder().Build()
            .Serialize(new Dictionary<string, object> { ["catalogues"] = catalogues });
    }

    private string BuildScript()
    {
        var root = _cic.RootCohortAggregateContainer;

        // ' => $handle' declares the handle bound to the object a creating command produces, so
        // the runner (BuildCohortFromScript) can re-bind it to the real id it gets at build time.
        var lines = new List<string>
        {
            $"# Decompiled from CohortIdentificationConfiguration ID {_cic.ID}",
            "Commands:",
            root != null
                ? $"  - CreateNewCohortIdentificationConfiguration \"{_cic.Name}\" => {ContainerRef(root)}"
                : $"  - CreateNewCohortIdentificationConfiguration \"{_cic.Name}\"",
        };

        // If this cohort is tied to a Project (required to use project-specific catalogues),
        // record it so the rebuilt cohort joins the same Project BEFORE its catalogues are added.
        var assoc = BasicActivator.RepositoryLocator.DataExportRepository
            .GetAllObjectsWhere<ProjectCohortIdentificationConfigurationAssociation>(
                "CohortIdentificationConfiguration_ID", _cic.ID).FirstOrDefault();
        if (assoc != null)
            lines.Add($"  - AssociateWithProject Project:{assoc.Project_ID}");

        // Cohort-level (global) parameters - declared first so anything referencing them exists.
        EmitGlobalParameters(lines);

        // Patient-index tables (joinables) must exist BEFORE the cohort sets that join to them.
        // The runner creates each as $pit<oldJoinableId> and rewrites the ix<oldId> filter alias.
        foreach (var j in _cic.GetAllJoinables())
        {
            var pitAgg = j.AggregateConfiguration;
            // listed by UNDERLYING column name (an aliased dimension's GetRuntimeName returns the
            // alias, which would not match any ExtractionInformation on rebuild)
            var dims = pitAgg.AggregateDimensions
                .Where(d => d.ExtractionInformation is not { IsExtractionIdentifier: true })
                .Select(UnderlyingColumnName);
            // Aggregate:<oldId> lets the runner bind the rebuilt PIT aggregate to $a<oldId> so the
            // dimension-SQL overrides below (which restore e.g. the qualified chi) can target it.
            lines.Add(
                $"  - CreatePatientIndexTable Catalogue:{Quote(pitAgg.Catalogue.Name)} Aggregate:{pitAgg.ID} Dimensions:\"{string.Join(",", dims)}\" => $pit{j.ID}");
            EmitDimensionOverrides(pitAgg, AggregateRef(pitAgg), lines);
            // a patient index table can itself carry forced joins, parameters and a filter tree
            EmitAggregateBody(pitAgg, AggregateRef(pitAgg), lines);
        }

        if (root != null)
            EmitContainer(root, lines);
        else
            lines.Add("  # (this CIC has no root container)");

        // Disabled cohort sets / sub-containers are kept in the tree but excluded from the query.
        // Emit these LAST (after the whole structure exists) so the disabled state is restored
        // without affecting how the structure is built.
        if (root != null)
            EmitDisabledStates(root, lines);

        return string.Join("\n", lines) + "\n";
    }

    // Restores IsDisabled on any cohort set / sub-container that was disabled. Uses the generic
    // Set command (same mechanism as filter-parameter values), so no special runner handling.
    private void EmitDisabledStates(CohortAggregateContainer container, List<string> lines)
    {
        if (container.IsDisabled)
            lines.Add($"  - Set CohortAggregateContainer:{ContainerRef(container)} IsDisabled true");

        foreach (var content in container.GetOrderedContents())
            switch (content)
            {
                case AggregateConfiguration agg when agg.IsDisabled:
                    lines.Add($"  - Set AggregateConfiguration:{AggregateRef(agg)} IsDisabled true");
                    break;
                case CohortAggregateContainer sub:
                    EmitDisabledStates(sub, lines);
                    break;
            }
    }

    private void EmitGlobalParameters(List<string> lines)
    {
        // Cohort-level (global) SQL parameters - referenced by filters across any cohort set.
        // Recreated on the rebuilt CIC by the runner. Filter parameters are emitted inline as Set
        // commands after each filter, so they are not duplicated here.
        foreach (var p in _cic.GetAllParameters())
            lines.Add(
                $"  - AddGlobalParameter \"{p.ParameterName}\" \"{OneLine(p.ParameterSQL)}\" \"{OneLine(p.Value)}\"");
    }

    // Containers and aggregates are referenced by stable handles ($c<id> / $a<id>) rather than
    // by name: names are not guaranteed unique or meaningful (an unnamed sub-container reports
    // its operation as its name). The trailing comment keeps the script human-readable.
    private static string ContainerRef(CohortAggregateContainer c) => $"$c{c.ID}";
    private static string AggregateRef(AggregateConfiguration a) => $"$a{a.ID}";

    private void EmitContainer(CohortAggregateContainer container, List<string> lines)
    {
        var cref = ContainerRef(container);
        lines.Add($"  - SetContainerOperation CohortAggregateContainer:{cref} {container.Operation}   # {container.Name}");
        // Restore the container's name. Always emitted: it carries custom names (e.g. "UNION T2")
        // and fixes the stale default label a rebuilt container would otherwise show (created as
        // "UNION", then Operation changed directly without the rename the GUI command performs).
        lines.Add($"  - Set CohortAggregateContainer:{cref} Name \"{OneLine(container.Name)}\"");

        foreach (var content in container.GetOrderedContents())
            switch (content)
            {
                case AggregateConfiguration agg:
                    EmitAggregate(agg, cref, lines);
                    break;
                case CohortAggregateContainer sub:
                    lines.Add($"  - AddCohortSubContainer CohortAggregateContainer:{cref} => {ContainerRef(sub)}");
                    EmitContainer(sub, lines);
                    break;
            }
    }

    private void EmitAggregate(AggregateConfiguration agg, string containerRef, List<string> lines)
    {
        var cata = agg.Catalogue;
        var cataRef = cata != null ? Quote(cata.Name) : "<unknown-catalogue>";
        lines.Add(
            $"  - AddCatalogueToCohortIdentificationSetContainer CohortAggregateContainer:{containerRef} Catalogue:{cataRef} => {AggregateRef(agg)}");

        foreach (var dim in agg.AggregateDimensions)
            lines.Add($"  # dimension: {dim.GetRuntimeName()}   (auto-set when catalogue added)");

        // Restore any dimension whose SelectSQL was customised away from the catalogue default
        // (e.g. the extraction identifier qualified to [db]..[tbl].[col] so a PIT join isn't ambiguous).
        EmitDimensionOverrides(agg, AggregateRef(agg), lines);

        // join-uses: this cohort set joins to a patient-index table ($pit<id>) created earlier.
        foreach (var use in agg.PatientIndexJoinablesUsed)
            lines.Add(
                $"  - UsePatientIndexTable {AggregateRef(agg)} $pit{use.JoinableCohortAggregateConfiguration_ID} {use.JoinType}");

        EmitAggregateBody(agg, AggregateRef(agg), lines);
    }

    // Forced joins, aggregate-level parameters and the filter tree of an aggregate. Shared by
    // cohort sets and patient index tables (a PIT can have its own filters/params/forced joins too).
    private void EmitAggregateBody(AggregateConfiguration agg, string aggRef, List<string> lines)
    {
        // HAVING clause (e.g. "count(*) >= 2") - changes which patients the set matches.
        if (!string.IsNullOrWhiteSpace(agg.HavingSQL))
            lines.Add($"  - Set AggregateConfiguration:{aggRef} HavingSQL \"{OneLine(agg.HavingSQL)}\"");

        // forced joins: tables explicitly joined into this aggregate's query
        foreach (var ti in agg.ForcedJoins)
            lines.Add($"  - AddForcedJoin {aggRef} TableInfo:{Quote(ti.Name)}");

        // aggregate-level parameters (e.g. @window) - distinct from filter parameters.
        // Query directly (agg.Parameters also filters on repository-type, which can miss).
        foreach (var ap in BasicActivator.RepositoryLocator.CatalogueRepository
                     .GetAllObjects<AnyTableSqlParameter>()
                     .Where(p => p.ReferencedObjectType == nameof(AggregateConfiguration) && p.ReferencedObjectID == agg.ID))
            lines.Add(
                $"  - AddAggregateParameter {aggRef} \"{ap.ParameterName}\" \"{OneLine(ap.ParameterSQL)}\" \"{OneLine(ap.Value)}\"");

        if (agg.RootFilterContainer is { } fc)
            EmitFilters(fc, aggRef, lines);
    }

    // Emits SetDimensionSql / SetDimensionAlias directives for every dimension whose SelectSQL or
    // Alias was customised away from its catalogue ExtractionInformation default. Both are keyed by
    // the UNDERLYING column name (not GetRuntimeName, which returns the alias once one is set) so
    // the runner can find the dimension regardless of alias. An aliased PIT dimension is functional:
    // the join filter references ix<id>.<alias>.
    private void EmitDimensionOverrides(AggregateConfiguration agg, string aggRef, List<string> lines)
    {
        foreach (var dim in agg.AggregateDimensions)
        {
            var key = UnderlyingColumnName(dim);
            var eiSql = dim.ExtractionInformation?.SelectSQL;
            if (!string.IsNullOrWhiteSpace(dim.SelectSQL) && dim.SelectSQL != eiSql)
                lines.Add($"  - SetDimensionSql {aggRef} \"{key}\" \"{OneLine(dim.SelectSQL)}\"");
            if (!string.IsNullOrWhiteSpace(dim.Alias))
                lines.Add($"  - SetDimensionAlias {aggRef} \"{key}\" \"{dim.Alias}\"");
        }
    }

    // The dimension's underlying catalogue column name, stable across aliasing.
    private static string UnderlyingColumnName(AggregateDimension dim) =>
        dim.ExtractionInformation?.GetRuntimeName() ?? dim.GetRuntimeName();

    private void EmitFilters(IContainer rootFc, string aggRef, List<string> lines)
    {
        // Build the aggregate's root filter container with the right AND/OR operation, then fill it.
        // (Directives the runner handles directly; the CLI AddNewFilterContainer misbehaves headless.)
        var key = $"fc{((AggregateFilterContainer)rootFc).ID}";
        lines.Add($"  - EnsureFilterContainer {aggRef} {rootFc.Operation} => ${key}");
        if (((AggregateFilterContainer)rootFc).IsDisabled)
            lines.Add($"  - Set AggregateFilterContainer:${key} IsDisabled true");
        EmitContainerFilters(rootFc, key, lines);
    }

    // fcKey (no leading $) is the handle of the container the filters/sub-containers go INTO.
    private void EmitContainerFilters(IContainer fc, string fcKey, List<string> lines)
    {
        foreach (var filter in fc.GetFilters())
        {
            // Recreate every filter from its ACTUAL WhereSQL + parameters (exactly what the GUI
            // clone does). We deliberately do NOT re-import via ExtractionFilter: the published
            // master filter can have drifted from the cohort's copy (e.g. an EXISTS added/removed,
            // or a different default value), which would silently change the query. CreateNewFilter
            // with a literal WhereSQL does not auto-create parameters, so we bind the FILTER ($f)
            // and create each parameter explicitly.
            var afilter = (AggregateFilter)filter;
            var afps = filter.GetAllParameters().OfType<AggregateFilterParameter>().ToArray();
            var fid = afilter.ID;
            var fbind = afps.Length == 0 && !afilter.IsDisabled ? "" : $" => $f{fid}";
            lines.Add(
                $"  - CreateNewFilter AggregateFilterContainer:${fcKey} \"{filter.Name}\" \"{OneLine(filter.WhereSQL)}\"{fbind}");
            foreach (var afp in afps)
                lines.Add(
                    $"  - AddFilterParameter $f{fid} \"{afp.ParameterName}\" \"{OneLine(afp.ParameterSQL)}\" \"{OneLine(afp.Value)}\"");
            // a disabled filter is excluded from the WHERE clause but kept in the tree
            if (afilter.IsDisabled)
                lines.Add($"  - Set AggregateFilter:$f{fid} IsDisabled true");
        }

        foreach (var sub in fc.GetSubContainers())
        {
            var subKey = $"fc{((AggregateFilterContainer)sub).ID}";
            lines.Add($"  - AddFilterSubContainer ${fcKey} {sub.Operation} => ${subKey}");
            if (((AggregateFilterContainer)sub).IsDisabled)
                lines.Add($"  - Set AggregateFilterContainer:${subKey} IsDisabled true");
            EmitContainerFilters(sub, subKey, lines);
        }
    }

    private static string Quote(string name) => $"\"{name}\"";
    private static string OneLine(string sql) => (sql ?? "").Replace("\r", " ").Replace("\n", " ").Trim();

    private static string Sanitise(string name)
    {
        var sb = new StringBuilder();
        foreach (var c in name)
            sb.Append(Array.IndexOf(Path.GetInvalidFileNameChars(), c) >= 0 ? '_' : c);
        return sb.ToString();
    }
}
