// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Reflection;
using FAnsi;
using FAnsi.Discovery;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;

/// <summary>
///     Identifies databases belong to a specific .Database assembly that might need patching at Startup.
///     <para>If you are writing a plugin you should use <see cref="PluginPatcher" /> instead which is MEF discoverable</para>
/// </summary>
public interface IPatcher
{
    /// <summary>
    ///     Returns the assembly containing all the Sql scripts to run to bring the database up to the current version.  These
    ///     scripts
    ///     must be Embedded Resources and are usually in the same assembly as the <see cref="IPatcher" />.
    /// </summary>
    /// <returns></returns>
    Assembly GetDbAssembly();

    /// <summary>
    ///     The subdirectory of the assembly which contains the embedded resources (sql files to create/patch the database).
    /// </summary>
    string ResourceSubdirectory { get; }

    /// <summary>
    ///     The tier of the database, 1 for Catalogue and Data export, 2 for satellite optional databases (e.g. ANOStore) and 3
    ///     for plugin
    /// </summary>
    int Tier { get; }

    /// <summary>
    ///     The unique name for the assembly/subdirectory
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     The legacy name (if any) that this patcher might have been known by in the past
    /// </summary>
    string LegacyName { get; }

    /// <summary>
    ///     True if the <see cref="Patcher" /> contains SQL implementations only for
    ///     <see cref="DatabaseType.MicrosoftSQLServer" /> or false
    ///     if it can create tailored schemas for other <see cref="DatabaseType" />.
    /// </summary>
    bool SqlServerOnly { get; }


    /// <summary>
    ///     Returns the initial Sql to create <paramref name="db" /> with the appropriate SQL code
    /// </summary>
    /// <returns></returns>
    Patch GetInitialCreateScriptContents(DiscoveredDatabase db);

    /// <summary>
    ///     Returns all patches defined in the <see cref="Patcher" /> assembly.  This is probably a superset of the ones that
    ///     have been
    ///     run on the live database on which you want to patch
    /// </summary>
    /// <returns></returns>
    SortedDictionary<string, Patch> GetAllPatchesInAssembly(DiscoveredDatabase db);
}