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

namespace Rdmp.Core.CohortCreation;

/// <summary>
/// Projects a cohort build's count tree (the per-set / per-container <c>FinalCount</c> and cumulative
/// running totals shown in the Cohort Builder) split by region into a WIDE CSV: one row per
/// (count-point x metric), the container/set name written once, a <c>Total</c> column (RDMP's own
/// national count), one column per region recognised by the supplied <see cref="RegionLookup"/>, an
/// <c>Other</c> column (present codes the lookup does not recognise) and a <c>NotKnown</c> residual
/// (patients not in demography / NULL region). Two bottom rows give each region's share of the final
/// cohort and of the whole demography population (a sanity check). Regions + Other + NotKnown reconcile
/// to Total on every row.
/// </summary>
public static class CohortBuildHealthBoardBreakdownReport
{
    public const string OtherColumn = "Other";
    public const string NotKnownColumn = "NotKnown";
    public const string PercentMetric = "% of final cohort";

    /// <summary>Label for the reference row: each region's share of the whole demography population.</summary>
    public const string DemographyPercentMetric = "% of demography";

    /// <summary>A resolved output column (a region recognised by the lookup).</summary>
    private sealed record RegionColumn(string Code, string Name, string Node);

    /// <summary>
    /// Splits one node's region counts into Total / recognised-regions / Other / NotKnown using
    /// <paramref name="lookup"/>. <paramref name="byRegion"/> is the GROUP BY Region result (every present
    /// code); <paramref name="total"/> is RDMP's own count.
    /// </summary>
    public static CohortBuildBreakdownBuckets Split(int total, IReadOnlyDictionary<string, int> byRegion,
        RegionLookup lookup)
    {
        var regions = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        var other = 0;
        foreach (var (code, n) in byRegion)
            if (lookup.Contains(code))
                regions[code] = n;
            else
                other += n; // present but not recognised by the lookup

        return new CohortBuildBreakdownBuckets(total, regions, other, total - regions.Values.Sum() - other);
    }

    /// <summary>The recognised regions that appear anywhere, ordered by node (nulls last) then name.</summary>
    private static List<RegionColumn> RegionColumns(IEnumerable<CohortBuildBreakdownNode> nodes,
        CohortBuildBreakdownBuckets demographyReference, RegionLookup lookup) =>
        nodes
            .SelectMany(n => n.FinalByRegion.Keys.Concat(n.CumulativeByRegion?.Keys ?? Enumerable.Empty<string>()))
            .Concat(demographyReference?.Regions.Keys ?? Enumerable.Empty<string>())
            .Where(lookup.Contains)
            .GroupBy(code => code, System.StringComparer.OrdinalIgnoreCase)
            .Select(g => new RegionColumn(g.Key, lookup.NameOf(g.Key), lookup.NodeOf(g.Key)))
            .OrderBy(c => string.IsNullOrEmpty(c.Node) ? 1 : 0) // nodeless regions (e.g. non-Scottish) last
            .ThenBy(c => c.Node, System.StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Name, System.StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>
    /// Builds the wide CSV (data rows per node+metric, then a <c>% of final cohort</c> row and, when
    /// <paramref name="demographyReference"/> is supplied, a <c>% of demography</c> row underneath it as
    /// a cohort-vs-population sanity check).
    /// </summary>
    public static string ToCsv(IReadOnlyList<CohortBuildBreakdownNode> nodes, RegionLookup lookup,
        CohortBuildBreakdownBuckets demographyReference = null)
    {
        var ordered = nodes.OrderBy(n => n.Seq).ToList();
        var columns = RegionColumns(ordered, demographyReference, lookup);

        var header = new List<string> { "Order", "Type", "Name", "Container", "SetOperation", "Metric", "Total" };
        header.AddRange(columns.Select(c => c.Name));
        header.Add(OtherColumn);
        header.Add(NotKnownColumn);

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", header.Select(Escape)));

        foreach (var n in ordered)
        {
            AppendCountRow(sb, n, columns, "Final", Split(n.FinalUnfiltered, n.FinalByRegion, lookup));
            if (n.CumulativeUnfiltered.HasValue && n.CumulativeByRegion != null)
                AppendCountRow(sb, n, columns, "Cumulative",
                    Split(n.CumulativeUnfiltered.Value, n.CumulativeByRegion, lookup));
        }

        // bottom: % of final cohort (root node's Final), then % of demography, after a blank separator
        var root = ordered.FirstOrDefault(n => string.IsNullOrEmpty(n.Container)) ?? ordered.FirstOrDefault();
        if (root != null && root.FinalUnfiltered > 0)
        {
            sb.AppendLine();
            sb.AppendLine(string.Join(",", header.Select(Escape))); // repeat header so % aligns to each region
            AppendPercentRow(sb, PercentMetric, columns, Split(root.FinalUnfiltered, root.FinalByRegion, lookup));
            if (demographyReference != null)
                AppendPercentRow(sb, DemographyPercentMetric, columns, demographyReference);
        }

        return sb.ToString();
    }

    private static void AppendPercentRow(StringBuilder sb, string label, List<RegionColumn> columns,
        CohortBuildBreakdownBuckets b)
    {
        double Pct(int v) => b.Total == 0 ? 0 : v * 100.0 / b.Total;
        var cells = new List<string> { "", "", label, "", "", label, Fmt(b.Total == 0 ? 0 : 100.0) };
        cells.AddRange(columns.Select(c => Fmt(Pct(b.Regions.TryGetValue(c.Code, out var v) ? v : 0))));
        cells.Add(Fmt(Pct(b.Other)));
        cells.Add(Fmt(Pct(b.NotKnown)));
        sb.AppendLine(string.Join(",", cells.Select(Escape)));
    }

    private static void AppendCountRow(StringBuilder sb, CohortBuildBreakdownNode n, List<RegionColumn> columns,
        string metric, CohortBuildBreakdownBuckets b)
    {
        var cells = new List<string>
        {
            n.DisplayOrder.ToString(CultureInfo.InvariantCulture),
            n.Type, n.Name, n.Container, n.SetOperation, metric,
            b.Total.ToString(CultureInfo.InvariantCulture)
        };
        cells.AddRange(columns.Select(c =>
            (b.Regions.TryGetValue(c.Code, out var v) ? v : 0).ToString(CultureInfo.InvariantCulture)));
        cells.Add(b.Other.ToString(CultureInfo.InvariantCulture));
        cells.Add(b.NotKnown.ToString(CultureInfo.InvariantCulture));
        sb.AppendLine(string.Join(",", cells.Select(Escape)));
    }

    public static void WriteCsv(string path, IReadOnlyList<CohortBuildBreakdownNode> nodes, RegionLookup lookup,
        CohortBuildBreakdownBuckets demographyReference = null) =>
        File.WriteAllText(path, ToCsv(nodes, lookup, demographyReference));

    private static string Fmt(double d) => d.ToString("0.0", CultureInfo.InvariantCulture);

    private static string Escape(string field)
    {
        field ??= "";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
