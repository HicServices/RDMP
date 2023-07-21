// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data.Defaults;

/// <summary>
///     Server defaults let you identify a role a server plays (e.g. IdentifierDumpServer) and make it the default one of
///     its type for all rows created which have an IdentifierDump.
///     For example TableInfo.IdentifierDumpServer_ID defaults to whichever IdentifierDump ExternalDatabaseServer is
///     configured (can be DBNull.Value).
/// </summary>
public interface IServerDefaults
{
    /// <summary>
    ///     Returns the default server for performing the activity
    /// </summary>
    /// <param name="field"></param>
    /// <returns>
    ///     the currently configured ExternalDatabaseServer the user wants to use as the default for the supplied role or
    ///     null if no default has yet been picked
    /// </returns>
    IExternalDatabaseServer GetDefaultFor(PermissableDefaults field);

    /// <summary>
    ///     Sets the database <paramref name="toDelete" /> default to null (not configured)
    /// </summary>
    /// <param name="toDelete"></param>
    void ClearDefault(PermissableDefaults toDelete);

    /// <summary>
    ///     Changes the database <paramref name="toChange" /> default to the specified server
    /// </summary>
    /// <param name="toChange"></param>
    /// <param name="externalDatabaseServer"></param>
    void SetDefault(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer);
}