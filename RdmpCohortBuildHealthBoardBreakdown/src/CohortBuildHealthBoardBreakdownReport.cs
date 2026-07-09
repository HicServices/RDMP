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
/// running totals shown in the Cohort Builder) split by health board into a WIDE CSV: one row per
/// (count-point × metric), the container/set name written once, a <c>Total</c> column (RDMP's own
/// national count), one column per Scottish health board, an <c>Other</c> column (present non-Scottish /
/// unmapped region codes) and a <c>NotKnown</c> residual (patients not in demography / NULL region).
/// A bottom <c>% of final cohort</c> row gives each board's share of the final national cohort.
/// Boards + Other + NotKnown reconcile to Total on every row.
/// </summary>
public static class CohortBuildHealthBoardBreakdownReport
{
    public const string OtherColumn = "Other";
    public const string NotKnownColumn = "NotKnown";
    public const string PercentMetric = "% of final cohort";

    /// <summary>Label for the reference row: each board's share of the whole demography population.</summary>
    public const string DemographyPercentMetric = "% of demography";

    /// <summary>One count point of the build tree with its per-region counts (known boards only).</summary>
    public sealed class NodeBreakdown
    {
        public int Seq { get; init; }
        public string Type { get; init; } = "";
        public string Name { get; init; } = "";

        /// <summary>Parent container name (empty for the root).</summary>
        public string Container { get; init; } = "";

        public string SetOperation { get; init; } = "";
        public int DisplayOrder { get; init; }

        /// <summary>RDMP's own count for this node (the unfiltered/national total).</summary>
        public int FinalUnfiltered { get; init; }

        /// <summary>RDMP's own cumulative within the parent container; null if not applicable.</summary>
        public int? CumulativeUnfiltered { get; init; }

        /// <summary>Region cipher → final count (every present code; GROUP BY Region result).</summary>
        public IReadOnlyDictionary<string, int> FinalByRegion { get; init; } = new Dictionary<string, int>();

        /// <summary>Region cipher → cumulative count; null when this node has no cumulative.</summary>
        public IReadOnlyDictionary<string, int> CumulativeByRegion { get; init; }
    }

    /// <summary>The Total / per-board / Other / NotKnown counts for one node+metric.</summary>
    public sealed class Buckets
    {
        public int Total { get; init; }

        /// <summary>Region cipher → count (mapped Scottish boards only).</summary>
        public IReadOnlyDictionary<string, int> Boards { get; init; } = new Dictionary<string, int>();

        /// <summary>Sum of present region codes that are NOT one of the 15 Scottish boards.</summary>
        public int Other { get; init; }

        /// <summary>Total − boards − Other = not-in-demography + NULL region.</summary>
        public int NotKnown { get; init; }
    }

    /// <summary>
    /// Splits one node's region counts into Total / mapped-boards / Other / NotKnown. <paramref name="byRegion"/>
    /// is the GROUP BY Region result (every present code); <paramref name="total"/> is RDMP's own count.
    /// </summary>
    public static Buckets Split(int total, IReadOnlyDictionary<string, int> byRegion)
    {
        var boards = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        var other = 0;
        foreach (var (code, n) in byRegion)
            if (HealthBoardLookup.Resolve(code).Node == HealthBoardLookup.UnknownNode)
                other += n; // present but not a Scottish board (non-Scottish / unmapped)
            else
                boards[code] = n;

        return new Buckets
        {
            Total = total,
            Boards = boards,
            Other = other,
            NotKnown = total - boards.Values.Sum() - other
        };
    }

