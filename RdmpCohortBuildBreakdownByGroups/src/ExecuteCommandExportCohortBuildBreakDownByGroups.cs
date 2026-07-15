// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace RdmpCohortBuildBreakdownByGroups;

/// <summary>
/// Reproduces the Cohort Builder's per-set / per-container count tree (the FinalCount and cumulative
/// running totals shown as UNION/INTERSECT/EXCEPT are applied) split by an arbitrary group column
/// (e.g. health board, GP practice, age band), using a user-supplied lookup table to label and order
/// the groups. Operates purely on the query cache: it builds the cohort once to populate the per-set
/// cache tables, then recomposes every count point from those cache tables and splits it by group with
/// one GROUP BY per node (all groups at once).
///
/// <para>Inputs are four columns; the tables are derived: <c>groupColumn</c>'s table is the reference
/// table (which must contain exactly one IsExtractionIdentifier column, the join key to the cohort),
/// and <c>lookupKeyColumn</c>'s table is the lookup table.</para>
///
/// <para>PRECONDITION: each identifier must map to AT MOST ONE group in the reference table (a
/// single-valued identifier-to-group mapping, e.g. patient -> health board; many identifiers per
/// group is of course fine). Multi-group membership double-counts
/// patients across group columns, inflates the reference-population denominator and can make the
/// NotKnown residual negative. The post-run warning (group counts exceeding the unfiltered total)
/// is a symptom of violating this.</para>
/// </summary>
public class ExecuteCommandExportCohortBuildBreakDownByGroups : BasicCommandExecution
{
    private readonly CohortIdentificationConfiguration _cic;
    private ColumnInfo _groupColumn;
    private ColumnInfo _lookupKeyColumn;
    private ColumnInfo _lookupLabelColumn;
    private ColumnInfo _lookupGroupingColumn;
    private readonly int _timeout;
    private FileInfo _toFile;

    private ExtractionInformation _idEi;
    private IQuerySyntaxHelper _syntax;
    private DiscoveredDatabase _cacheDb;
    private CachedAggregateConfigurationResultsManager _cacheManager;
    private GroupLookup _lookup;
    private string _referenceTable;
    private string _referenceId;
    private string _groupName;

    private readonly Dictionary<int, (string fqn, string col)> _setCacheTable = new();
    private readonly Dictionary<(bool isContainer, int id), (int final, int? cumulative)> _baseline = new();

    public ExecuteCommandExportCohortBuildBreakDownByGroups(IBasicActivateItems activator,
        [DemandsInitialization("The cohort identification configuration whose build tree to break down")]
        CohortIdentificationConfiguration cic,
        [DemandsInitialization("The column to group the breakdown by (its table is the reference table and must contain the patient identifier)")]
        ColumnInfo groupColumn = null,
        [DemandsInitialization("Lookup table column holding the group code (as it appears in the group column); its table is the lookup table")]
        ColumnInfo lookupKeyColumn = null,
        [DemandsInitialization("Lookup table column holding the display label for each code")]
        ColumnInfo lookupLabelColumn = null,
        [DemandsInitialization("CSV file to write. Defaults to <cic>-build-breakdown.csv in the current directory")]
        FileInfo toFile = null,
        [DemandsInitialization("Optional lookup table column holding a higher grouping (used to order the output columns)")]
        ColumnInfo lookupGroupingColumn = null,
        [DemandsInitialization("Per-query command timeout in seconds", DefaultValue = 5000)]
        int timeout = 5000) : base(activator)
    {
        _cic = cic;
        _groupColumn = groupColumn;
        _lookupKeyColumn = lookupKeyColumn;
        _lookupLabelColumn = lookupLabelColumn;
        _lookupGroupingColumn = lookupGroupingColumn;
        _timeout = timeout;
        _toFile = toFile;

        if (_cic == null)
        {
            SetImpossible("No CohortIdentificationConfiguration was supplied");
            return;
        }

        if (_cic.RootCohortAggregateContainer_ID == null)
        {
            SetImpossible($"'{_cic}' has no root container to run");
            return;
        }

        if (_cic.QueryCachingServer_ID == null)
            SetImpossible($"'{_cic}' has no query caching server - this breakdown works only on cached results");

        // the columns are resolved (and prompted for, in the GUI) in Execute, so the command can be
        // added to a right-click menu with only the cohort selected.
    }

