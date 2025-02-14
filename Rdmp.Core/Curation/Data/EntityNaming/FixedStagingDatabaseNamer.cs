// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.Curation.Data.EntityNaming;

/// <summary>
/// Used when there is a single staging database used for multiple different Catalogues. The name of the database being loaded is prepended to the staging table name.
/// </summary>
public class FixedStagingDatabaseNamer : SuffixBasedNamer
{
    private readonly string _stagingDatabaseName;
    private readonly string _databaseName;

    /// <summary>
    /// <para>---</para>
    /// <para>For 'Staging', returns the table name prefixed with <paramref name="databaseName"/> and suffixed with _STAGING</para>
    /// <para>---</para>
    /// <para>For others, appends:</para>
    /// <para>_Archive for Archive</para>
    /// </summary>
    public FixedStagingDatabaseNamer(string databaseName, string stagingDatabaseName = "DLE_STAGING")
    {
        _databaseName = EnsureValueIsNotWrapped(databaseName);
        _stagingDatabaseName = EnsureValueIsNotWrapped(stagingDatabaseName);
    }

    /// <inheritdoc/>
    public override string GetName(string tableName, LoadBubble convention) => convention == LoadBubble.Staging
        ? $"{_databaseName}_{tableName}{Suffixes[convention]}"
        : base.GetName(tableName, convention);

    /// <inheritdoc/>
    public override string GetDatabaseName(string rootDatabaseName, LoadBubble stage) => stage == LoadBubble.Staging
        ? _stagingDatabaseName
        : base.GetDatabaseName(rootDatabaseName, stage);

    /// <summary>
    /// Returns the unwrapped value of <paramref name="s"/> by trimming brackets and quotes
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static string EnsureValueIsNotWrapped(string s)
    {
        if (s == null)
            return null;

        var toReturn = s.Trim('[', ']', '`', '"');

        return toReturn.Contains('[') ||
               toReturn.Contains(']') ||
               toReturn.Contains('\'')
            ? throw new Exception(
                $"Attempted to strip wrapping from {s} but result was {toReturn} which contains invalid characters like [ and ], possibly original string was a multipart identifier? e.g. [MyTable].dbo.[Bob]?")
            : toReturn;
    }
}