// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace RdmpCohortBuildBreakdownByGroups;

/// <summary>
/// Projects a cohort build's count tree (the per-set / per-container <c>FinalCount</c> and cumulative
/// running totals shown in the Cohort Builder) split by group into a WIDE CSV: one row per
/// (count-point x metric), the container/set name written once, a <c>Total</c> column (RDMP's own
/// unfiltered count), one column per group recognised by the supplied <see cref="GroupLookup"/>, an
/// <c>Other</c> column (present codes the lookup does not recognise) and a <c>NotKnown</c> residual
/// (patients not in the reference table / NULL group). Two bottom rows give each group's share of the
/// final cohort and of the whole reference population (a sanity check). Groups + Other + NotKnown reconcile
/// to Total on every row.
/// </summary>
public static class CohortBuildBreakdownByGroupsReport
{
    public const string OtherColumn = "Other";
    public const string NotKnownColumn = "NotKnown";
    public const string PercentMetric = "% of final cohort";

    /// <summary>Label for the reference row: each group's share of the whole reference population.</summary>
    public const string ReferencePercentMetric = "% of reference population";

    /// <summary>A resolved output column (a group recognised by the lookup).</summary>
    private sealed record GroupColumn(string Code, string Name, string Node);

    /// <summary>
    /// Splits one node's group counts into Total / recognised-groups / Other / NotKnown using
    /// <paramref name="lookup"/>. <paramref name="byRegion"/> is the GROUP BY Region result (every present
    /// code); <paramref name="total"/> is RDMP's own count.
    /// </summary>
    public static CohortBuildBreakdownBuckets Split(int total, IReadOnlyDictionary<string, int> byRegion,
        GroupLookup lookup)
    {
        var groups = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        var other = 0;
        foreach (var (code, n) in byRegion)
            if (lookup.Contains(code))
                groups[code] = n;
            else
                other += n; // present but not recognised by the lookup

        return new CohortBuildBreakdownBuckets(total, groups, other, total - groups.Values.Sum() - other);
    }

    /// <summary>The recognised groups that appear anywhere, ordered by node (nulls last) then name.</summary>
    private static List<GroupColumn> GroupColumns(IEnumerable<CohortBuildBreakdownNode> nodes,
        CohortBuildBreakdownBuckets referencePopulation, GroupLookup lookup) =>
        nodes
            .SelectMany(n => n.FinalByGroup.Keys.Concat(n.CumulativeByGroup?.Keys ?? Enumerable.Empty<string>()))
            .Concat(referencePopulation?.Groups.Keys ?? Enumerable.Empty<string>())
            .Where(lookup.Contains)
            .GroupBy(code => code, System.StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupColumn(g.Key, lookup.LabelOf(g.Key), lookup.GroupingOf(g.Key)))
            .OrderBy(c => string.IsNullOrEmpty(c.Node) ? 1 : 0) // ungrouped codes (grouping NULL) last
            .ThenBy(c => c.Node, System.StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Name, System.StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>
    /// Builds the wide CSV (data rows per node+metric, then a <c>% of final cohort</c> row and, when
    /// <paramref name="referencePopulation"/> is supplied, a <c>% of reference population</c> row underneath it as
    /// a cohort-vs-population sanity check).
    /// </summary>
    public static string ToCsv(IReadOnlyList<CohortBuildBreakdownNode> nodes, GroupLookup lookup,
        CohortBuildBreakdownBuckets referencePopulation = null)
    {
        var ordered = nodes.OrderBy(n => n.Seq).ToList();
        var columns = GroupColumns(ordered, referencePopulation, lookup);

        var header = new List<string> { "Order", "Type", "Name", "Container", "SetOperation", "Metric", "Total" };
        header.AddRange(columns.Select(c => c.Name));
        header.Add(OtherColumn);
        header.Add(NotKnownColumn);

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", header.Select(Escape)));

        foreach (var n in ordered)
        {
            AppendCountRow(sb, n, columns, "Final", Split(n.FinalUnfiltered, n.FinalByGroup, lookup));
            if (n.CumulativeUnfiltered.HasValue && n.CumulativeByGroup != null)
                AppendCountRow(sb, n, columns, "Cumulative",
                    Split(n.CumulativeUnfiltered.Value, n.CumulativeByGroup, lookup));
        }

        // bottom: % of final cohort (root node's Final), then % of reference population, after a blank separator
        var root = ordered.FirstOrDefault(n => string.IsNullOrEmpty(n.Container)) ?? ordered.FirstOrDefault();
        if (root != null && root.FinalUnfiltered > 0)
        {
            sb.AppendLine();
            sb.AppendLine(string.Join(",", header.Select(Escape))); // repeat header so % aligns to each group
            AppendPercentRow(sb, PercentMetric, columns, Split(root.FinalUnfiltered, root.FinalByGroup, lookup));
            if (referencePopulation != null)
                AppendPercentRow(sb, ReferencePercentMetric, columns, referencePopulation);
        }

        return sb.ToString();
    }

    private static void AppendPercentRow(StringBuilder sb, string label, List<GroupColumn> columns,
        CohortBuildBreakdownBuckets b)
    {
        double Pct(int v) => b.Total == 0 ? 0 : v * 100.0 / b.Total;
        var cells = new List<string> { "", "", label, "", "", label, Fmt(b.Total == 0 ? 0 : 100.0) };
        cells.AddRange(columns.Select(c => Fmt(Pct(b.Groups.TryGetValue(c.Code, out var v) ? v : 0))));
        cells.Add(Fmt(Pct(b.Other)));
        cells.Add(Fmt(Pct(b.NotKnown)));
        sb.AppendLine(string.Join(",", cells.Select(Escape)));
    }

    private static void AppendCountRow(StringBuilder sb, CohortBuildBreakdownNode n, List<GroupColumn> columns,
        string metric, CohortBuildBreakdownBuckets b)
    {
        var cells = new List<string>
        {
            n.DisplayOrder.ToString(CultureInfo.InvariantCulture),
            n.Type, n.Name, n.Container, n.SetOperation, metric,
            b.Total.ToString(CultureInfo.InvariantCulture)
        };
        cells.AddRange(columns.Select(c =>
            (b.Groups.TryGetValue(c.Code, out var v) ? v : 0).ToString(CultureInfo.InvariantCulture)));
        cells.Add(b.Other.ToString(CultureInfo.InvariantCulture));
        cells.Add(b.NotKnown.ToString(CultureInfo.InvariantCulture));
        sb.AppendLine(string.Join(",", cells.Select(Escape)));
    }

    public static void WriteCsv(string path, IReadOnlyList<CohortBuildBreakdownNode> nodes, GroupLookup lookup,
        CohortBuildBreakdownBuckets referencePopulation = null) =>
        File.WriteAllText(path, ToCsv(nodes, lookup, referencePopulation));

    private static string Fmt(double d) => d.ToString("0.0", CultureInfo.InvariantCulture);

    private static string Escape(string field)
    {
        field ??= "";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