    /// <summary>Resolves the column inputs, prompting the user for any not supplied. Returns false on cancel.</summary>
    private bool ResolveInputs()
    {
        _groupColumn ??= SelectColumn("Group-by column (on the reference table, e.g. demography Region)");
        if (_groupColumn == null)
            return Fail("No group column was supplied");

        // derive the patient identifier from the group column's table: the (single) column flagged
        // IsExtractionIdentifier. Zero = no join key; several = ambiguous identifier domain.
        var idEis = _groupColumn.TableInfo.ColumnInfos
            .SelectMany(c => c.CatalogueItems.Select(ci => ci.ExtractionInformation))
            .Where(ei => ei is { IsExtractionIdentifier: true })
            .GroupBy(ei => ei.ColumnInfo.ID)
            .Select(g => g.First())
            .ToList();

        if (idEis.Count != 1)
            return Fail($"Table '{_groupColumn.TableInfo}' must have exactly one IsExtractionIdentifier column " +
                        $"to join the cohort on (found {idEis.Count})");
        _idEi = idEis[0];

        // a TRANSFORMED identifier (e.g. UPPER(chi)) means the cohort's cached ids may not equal the
        // raw column values, so a raw-table join would silently mismatch - refuse rather than guess
        if (IsTransformedIdentifier(_idEi.SelectSQL, _idEi.ColumnInfo.GetRuntimeName(),
                _groupColumn.GetQuerySyntaxHelper()))
            return Fail($"The identifier column '{_idEi}' is a transformed expression ('{_idEi.SelectSQL}'); " +
                        "this breakdown joins on the raw table column, so transformed identifiers are not supported");

        _lookupKeyColumn ??= SelectColumn("Lookup KEY column (the group code, e.g. z_hb_lookup.Region)");
        if (_lookupKeyColumn == null)
            return Fail("No lookup key column was supplied");

        _lookupLabelColumn ??= SelectColumn("Lookup LABEL column (display name, e.g. z_hb_lookup.HB_Name)");
        if (_lookupLabelColumn == null)
            return Fail("No lookup label column was supplied");

        // grouping stays optional: do not prompt for it, only validate if supplied

        if (_lookupLabelColumn.TableInfo_ID != _lookupKeyColumn.TableInfo_ID ||
            (_lookupGroupingColumn != null && _lookupGroupingColumn.TableInfo_ID != _lookupKeyColumn.TableInfo_ID))
            return Fail("The lookup key, label and grouping columns must all belong to the same table");

        // co-location: the recompose + GROUP BY join runs on the cache server, so the reference table
        // must be reachable there. Use RDMP's own single-server validation (server + DBMS type +
        // credential compatibility) rather than a hand-rolled string comparison.
        try
        {
            var points = new DataAccessPointCollection(true);
            points.Add(_cic.QueryCachingServer);
            points.Add(_groupColumn.TableInfo);
        }
        catch (Exception ex)
        {
            return Fail($"The reference table '{_groupColumn.TableInfo}' and the query cache " +
                        $"'{_cic.QueryCachingServer}' must be on the same server: {ex.Message}");
        }

        return true;
    }

    private ColumnInfo SelectColumn(string prompt)
    {
        var available = BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<ColumnInfo>();
        return available.Length > 0 && BasicActivator.SelectObject(prompt, available, out var selected)
            ? selected
            : null;
    }

    private bool Fail(string reason)
    {
        BasicActivator.Show(reason);
        return false;
    }

