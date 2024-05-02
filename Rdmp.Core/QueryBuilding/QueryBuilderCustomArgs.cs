// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     Describes a number of ways in which a standard query built by a <see cref="CohortQueryBuilderHelper" /> can be
///     modified e.g. to show TopX of the cohort only or
///     to select all columns instead of just the cohort identifier (e.g. for previewing to the matched patient's records).
/// </summary>
public class QueryBuilderCustomArgs
{
    public string OverrideSelectList { get; set; }
    public string OverrideLimitationSQL { get; set; }
    public int TopX { get; set; } = -1;

    public QueryBuilderCustomArgs(string overrideSelectList, string overrideLimitationSQL, int topX)
    {
        OverrideSelectList = overrideSelectList;
        OverrideLimitationSQL = overrideLimitationSQL;
        TopX = topX;
    }

    public QueryBuilderCustomArgs()
    {
    }

    /// <summary>
    ///     Populates <paramref name="other" /> with the values stored in this
    /// </summary>
    /// <param name="other"></param>
    public void Populate(QueryBuilderCustomArgs other)
    {
        other.OverrideLimitationSQL = OverrideLimitationSQL;
        other.OverrideSelectList = OverrideSelectList;
        other.TopX = TopX;
    }
}