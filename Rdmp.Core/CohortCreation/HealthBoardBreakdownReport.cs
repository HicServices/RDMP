// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Rdmp.Core.CohortCreation;

/// <summary>
/// Turns a per-<c>Region</c> count (the <c>SELECT Region, COUNT(DISTINCT chi) ... GROUP BY Region</c>
/// result of a cohort's final inclusion list joined to SHARE_Demography) into a flat, file-friendly
/// breakdown: one row per health board, a subtotal per safe-haven node, and a grand total. Region
/// ciphers are mapped to boards/nodes via <see cref="HealthBoardLookup"/>; unmapped or NULL regions
/// are reported under the <see cref="HealthBoardLookup.UnknownNode"/> bucket so the rows reconcile to
/// the cohort size.
/// </summary>
public static class HealthBoardBreakdownReport
{
    /// <summary>The kind of row: a single board, a per-node subtotal, or the grand total.</summary>
    public enum RowKind
    {
        Board,
        NodeSubtotal,
        GrandTotal
    }

    /// <summary>One row of the breakdown.</summary>
    public sealed class BreakdownRecord
    {
        public RowKind Kind { get; init; }

        /// <summary>Region cipher (board rows only; empty for subtotal/total rows).</summary>
        public string Region { get; init; } = "";

        /// <summary>Numeric health board code, where one exists (board rows only).</summary>
        public int? HbCode { get; init; }

        /// <summary>Display name: the board name, "&lt;Node&gt; - total", or "TOTAL".</summary>
        public string HbName { get; init; } = "";

        /// <summary>Safe-haven node (board + subtotal rows; empty for the grand total).</summary>
        public string Node { get; init; } = "";

        /// <summary>Distinct patient count.</summary>
        public int Count { get; init; }
    }

    private static readonly string[] Header = { "Region", "HBCode", "HBName", "Node", "Count" };

    /// <summary>
    /// Builds the ordered breakdown records from a region/count table. Boards are grouped by node
    /// (nodes alphabetical, <see cref="HealthBoardLookup.UnknownNode"/> last; boards within a node
    /// alphabetical), each node followed by its subtotal, then a final grand total. The
    /// <paramref name="dt"/> is expected to have a region column and an integer count column.
    /// </summary>
    public static IReadOnlyList<BreakdownRecord> BuildRecords(DataTable dt, string regionColumn = "Region",
        string countColumn = "n")
    {
        // Collapse to one count per resolved board (defensive: a region could appear twice if the
        // source query did not group cleanly, and several ciphers/NULLs all fold into Unknown).
        var boards = new Dictionary<string, (HealthBoard Hb, int Count)>(StringComparer.OrdinalIgnoreCase);

        foreach (DataRow row in dt.Rows)
        {
            var regionVal = row[regionColumn] == DBNull.Value ? null : row[regionColumn]?.ToString();
            var hb = HealthBoardLookup.Resolve(regionVal);
            var count = row[countColumn] == DBNull.Value
                ? 0
                : Convert.ToInt32(row[countColumn], CultureInfo.InvariantCulture);

            // key on the resolved board (Region for known boards, "" for the single Unknown bucket)
            var key = hb.Node == HealthBoardLookup.UnknownNode ? "\0unknown" : hb.Region;
            if (boards.TryGetValue(key, out var existing))
                boards[key] = (existing.Hb, existing.Count + count);
            else
                boards[key] = (hb, count);
        }

        var records = new List<BreakdownRecord>();

        // node ordering: alphabetical, Unknown always last
        var nodes = boards.Values
            .Select(b => b.Hb.Node)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n == HealthBoardLookup.UnknownNode ? 1 : 0)
            .ThenBy(n => n, StringComparer.OrdinalIgnoreCase);

        var grandTotal = 0;

        foreach (var node in nodes)
        {
            var inNode = boards.Values
                .Where(b => string.Equals(b.Hb.Node, node, StringComparison.OrdinalIgnoreCase))
                .OrderBy(b => b.Hb.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var (hb, count) in inNode)
                records.Add(new BreakdownRecord
                {
                    Kind = RowKind.Board,
                    Region = hb.Region,
                    HbCode = hb.HbCode,
                    HbName = hb.Name,
                    Node = hb.Node,
                    Count = count
                });

            var nodeTotal = inNode.Sum(b => b.Count);
            grandTotal += nodeTotal;

            records.Add(new BreakdownRecord
            {
                Kind = RowKind.NodeSubtotal,
                HbName = $"{node} - total",
                Node = node,
                Count = nodeTotal
            });
        }

        records.Add(new BreakdownRecord
        {
            Kind = RowKind.GrandTotal,
            HbName = "TOTAL",
            Count = grandTotal
        });

        return records;
    }

    /// <summary>Serialises records to CSV (with a header row).</summary>
    public static string ToCsv(IEnumerable<BreakdownRecord> records)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", Header.Select(Escape)));

        foreach (var r in records)
            sb.AppendLine(string.Join(",", new[]
            {
                r.Region,
                r.HbCode?.ToString(CultureInfo.InvariantCulture) ?? "",
                r.HbName,
                r.Node,
                r.Count.ToString(CultureInfo.InvariantCulture)
            }.Select(Escape)));

        return sb.ToString();
    }

    /// <summary>Convenience: build the records from a region/count table and write the CSV.</summary>
    public static void WriteCsv(string path, DataTable dt, string regionColumn = "Region",
        string countColumn = "n") =>
        File.WriteAllText(path, ToCsv(BuildRecords(dt, regionColumn, countColumn)));

    private static string Escape(string field)
    {
        field ??= "";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