    public override void Execute()
    {
        base.Execute();

        if (!ResolveInputs())
            return;

        _toFile ??= BasicActivator.IsInteractive
            ? BasicActivator.SelectFile("Path to write build breakdown to", "Build breakdown", "*.csv")
            : new FileInfo(Path.Combine(Environment.CurrentDirectory, $"{Sanitise(_cic.Name)}-build-breakdown.csv"));

        if (_toFile == null)
            return;

        _syntax = _groupColumn.GetQuerySyntaxHelper();
        _cacheDb = _cic.QueryCachingServer.Discover(DataAccessContext.InternalDataProcessing);
        _cacheManager = new CachedAggregateConfigurationResultsManager(_cic.QueryCachingServer);
        // fully-qualified via the discovery API rather than trusting TableInfo.Name verbatim
        _referenceTable = _groupColumn.TableInfo.Discover(DataAccessContext.InternalDataProcessing)
            .GetFullyQualifiedName();
        // join on the PHYSICAL identifier column: ExtractionInformation.GetRuntimeName() returns the
        // alias when one exists (e.g. "chi AS PatientId"), which does not exist on the raw table
        _referenceId = _syntax.EnsureWrapped(_idEi.ColumnInfo.GetRuntimeName());
        _groupName = _syntax.EnsureWrapped(_groupColumn.GetRuntimeName());

        // load the (user-defined) code -> label/grouping mapping from the lookup table
        _lookup = GroupLookup.LoadFrom(
            _lookupKeyColumn.TableInfo.Discover(DataAccessContext.InternalDataProcessing),
            _lookupKeyColumn.GetRuntimeName(),
            _lookupLabelColumn.GetRuntimeName(),
            _lookupGroupingColumn?.GetRuntimeName(),
            _timeout);

        // 1. Build once: populates every per-set cache table and gives the baseline (unfiltered) counts.
        var compiler = new CohortCompiler(BasicActivator, _cic) { IncludeCumulativeTotals = true };
        var runner = new CohortCompilerRunner(compiler, _timeout) { RunSubcontainers = true };
        runner.Run(new CancellationToken());

        var crashed = compiler.Tasks.Keys.Where(t => t.State == CompilationState.Crashed).ToList();
        if (crashed.Any())
        {
            SetImpossible($"{crashed.Count} task(s) crashed during the build - cannot produce a reliable breakdown");
            BasicActivator.Show($"Build failed: {crashed[0].CrashMessage?.Message}");
            return;
        }

        foreach (var task in compiler.Tasks.Keys)
        {
            var isContainer = task switch
            {
                AggregationContainerTask => true,
                AggregationTask => false,
                _ => (bool?)null // skip joinables / plugin tasks
            };
            if (isContainer == null || task.Child == null)
                continue;
            _baseline[(isContainer.Value, task.Child.ID)] = (task.FinalRowCount, task.CumulativeRowCount);
        }

        // 2. Walk the tree, recomposing each count point from the cache and splitting by group.
        var nodes = new List<CohortBuildBreakdownNode>();
        var seq = 0;
        Walk(_cic.RootCohortAggregateContainer, null, 0, nodes, ref seq);

        // reference: each group's share of the WHOLE reference population (a sanity-check row)
        var referencePopulation = ComputeReferencePopulation();

        CohortBuildBreakdownByGroupsReport.WriteCsv(_toFile.FullName, nodes, _lookup, referencePopulation);

        // reconciliation note: recognised-group counts should never exceed the unfiltered total
        var drift = nodes.Count(n =>
            n.FinalByGroup.Where(kv => _lookup.Contains(kv.Key)).Sum(kv => kv.Value) > n.FinalUnfiltered);
        var summary = $"Exported build breakdown to {_toFile.FullName} ({nodes.Count} count points)";
        if (drift > 0)
            summary += $" - WARNING: {drift} node(s) have group counts exceeding the unfiltered total (check keys)";
        BasicActivator.Show(summary);
    }

