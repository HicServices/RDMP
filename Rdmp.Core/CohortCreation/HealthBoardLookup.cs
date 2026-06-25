// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Rdmp.Core.CohortCreation;

/// <summary>
/// A Scottish health board: the single-letter <see cref="Region"/> cipher held in
/// SHARE_Demography, its numeric <see cref="HbCode"/> (null for legacy boards), its display
/// <see cref="Name"/>, and the safe-haven <see cref="Node"/> it rolls up to.
/// </summary>
public sealed record HealthBoard(string Region, int? HbCode, string Name, string Node);

/// <summary>
/// Hardcoded mapping from a SHARE_Demography <c>Region</c> cipher to its health board and
/// safe-haven node. Single source of truth for the cohort health-board breakdown report; an
/// unrecognised or NULL region resolves to a non-null "(unknown)" board under the
/// <see cref="UnknownNode"/> so counts are never silently dropped.
/// </summary>
public static class HealthBoardLookup
{
    /// <summary>Node assigned to any region cipher not present in the lookup (or NULL/empty).</summary>
    public const string UnknownNode = "Unknown";

    // keyed by the single-letter Region cipher held in SHARE_Demography.Region
    private static readonly Dictionary<string, HealthBoard> ByRegion = new(System.StringComparer.OrdinalIgnoreCase)
    {
        ["A"] = new("A", 11, "Ayrshire & Arran", "West"),
        ["B"] = new("B", 6, "Borders", "South East"),
        ["Y"] = new("Y", 12, "Dumfries & Galloway", "West"),
        ["F"] = new("F", 4, "Fife", "East"),
        ["V"] = new("V", 7, "Forth Valley", "East"),
        ["N"] = new("N", 2, "Grampian", "North"),
        ["G"] = new("G", 16, "Greater Glasgow & Clyde", "West"),
        ["H"] = new("H", 17, "Highland", "North"),
        ["L"] = new("L", 10, "Lanarkshire", "West"),
        ["S"] = new("S", 5, "Lothian", "South East"),
        ["R"] = new("R", 13, "Orkney", "North"),
        ["Z"] = new("Z", 14, "Shetland", "North"),
        ["T"] = new("T", 3, "Tayside", "East"),
        ["W"] = new("W", 15, "Western Isles", "North"),
        ["C"] = new("C", null, "Clyde", "West"), // legacy board: no numeric HB_Code (intentional)
    };

    /// <summary>
    /// Resolves a <c>Region</c> cipher to its <see cref="HealthBoard"/>. Unknown, NULL or empty
    /// ciphers map to a placeholder board (name "(unknown)", node <see cref="UnknownNode"/>)
    /// rather than null, so unmapped patients are reported and reconcile to the cohort total.
    /// </summary>
    public static HealthBoard Resolve(string region)
    {
        var key = region?.Trim();
        return !string.IsNullOrEmpty(key) && ByRegion.TryGetValue(key, out var hb)
            ? hb
            : new HealthBoard(key ?? "", null, "(unknown)", UnknownNode);
    }
}
