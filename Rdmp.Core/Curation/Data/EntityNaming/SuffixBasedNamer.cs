// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace Rdmp.Core.Curation.Data.EntityNaming;

/// <summary>
///     Determines how to translate a TABLE (not database!) name based on the load stage of a DLE RAW=>STAGING=>LIVE
///     migration.  E.g. Raw tables
///     already have the same name as Live tables (they are in a RAW database) but Staging tables and Archive tables have
///     the suffixes specified
/// </summary>
public class SuffixBasedNamer : INameDatabasesAndTablesDuringLoads
{
    protected static Dictionary<LoadBubble, string> Suffixes = new()
    {
        { LoadBubble.Raw, "" },
        { LoadBubble.Staging, "_STAGING" },
        { LoadBubble.Live, "" },
        { LoadBubble.Archive, "_Archive" }
    };

    /// <inheritdoc />
    public virtual string GetDatabaseName(string rootDatabaseName, LoadBubble stage)
    {
        return stage switch
        {
            LoadBubble.Raw => $"{rootDatabaseName}_RAW",
            LoadBubble.Staging => $"{rootDatabaseName}_STAGING",
            LoadBubble.Live => rootDatabaseName,
            _ => throw new ArgumentOutOfRangeException(nameof(stage))
        };
    }

    /// <inheritdoc />
    public virtual string GetName(string tableName, LoadBubble convention)
    {
        return !Suffixes.TryGetValue(convention, out var s)
            ? throw new ArgumentException($"Do not have a suffix for convention: {convention}")
            : $"{tableName}{s}";
    }
}