    private void Walk(CohortAggregateContainer container, CohortAggregateContainer parent, int indexInParent,
        List<CohortBuildBreakdownNode> nodes, ref int seq)
    {
        // container node row (cumulative is within its parent)
        var (cFinal, cCum) = _baseline.TryGetValue((true, container.ID), out var cb) ? cb : (0, null);
        IReadOnlyDictionary<string, int> cCumByGroup = null;
        if (parent != null && indexInParent > 0 && cCum.HasValue)
            cCumByGroup = RunGroupCounts(CumulativeSql(parent, indexInParent));

        nodes.Add(new CohortBuildBreakdownNode(seq++, "Container", CleanName(container.Name),
            CleanName(parent?.Name), container.Operation.ToString(), container.Order, cFinal,
            parent != null && indexInParent > 0 ? cCum : null,
            RunGroupCounts(IdSql(container)), cCumByGroup));

        var kids = EnabledOrdered(container);
        for (var i = 0; i < kids.Count; i++)
        {
            switch (kids[i])
            {
                case AggregateConfiguration agg:
                    var (aFinal, aCum) = _baseline.TryGetValue((false, agg.ID), out var ab) ? ab : (0, null);
                    IReadOnlyDictionary<string, int> aCumByGroup = null;
                    if (i > 0 && aCum.HasValue)
                        aCumByGroup = RunGroupCounts(CumulativeSql(container, i));

                    nodes.Add(new CohortBuildBreakdownNode(seq++, "Cohort Set", CleanName(agg.Name),
                        CleanName(container.Name), "", agg.Order, aFinal, i > 0 ? aCum : null,
                        RunGroupCounts(CachedSetSql(agg)), aCumByGroup));
                    break;

                case CohortAggregateContainer sub:
                    Walk(sub, container, i, nodes, ref seq);
                    break;
            }
        }
    }

    // --- identifier-list SQL composed purely from the per-set cache tables ---

    private string IdSql(IOrderable node) => node switch
    {
        AggregateConfiguration agg => CachedSetSql(agg),
        CohortAggregateContainer c => Compose(c, EnabledOrdered(c)),
        _ => throw new NotSupportedException(node.GetType().Name)
    };

    private string CumulativeSql(CohortAggregateContainer container, int upToInclusive) =>
        Compose(container, EnabledOrdered(container).Take(upToInclusive + 1).ToList());

    private string Compose(CohortAggregateContainer container, IReadOnlyList<IOrderable> children)
    {
        if (children.Count == 0)
            throw new InvalidOperationException(
                $"Container '{container.Name}' has no enabled content to compose - it should have been skipped");
        var op = $"\n{SetOperationSql(container.Operation, _syntax.DatabaseType)}\n";
        return string.Join(op, children.Select(ch => $"({IdSql(ch)})"));
    }

    /// <summary>
    /// Renders a container's set operation for the target DBMS (Oracle spells EXCEPT as MINUS) - the
    /// same mapping RDMP uses in <c>CohortQueryBuilderResult.GetSetOperationSql</c>.
    /// </summary>
    public static string SetOperationSql(SetOperation operation, DatabaseType dbType) => operation switch
    {
        SetOperation.UNION => "UNION",
        SetOperation.INTERSECT => "INTERSECT",
        SetOperation.EXCEPT => dbType == DatabaseType.Oracle ? "MINUS" : "EXCEPT",
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
    };

    private string CachedSetSql(AggregateConfiguration agg)
    {
        if (!_setCacheTable.TryGetValue(agg.ID, out var t))
        {
            var table = _cacheManager.GetLatestResultsTableUnsafe(agg,
                AggregateOperation.IndexedExtractionIdentifierList) as DiscoveredTable;
            if (table == null)
                throw new Exception($"Cohort set '{agg.Name}' has no cached identifier list - the build did not cache it");
            var col = table.DiscoverColumns()[0].GetRuntimeName();
            t = (table.GetFullyQualifiedName(), _syntax.EnsureWrapped(col));
            _setCacheTable[agg.ID] = t;
        }

        return $"SELECT {t.col} id FROM {t.fqn}";
    }

