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
/// A user-defined mapping from a region code to a display name and a grouping node, loaded at runtime
/// from a lookup table (rather than hard-coded). The lookup table is expected to have the columns named
/// by <see cref="RegionColumn"/>, <see cref="NameColumn"/> and <see cref="NodeColumn"/>; a NULL
/// <see cref="NodeColumn"/> is allowed (e.g. non-Scottish / administrative codes that still have a name
/// but no safe-haven node). Codes absent from the table are treated as unmapped ("Other").
/// </summary>
public sealed class RegionLookup
{
    /// <summary>Column holding the region code that appears in the demography data.</summary>
    public const string RegionColumn = "Region";

    /// <summary>Column holding the display name for a region code.</summary>
    public const string NameColumn = "HB_Name";

    /// <summary>Column holding the grouping node (used to order columns); may be NULL.</summary>
    public const string NodeColumn = "SafeHaven_Region";

    private readonly Dictionary<string, (string Name, string Node)> _map;

    public RegionLookup(IReadOnlyDictionary<string, (string Name, string Node)> map) =>
        _map = new Dictionary<string, (string, string)>(map, StringComparer.OrdinalIgnoreCase);

    /// <summary>True if the code is present in the lookup (a recognised region).</summary>
    public bool Contains(string code) => code != null && _map.ContainsKey(code.Trim());

    /// <summary>Display name for the code, or null if unmapped.</summary>
    public string NameOf(string code) =>
        code != null && _map.TryGetValue(code.Trim(), out var v) ? v.Name : null;

    /// <summary>Grouping node for the code (may be null even for a mapped code), or null if unmapped.</summary>
    public string NodeOf(string code) =>
        code != null && _map.TryGetValue(code.Trim(), out var v) ? v.Node : null;

    /// <summary>
    /// Reads the mapping from a lookup <paramref name="table"/> (columns <see cref="RegionColumn"/> /
    /// <see cref="NameColumn"/> / <see cref="NodeColumn"/>). Rows with a NULL/blank region code are skipped;
    /// a NULL name falls back to the code; a NULL node is preserved.
    /// </summary>
    public static RegionLookup LoadFrom(DiscoveredTable table, int timeout)
    {
        var syntax = table.Database.Server.GetQuerySyntaxHelper();
        var sql =
            $"SELECT {syntax.EnsureWrapped(RegionColumn)}, {syntax.EnsureWrapped(NameColumn)}, " +
            $"{syntax.EnsureWrapped(NodeColumn)} FROM {table.GetFullyQualifiedName()}";

        var map = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);
        using var con = table.Database.Server.GetConnection();
        con.Open();
        using var cmd = table.Database.Server.GetCommand(sql, con);
        cmd.CommandTimeout = timeout;
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var code = r[0] == DBNull.Value ? null : r[0].ToString()?.Trim();
            if (string.IsNullOrEmpty(code))
                continue;
            var name = r[1] == DBNull.Value ? code : r[1].ToString();
            var node = r[2] == DBNull.Value ? null : r[2].ToString();
            map[code] = (name, node);
        }

        return new RegionLookup(map);
    }
}
