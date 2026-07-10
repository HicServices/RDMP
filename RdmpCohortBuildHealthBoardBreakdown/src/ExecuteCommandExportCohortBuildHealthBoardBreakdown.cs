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
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.CohortCreation;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Reproduces the Cohort Builder's per-set / per-container count tree (the FinalCount and cumulative
/// running totals shown as UNION/INTERSECT/EXCEPT are applied) split by a region column, using a
/// user-supplied lookup table to name/group the regions. Operates purely on the query cache: it builds
/// the cohort once to populate the per-set cache tables, then recomposes every count point from those
/// cache tables and splits it by region with one GROUP BY per node (all regions at once).
/// </summary>
public class ExecuteCommandExportCohortBuildHealthBoardBreakdown : BasicCommandExecution
{
    private readonly CohortIdentificationConfiguration _cic;
    private ICatalogue _demographyCatalogue;
    private ColumnInfo _regionColumn;
    private TableInfo _groupLookup;
    private readonly int _timeout;
    private FileInfo _toFile;

    private ExtractionInformation _idEi;
    private IQuerySyntaxHelper _syntax;
    private DiscoveredDatabase _cacheDb;
    private CachedAggregateConfigurationResultsManager _cacheManager;
    private RegionLookup _lookup;
    private string _demogTable;
    private string _demogId;
    private string _regionName;

    private readonly Dictionary<int, (string fqn, string col)> _setCacheTable = new();
    private readonly Dictionary<(bool isContainer, int id), (int final, int? cumulative)> _baseline = new();

    public ExecuteCommandExportCohortBuildHealthBoardBreakdown(IBasicActivateItems activator,
        [DemandsInitialization("The cohort identification configuration whose build tree to break down")]
        CohortIdentificationConfiguration cic,
        [DemandsInitialization("Demography catalogue that provides the patient identifier and the region column")]
        ICatalogue demographyCatalogue = null,
        [DemandsInitialization("The region column to group the breakdown by")]
        ColumnInfo regionColumn = null,
        [DemandsInitialization("Lookup table mapping region code to a name and node (columns: Region, HB_Name, SafeHaven_Region)")]
        TableInfo groupLookup = null,
        [DemandsInitialization("CSV file to write. Defaults to <cic>-build-breakdown.csv in the current directory")]
        FileInfo toFile = null,
        [DemandsInitialization("Per-query command timeout in seconds", DefaultValue = 5000)]
        int timeout = 5000) : base(activator)
    {
        _cic = cic;
        _demographyCatalogue = demographyCatalogue;
        _regionColumn = regionColumn;
        _groupLookup = groupLookup;
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

        // the demography catalogue / region column / lookup table are resolved (and prompted for, in the
        // GUI) in Execute, so the command can be added to a right-click menu with only the cohort selected.
    }

    /// <summary>Resolves the RDMP-object inputs, prompting the user for any not supplied. Returns false on cancel.</summary>
    private bool ResolveInputs()
    {
        _demographyCatalogue ??= SelectOne<Catalogue>("Demography catalogue (provides the patient identifier + region column)",
            BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>());
        if (_demographyCatalogue == null)
            return Fail("No demography catalogue was supplied");

        _idEi = _demographyCatalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .FirstOrDefault(e => e.IsExtractionIdentifier);
        if (_idEi == null)
            return Fail($"'{_demographyCatalogue}' has no IsExtractionIdentifier column to join the cohort on");

        _regionColumn ??= SelectOne("Region column to group by",
            _demographyCatalogue.GetAllExtractionInformation(ExtractionCategory.Any)
                .Select(e => e.ColumnInfo).Where(c => c != null).Distinct().ToArray());
        if (_regionColumn == null)
            return Fail("No region column was supplied");

        _groupLookup ??= SelectOne<TableInfo>("Region lookup table (Region, HB_Name, SafeHaven_Region)",
            BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>());
        if (_groupLookup == null)
            return Fail("No region lookup table was supplied");

        // co-location: the recompose + GROUP BY join runs on the cache server, so demography must be there
        var cacheServer = _cic.QueryCachingServer.Server;
        var demogServer = _regionColumn.TableInfo.Server;
        if (!string.IsNullOrWhiteSpace(cacheServer) && !string.IsNullOrWhiteSpace(demogServer)
            && !string.Equals(cacheServer.Trim(), demogServer.Trim(), StringComparison.OrdinalIgnoreCase))
            return Fail($"Region column is on server '{demogServer}' but the query cache is on '{cacheServer}'; " +
                        "the breakdown joins on the cache server, so they must be the same server.");

        return true;
    }

    private T SelectOne<T>(string prompt, T[] available) where T : class =>
        available.Length > 0 && BasicActivator.SelectObject(prompt, available, out var selected) ? selected : null;

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

        _syntax = _regionColumn.GetQuerySyntaxHelper();
        _cacheDb = _cic.QueryCachingServer.Discover(DataAccessContext.InternalDataProcessing);
        _cacheManager = new CachedAggregateConfigurationResultsManager(_cic.QueryCachingServer);
        _demogTable = _regionColumn.TableInfo.Name;
        _demogId = _syntax.EnsureWrapped(_idEi.GetRuntimeName());
        _regionName = _syntax.EnsureWrapped(_regionColumn.GetRuntimeName());

