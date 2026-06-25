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
/// running totals shown in the Cohort Builder) split by health board into a long, file-friendly CSV:
/// one row per (board × count-point). The boards partition the cohort (1 patient ↔ 1 board), so an
/// <c>Unfiltered</c> pseudo-board (RDMP's own counts) and an <c>Unknown</c> board (patients with no /
/// unmapped region) bracket the real boards and reconcile to the unfiltered total at every node.
/// </summary>
public static class CohortBuildHealthBoardBreakdownReport
{
    public const string UnfilteredBoard = "Unfiltered";
    public const string UnknownBoard = "Unknown";

    /// <summary>One count point of the build tree with its per-region counts (known boards only).</summary>
    public sealed class NodeBreakdown
    {
        /// <summary>Tree walk order (stable, used to order rows within a board).</summary>
        public int Seq { get; init; }

        /// <summary>"Cohort Set" or "Container".</summary>
        public string Type { get; init; } = "";

        public string Name { get; init; } = "";

        /// <summary>Name of the parent container (empty for the root).</summary>
        public string Container { get; init; } = "";

        /// <summary>UNION / INTERSECT / EXCEPT for containers; empty for sets.</summary>
        public string SetOperation { get; init; } = "";

        public int DisplayOrder { get; init; }

        /// <summary>RDMP's own count for this node (the unfiltered total).</summary>
        public int FinalUnfiltered { get; init; }

        /// <summary>RDMP's own cumulative within the parent container; null if not applicable.</summary>
        public int? CumulativeUnfiltered { get; init; }

        /// <summary>Region cipher → final count (known boards only; GROUP BY Region result).</summary>
        public IReadOnlyDictionary<string, int> FinalByRegion { get; init; } = new Dictionary<string, int>();

        /// <summary>Region cipher → cumulative count; null when this node has no cumulative.</summary>
        public IReadOnlyDictionary<string, int> CumulativeByRegion { get; init; }
    }

    public sealed class Row
    {
        public string Board { get; init; } = "";
        public string Node { get; init; } = "";
        public int Order { get; init; }
        public string Type { get; init; } = "";
        public string Name { get; init; } = "";
        public string Container { get; init; } = "";
        public string SetOperation { get; init; } = "";
        public int? FinalCount { get; init; }
        public int? CumulativeCount { get; init; }
    }

    private static readonly string[] Header =
    {
        "Board", "Node", "Order", "Type", "Name", "Container", "SetOperation", "FinalCount", "CumulativeCount"
    };

    /// <summary>
    /// Flattens the per-node breakdowns into long rows, board-major: the <see cref="UnfilteredBoard"/>
    /// tree first, then each real board's full tree (every board appears at every node, 0 where absent),
    /// then the <see cref="UnknownBoard"/> tree last. Within a board, rows follow the tree walk order.
    /// </summary>
    public static IReadOnlyList<Row> BuildRows(IReadOnlyList<NodeBreakdown> nodes)
    {
        var ordered = nodes.OrderBy(n => n.Seq).ToList();

        // every region cipher seen anywhere → its board; keep a deterministic board ordering
        var boards = ordered
            .SelectMany(n => n.FinalByRegion.Keys)
            .Select(r => HealthBoardLookup.Resolve(r))
            .Where(b => b.Node != HealthBoardLookup.UnknownNode)
            .GroupBy(b => b.Region, System.StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(b => b.Node, System.StringComparer.OrdinalIgnoreCase)
            .ThenBy(b => b.Name, System.StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rows = new List<Row>();

        // 1) Unfiltered (RDMP's own counts)
        foreach (var n in ordered)
            rows.Add(MakeRow(n, UnfilteredBoard, "", n.FinalUnfiltered, n.CumulativeUnfiltered));

        // 2) each real board
        foreach (var b in boards)
            foreach (var n in ordered)
            {
                var fin = n.FinalByRegion.TryGetValue(b.Region, out var f) ? f : 0;
                int? cum = n.CumulativeByRegion == null
                    ? null
                    : n.CumulativeByRegion.TryGetValue(b.Region, out var c) ? c : 0;
                rows.Add(MakeRow(n, b.Name, b.Node, fin, cum));
            }

        // 3) Unknown = unfiltered − Σ known boards (patients not in demography / unmapped region)
        foreach (var n in ordered)
        {
            var knownFinal = n.FinalByRegion
                .Where(kv => HealthBoardLookup.Resolve(kv.Key).Node != HealthBoardLookup.UnknownNode)
                .Sum(kv => kv.Value);
            var finalUnknown = n.FinalUnfiltered - knownFinal;

            int? cumUnknown = null;
            if (n.CumulativeUnfiltered.HasValue && n.CumulativeByRegion != null)
            {
                var knownCum = n.CumulativeByRegion
                    .Where(kv => HealthBoardLookup.Resolve(kv.Key).Node != HealthBoardLookup.UnknownNode)
                    .Sum(kv => kv.Value);
                cumUnknown = n.CumulativeUnfiltered.Value - knownCum;
            }

            rows.Add(MakeRow(n, UnknownBoard, UnknownBoard, finalUnknown, cumUnknown));
        }

        return rows;
    }

    private static Row MakeRow(NodeBreakdown n, string board, string node, int? final, int? cumulative) => new()
    {
        Board = board,
        Node = node,
        Order = n.DisplayOrder,
        Type = n.Type,
        Name = n.Name,
        Container = n.Container,
        SetOperation = n.SetOperation,
        FinalCount = final,
        CumulativeCount = cumulative
    };

    public static string ToCsv(IEnumerable<Row> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", Header.Select(Escape)));
        foreach (var r in rows)
            sb.AppendLine(string.Join(",", new[]
            {
                r.Board, r.Node,
                r.Order.ToString(CultureInfo.InvariantCulture),
                r.Type, r.Name, r.Container, r.SetOperation,
                r.FinalCount?.ToString(CultureInfo.InvariantCulture) ?? "",
                r.CumulativeCount?.ToString(CultureInfo.InvariantCulture) ?? ""
            }.Select(Escape)));
        return sb.ToString();
    }

    public static void WriteCsv(string path, IReadOnlyList<NodeBreakdown> nodes) =>
        File.WriteAllText(path, ToCsv(BuildRows(nodes)));

    private static string Escape(string field)
    {
        field ??= "";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
