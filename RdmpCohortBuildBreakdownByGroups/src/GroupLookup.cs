// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FAnsi.Discovery;

namespace Rdmp.Core.CohortCreation;

/// <summary>
/// A user-defined mapping from a group code to a display label and an optional grouping node, loaded
/// at runtime from a lookup table whose key/label/grouping columns are supplied by the caller (nothing
/// is hard-coded). A NULL grouping is allowed (e.g. codes that have a label but no higher grouping).
/// Codes absent from the table are treated as unmapped ("Other").
/// </summary>
public sealed class GroupLookup
{
    private readonly Dictionary<string, (string Label, string Grouping)> _map;

    public GroupLookup(IReadOnlyDictionary<string, (string Label, string Grouping)> map) =>
        _map = new Dictionary<string, (string, string)>(map, StringComparer.OrdinalIgnoreCase);

    /// <summary>True if the code is present in the lookup (a recognised group).</summary>
    public bool Contains(string code) => code != null && _map.ContainsKey(code.Trim());

    /// <summary>Display label for the code, or null if unmapped.</summary>
    public string LabelOf(string code) =>
        code != null && _map.TryGetValue(code.Trim(), out var v) ? v.Label : null;

    /// <summary>Grouping node for the code (may be null even for a mapped code), or null if unmapped.</summary>
    public string GroupingOf(string code) =>
        code != null && _map.TryGetValue(code.Trim(), out var v) ? v.Grouping : null;

    /// <summary>
    /// Reads the mapping from a lookup <paramref name="table"/> using the supplied runtime column
    /// names. Rows with a NULL/blank key are skipped; a NULL label falls back to the code; a NULL
    /// grouping is preserved. <paramref name="groupingColumn"/> may be null (no grouping at all).
    /// </summary>
    public static GroupLookup LoadFrom(DiscoveredTable table, string keyColumn, string labelColumn,
        string groupingColumn, int timeout)
    {
        var syntax = table.Database.Server.GetQuerySyntaxHelper();
        var groupingSelect = groupingColumn == null ? "" : $", {syntax.EnsureWrapped(groupingColumn)}";
        var sql =
            $"SELECT {syntax.EnsureWrapped(keyColumn)}, {syntax.EnsureWrapped(labelColumn)}{groupingSelect} " +
            $"FROM {table.GetFullyQualifiedName()}";

        var map = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
        using var con = table.Database.Server.GetConnection();
        con.Open();
        using var cmd = table.Database.Server.GetCommand(sql, con);
        cmd.CommandTimeout = timeout;
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var code = r.IsDBNull(0) ? null : r.GetValue(0).ToString()?.Trim();
            if (string.IsNullOrEmpty(code))
                continue;
            var label = r.IsDBNull(1) ? code : r.GetValue(1).ToString();
            var grouping = groupingColumn != null && !r.IsDBNull(2) ? r.GetValue(2).ToString() : null;
            map[code] = (label, grouping);
        }

        return new GroupLookup(map);
    }
}