        // load the (user-defined) region -> name/node mapping from the lookup table
        _lookup = RegionLookup.LoadFrom(_groupLookup.Discover(DataAccessContext.InternalDataProcessing), _timeout);

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

        // 2. Walk the tree, recomposing each count point from the cache and splitting by region.
        var nodes = new List<CohortBuildBreakdownNode>();
        var seq = 0;
        Walk(_cic.RootCohortAggregateContainer, null, 0, nodes, ref seq);

        // reference: each region's share of the WHOLE demography population (a sanity-check row)
        var demographyReference = ComputeDemographyReference();

        CohortBuildHealthBoardBreakdownReport.WriteCsv(_toFile.FullName, nodes, _lookup, demographyReference);

        // reconciliation note: recognised-region counts should never exceed the unfiltered total
        var drift = nodes.Count(n =>
            n.FinalByRegion.Where(kv => _lookup.Contains(kv.Key)).Sum(kv => kv.Value) > n.FinalUnfiltered);
        var summary = $"Exported build breakdown to {_toFile.FullName} ({nodes.Count} count points)";
        if (drift > 0)
            summary += $" - WARNING: {drift} node(s) have region counts exceeding the unfiltered total (check keys)";
        BasicActivator.Show(summary);
    }

    private void Walk(CohortAggregateContainer container, CohortAggregateContainer parent, int indexInParent,
        List<CohortBuildBreakdownNode> nodes, ref int seq)
    {
        // container node row (cumulative is within its parent)
        var (cFinal, cCum) = _baseline.TryGetValue((true, container.ID), out var cb) ? cb : (0, null);
        IReadOnlyDictionary<string, int> cCumByRegion = null;
        if (parent != null && indexInParent > 0 && cCum.HasValue)
            cCumByRegion = RunRegionCounts(CumulativeSql(parent, indexInParent));

        nodes.Add(new CohortBuildBreakdownNode(seq++, "Container", CleanName(container.Name),
            CleanName(parent?.Name), container.Operation.ToString(), container.Order, cFinal,
            parent != null && indexInParent > 0 ? cCum : null,
            RunRegionCounts(IdSql(container)), cCumByRegion));

        var kids = EnabledOrdered(container);
        for (var i = 0; i < kids.Count; i++)
        {
            switch (kids[i])
            {
                case AggregateConfiguration agg:
                    var (aFinal, aCum) = _baseline.TryGetValue((false, agg.ID), out var ab) ? ab : (0, null);
                    IReadOnlyDictionary<string, int> aCumByRegion = null;
                    if (i > 0 && aCum.HasValue)
                        aCumByRegion = RunRegionCounts(CumulativeSql(container, i));

                    nodes.Add(new CohortBuildBreakdownNode(seq++, "Cohort Set", CleanName(agg.Name),
                        CleanName(container.Name), "", agg.Order, aFinal, i > 0 ? aCum : null,
                        RunRegionCounts(CachedSetSql(agg)), aCumByRegion));
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
        var op = $"\n{container.Operation}\n"; // UNION / INTERSECT / EXCEPT (the operators RDMP itself uses)
        return string.Join(op, children.Select(ch => $"({IdSql(ch)})"));
    }

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
            CohortAggregateContainer c => !c.IsDisabled,
            _ => true
        }).ToList();

    // --- run a GROUP BY Region join (on the cache server) for one count point ---

    private IReadOnlyDictionary<string, int> RunRegionCounts(string idListSql)
    {
        var sql =
            $"SELECT d.{_regionName} region, COUNT(DISTINCT i.id) n\n" +
            $"FROM (\n{idListSql}\n) i\n" +
            $"INNER JOIN {_demogTable} d ON d.{_demogId} = i.id\n" +
            $"GROUP BY d.{_regionName}";

        return ReadRegionCounts(sql, out _);
    }

    /// <summary>
    /// The whole demography population split by region (the reference/background distribution). Total
    /// includes NULL-region rows so <see cref="CohortBuildBreakdownBuckets.NotKnown"/> captures them.
    /// </summary>
    private CohortBuildBreakdownBuckets ComputeDemographyReference()
    {
        var sql =
            $"SELECT d.{_regionName} region, COUNT(DISTINCT d.{_demogId}) n\n" +
            $"FROM {_demogTable} d\n" +
            $"GROUP BY d.{_regionName}";

        var byRegion = ReadRegionCounts(sql, out var total);
        return CohortBuildHealthBoardBreakdownReport.Split(total, byRegion, _lookup);
    }

    /// <summary>Runs a "region, count" query on the cache server; returns non-null regions and the grand total.</summary>
    private Dictionary<string, int> ReadRegionCounts(string sql, out int total)
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
            total += n; // includes any NULL-region group in the denominator
            if (!r.IsDBNull(0))
                result[r.GetValue(0).ToString()] = n;
        }

        return result;
    }

    // RDMP prefixes cohort set names with "cic_<ID>_" (EnsureNamingConvention); cloning a cohort across
    // CICs stacks them (e.g. cic_18286_cic_18284_cic_17950_People in SHARE...). Strip them for display.
    private static readonly Regex CicPrefix = new(@"^(cic_\d+_)+", RegexOptions.Compiled);

    public static string CleanName(string name) => string.IsNullOrEmpty(name) ? "" : CicPrefix.Replace(name, "");

    private static string Sanitise(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