    private List<IOrderable> EnabledOrdered(CohortAggregateContainer container) =>
        container.GetOrderedContents().Where(o => o switch
        {
            AggregateConfiguration a => !a.IsDisabled,
            // reuse RDMP's own enabled/skip logic (disabled or empty containers) so our walk matches
            // the baseline task set produced by CohortCompilerRunner
            CohortAggregateContainer c => CohortQueryBuilderResult.IsEnabled(c,
                BasicActivator.CoreChildProvider),
            _ => true
        }).ToList();

    // --- run a GROUP BY join (on the cache server) for one count point ---

    private IReadOnlyDictionary<string, int> RunGroupCounts(string idListSql)
    {
        var sql =
            $"SELECT d.{_groupName} grp, COUNT(DISTINCT i.id) n\n" +
            $"FROM (\n{idListSql}\n) i\n" +
            $"INNER JOIN {_referenceTable} d ON d.{_referenceId} = i.id\n" +
            $"GROUP BY d.{_groupName}";

        return ReadGroupCounts(sql, out _);
    }

    /// <summary>
    /// The whole reference population split by group (the background distribution). Total includes
    /// NULL-group rows so <see cref="CohortBuildBreakdownBuckets.NotKnown"/> captures them.
    /// </summary>
    private CohortBuildBreakdownBuckets ComputeReferencePopulation()
    {
        var sql =
            $"SELECT d.{_groupName} grp, COUNT(DISTINCT d.{_referenceId}) n\n" +
            $"FROM {_referenceTable} d\n" +
            $"GROUP BY d.{_groupName}";

        var byGroup = ReadGroupCounts(sql, out var total);
        return CohortBuildBreakdownByGroupsReport.Split(total, byGroup, _lookup);
    }

    /// <summary>Runs a "group, count" query on the cache server; returns non-null groups and the grand total.</summary>
    private Dictionary<string, int> ReadGroupCounts(string sql, out int total)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        total = 0;
        using var con = _cacheDb.Server.GetConnection();
        con.Open();
        using var cmd = _cacheDb.Server.GetCommand(sql, con);
        cmd.CommandTimeout = _timeout;
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var n = Convert.ToInt32(r.GetValue(1));
            total += n; // includes any NULL-group rows in the denominator
            if (r.IsDBNull(0))
                continue;
            // ACCUMULATE: a case-sensitive DBMS can return 'A' and 'a' as separate rows, which our
            // case-insensitive key would otherwise silently overwrite
            var key = r.GetValue(0).ToString();
            result[key] = result.TryGetValue(key, out var existing) ? existing + n : n;
        }

        return result;
    }

    // RDMP prefixes cohort set names with "cic_<ID>_" (EnsureNamingConvention); cloning a cohort across
    // CICs stacks them (e.g. cic_18286_cic_18284_cic_17950_People in SHARE...). Strip them for display.
    private static readonly Regex CicPrefix = new(@"^(cic_\d+_)+", RegexOptions.Compiled);

    public static string CleanName(string name) => string.IsNullOrEmpty(name) ? "" : CicPrefix.Replace(name, "");

    /// <summary>
    /// True unless the identifier's SelectSQL is a plain (possibly qualified) reference to the
    /// physical column: the syntax helper's runtime name of the expression must equal the physical
    /// column's runtime name (a WHITELIST - anything else, including expressions the helper cannot
    /// parse, is treated as transformed).
    /// </summary>
    public static bool IsTransformedIdentifier(string selectSql, string physicalRuntimeName,
        IQuerySyntaxHelper syntax)
    {
        if (string.IsNullOrWhiteSpace(selectSql))
            return false; // no expression stored - the column itself is used

        try
        {
            return !string.Equals(syntax.GetRuntimeName(selectSql), physicalRuntimeName,
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return true; // unparseable = not a plain column reference
        }
    }

    private static string Sanitise(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