    /// <summary>The ordered mapped boards that appear anywhere (column order: node then name).</summary>
    private static List<HealthBoard> BoardColumns(IEnumerable<NodeBreakdown> nodes, Buckets demographyReference) =>
        nodes
            .SelectMany(n => n.FinalByRegion.Keys.Concat(n.CumulativeByRegion?.Keys ?? Enumerable.Empty<string>()))
            .Concat(demographyReference?.Boards.Keys ?? Enumerable.Empty<string>())
            .Select(HealthBoardLookup.Resolve)
            .Where(b => b.Node != HealthBoardLookup.UnknownNode)
            .GroupBy(b => b.Region, System.StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(b => b.Node, System.StringComparer.OrdinalIgnoreCase)
            .ThenBy(b => b.Name, System.StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>
    /// Builds the wide CSV (data rows per node+metric, then a <c>% of final cohort</c> row and, when
    /// <paramref name="demographyReference"/> is supplied, a <c>% of demography</c> row underneath it
    /// giving each board's share of the whole demography population, as a sanity check).
    /// </summary>
    public static string ToCsv(IReadOnlyList<NodeBreakdown> nodes, Buckets demographyReference = null)
    {
        var ordered = nodes.OrderBy(n => n.Seq).ToList();
        var boards = BoardColumns(ordered, demographyReference);

        var header = new List<string> { "Order", "Type", "Name", "Container", "SetOperation", "Metric", "Total" };
        header.AddRange(boards.Select(b => b.Name));
        header.Add(OtherColumn);
        header.Add(NotKnownColumn);

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", header.Select(Escape)));

        foreach (var n in ordered)
        {
            AppendCountRow(sb, n, boards, "Final", Split(n.FinalUnfiltered, n.FinalByRegion));
            if (n.CumulativeUnfiltered.HasValue && n.CumulativeByRegion != null)
                AppendCountRow(sb, n, boards, "Cumulative",
                    Split(n.CumulativeUnfiltered.Value, n.CumulativeByRegion));
        }

        // bottom: % of final cohort (root node's Final), then % of demography, after a blank separator
        var root = ordered.FirstOrDefault(n => string.IsNullOrEmpty(n.Container)) ?? ordered.FirstOrDefault();
        if (root != null && root.FinalUnfiltered > 0)
        {
            sb.AppendLine();
            sb.AppendLine(string.Join(",", header.Select(Escape))); // repeat header so % aligns to each board
            AppendPercentRow(sb, PercentMetric, boards, Split(root.FinalUnfiltered, root.FinalByRegion));
            if (demographyReference != null)
                AppendPercentRow(sb, DemographyPercentMetric, boards, demographyReference);
        }

        return sb.ToString();
    }

    private static void AppendPercentRow(StringBuilder sb, string label, List<HealthBoard> boards, Buckets b)
    {
        double Pct(int v) => b.Total == 0 ? 0 : v * 100.0 / b.Total;
        var cells = new List<string> { "", "", label, "", "", label, Fmt(b.Total == 0 ? 0 : 100.0) };
        cells.AddRange(boards.Select(bd => Fmt(Pct(b.Boards.TryGetValue(bd.Region, out var v) ? v : 0))));
        cells.Add(Fmt(Pct(b.Other)));
        cells.Add(Fmt(Pct(b.NotKnown)));
        sb.AppendLine(string.Join(",", cells.Select(Escape)));
    }

    private static void AppendCountRow(StringBuilder sb, NodeBreakdown n, List<HealthBoard> boards,
        string metric, Buckets b)
    {
        var cells = new List<string>
        {
            n.DisplayOrder.ToString(CultureInfo.InvariantCulture),
            n.Type, n.Name, n.Container, n.SetOperation, metric,
            b.Total.ToString(CultureInfo.InvariantCulture)
        };
        cells.AddRange(boards.Select(bd =>
            (b.Boards.TryGetValue(bd.Region, out var v) ? v : 0).ToString(CultureInfo.InvariantCulture)));
        cells.Add(b.Other.ToString(CultureInfo.InvariantCulture));
        cells.Add(b.NotKnown.ToString(CultureInfo.InvariantCulture));
        sb.AppendLine(string.Join(",", cells.Select(Escape)));
    }

    public static void WriteCsv(string path, IReadOnlyList<NodeBreakdown> nodes, Buckets demographyReference = null) =>
        File.WriteAllText(path, ToCsv(nodes, demographyReference));

    private static string Fmt(double d) => d.ToString("0.0", CultureInfo.InvariantCulture);

    private static string Escape(string field)
    {
        field ??= "";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
