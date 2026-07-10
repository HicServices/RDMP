// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace Rdmp.Core.CohortCreation;

/// <summary>
/// One count point of a cohort build tree (a cohort set or a container) with the per-region counts that
/// were computed for it. Used by <see cref="CohortBuildHealthBoardBreakdownReport"/>.
/// </summary>
public sealed class CohortBuildBreakdownNode
{
    /// <summary>Tree walk order (stable, used to order rows).</summary>
    public int Seq { get; }

    /// <summary>"Cohort Set" or "Container".</summary>
    public string Type { get; }

    public string Name { get; }

    /// <summary>Name of the parent container (empty for the root).</summary>
    public string Container { get; }

    /// <summary>UNION / INTERSECT / EXCEPT for containers; empty for sets.</summary>
    public string SetOperation { get; }

    public int DisplayOrder { get; }

    /// <summary>RDMP's own count for this node (the unfiltered/national total).</summary>
    public int FinalUnfiltered { get; }

    /// <summary>RDMP's own cumulative within the parent container; null if not applicable.</summary>
    public int? CumulativeUnfiltered { get; }

    /// <summary>Region code -> final count (every present code; the GROUP BY Region result).</summary>
    public IReadOnlyDictionary<string, int> FinalByRegion { get; }

    /// <summary>Region code -> cumulative count; null when this node has no cumulative.</summary>
    public IReadOnlyDictionary<string, int> CumulativeByRegion { get; }

    public CohortBuildBreakdownNode(int seq, string type, string name, string container, string setOperation,
        int displayOrder, int finalUnfiltered, int? cumulativeUnfiltered,
        IReadOnlyDictionary<string, int> finalByRegion, IReadOnlyDictionary<string, int> cumulativeByRegion)
    {
        Seq = seq;
        Type = type ?? "";
        Name = name ?? "";
        Container = container ?? "";
        SetOperation = setOperation ?? "";
        DisplayOrder = displayOrder;
        FinalUnfiltered = finalUnfiltered;
        CumulativeUnfiltered = cumulativeUnfiltered;
        FinalByRegion = finalByRegion ?? new Dictionary<string, int>();
        CumulativeByRegion = cumulativeByRegion;
    }
}

/// <summary>
/// The Total / per-region / Other / NotKnown split of one node+metric, relative to a
/// <see cref="RegionLookup"/>. <see cref="Regions"/> holds the counts for codes present in the lookup;
/// <see cref="Other"/> sums present codes absent from the lookup; <see cref="NotKnown"/> is the residual
/// (not in demography, or NULL region).
/// </summary>
public sealed class CohortBuildBreakdownBuckets
{
    public int Total { get; }

    /// <summary>Region code -> count, for codes recognised by the lookup.</summary>
    public IReadOnlyDictionary<string, int> Regions { get; }

    /// <summary>Sum of present region codes that the lookup does not recognise.</summary>
    public int Other { get; }

    /// <summary>Total - recognised - Other = not in demography + NULL region.</summary>
    public int NotKnown { get; }

    public CohortBuildBreakdownBuckets(int total, IReadOnlyDictionary<string, int> regions, int other, int notKnown)
    {
        Total = total;
        Regions = regions ?? new Dictionary<string, int>();
        Other = other;
        NotKnown = notKnown;
    }
}
