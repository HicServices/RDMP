// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;

/// <summary>
///     Represents a Embedded Resource file in the up directory of a assembly found using an <see cref="IPatcher" />.  Used
///     during patching
///     to ensure that the live database that is about to be patched is in the expected state and ready for new patches to
///     be applied.
/// </summary>
public class Patch : IComparable
{
    public const string VersionKey = "--Version:";
    public const string DescriptionKey = "--Description:";

    public string EntireScript { get; set; }
    public string locationInAssembly { get; }

    public Version DatabaseVersionNumber { get; set; }
    public string Description { get; set; }


    public Patch(string scriptName, string entireScriptContents)
    {
        locationInAssembly = scriptName;
        EntireScript = entireScriptContents;

        ExtractDescriptionAndVersionFromScriptContents();
    }

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(Description))
            return $"Patch {DatabaseVersionNumber}";

        return Description.Length > 100
            ? $"Patch {DatabaseVersionNumber}({Description[..100]}...)"
            : $"Patch {DatabaseVersionNumber}({Description})";
    }

    private void ExtractDescriptionAndVersionFromScriptContents()
    {
        var lines = EntireScript.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        var idx = lines[0].IndexOf(VersionKey, StringComparison.Ordinal);

        if (idx == -1)
            throw new InvalidPatchException(locationInAssembly, $"Script does not start with {VersionKey}");

        var versionNumber = lines[0][(idx + VersionKey.Length)..].Trim(':', ' ', '\n', '\r', '/', '*');

        try
        {
            DatabaseVersionNumber = new Version(versionNumber);
        }
        catch (Exception e)
        {
            throw new InvalidPatchException(locationInAssembly,
                $"Could not process the scripts --Version: entry ('{versionNumber}') into a valid C# Version object",
                e);
        }

        if (lines.Length >= 2)
        {
            idx = lines[1].IndexOf(DescriptionKey, StringComparison.Ordinal);

            if (idx == -1)
                throw new InvalidPatchException(locationInAssembly,
                    $"Second line of patch scripts must start with {DescriptionKey}");

            var description = lines[1][(idx + DescriptionKey.Length)..].Trim(':', ' ', '\n', '\r', '/', '*');
            Description = description;
        }
    }

    /// <summary>
    ///     Returns the body without the header text "--Version:1.2.0 etc".
    /// </summary>
    /// <returns></returns>
    public string GetScriptBody()
    {
        return string.Join(Environment.NewLine,
            EntireScript.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(2));
    }

    public override int GetHashCode()
    {
        return locationInAssembly.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        var x = this;

        if (obj is not Patch y)
            return false;

        var equal = x.locationInAssembly.Equals(y.locationInAssembly);


        if (!equal)
            return false;

        return x.DatabaseVersionNumber.Equals(y.DatabaseVersionNumber)
            ? true
            : throw new InvalidPatchException(x.locationInAssembly,
                $"Patches x and y are being compared and they have the same location in assembly ({x.locationInAssembly})  but different Version numbers");
    }

    public int CompareTo(object obj)
    {
        if (obj is Patch patch)
            return -string.Compare(patch.locationInAssembly, locationInAssembly,
                StringComparison.Ordinal); //sort alphabetically (reverse)

        throw new Exception($"Cannot compare {GetType().Name} to {obj.GetType().Name}");
    }

    /// <summary>
    ///     Describes the state of a database schema when compared to the <see cref="IPatcher" /> which manages its schema
    /// </summary>
    public enum PatchingState
    {
        /// <summary>
        ///     Indicates that the running <see cref="IPatcher" /> has not detected any patches that require to be run on
        ///     the database schema
        /// </summary>
        NotRequired,

        /// <summary>
        ///     Indicates that the running <see cref="IPatcher" /> has identified patches that should be applied to the
        ///     database schema
        /// </summary>
        Required,

        /// <summary>
        ///     Indicates that the running <see cref="IPatcher" /> is older than the current database schema that is being
        ///     connected to
        /// </summary>
        SoftwareBehindDatabase
    }

    public static PatchingState IsPatchingRequired(DiscoveredDatabase database, IPatcher patcher,
        out Version databaseVersion, out Patch[] patchesInDatabase,
        out SortedDictionary<string, Patch> allPatchesInAssembly)
    {
        databaseVersion = DatabaseVersionProvider.GetVersionFromDatabase(database);

        var scriptExecutor = new MasterDatabaseScriptExecutor(database);
        patchesInDatabase = scriptExecutor.GetPatchesRun();

        allPatchesInAssembly = patcher.GetAllPatchesInAssembly(database);

        var databaseAssemblyName = patcher.GetDbAssembly().GetName();

        if (databaseAssemblyName.Version < databaseVersion)
            return PatchingState.SoftwareBehindDatabase;

        //if there are patches that have not been applied
        return
            allPatchesInAssembly.Values
                .Except(patchesInDatabase)
                .Any()
                ? PatchingState.Required
                : PatchingState.NotRequired;
    }
}