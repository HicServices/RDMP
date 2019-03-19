// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data.Defaults
{
    /// <summary>
    /// Server defaults let you identify a role a server plays (e.g. IdentifierDumpServer) and make it the default one of it's type for all rows created which have an IdentifierDump.
    /// For example TableInfo.IdentifierDumpServer_ID defaults to whichever IdentifierDump ExternalDatabaseServer is configured (can be DBNull.Value).
    /// 
    /// <para>A scalar valued function GetDefaultExternalServerIDFor is used to retrieve defaults so that even if the user creates a new record in the TableInfo table himself manually without
    /// using our library (very dangerous anyway btw) it will still have the default.</para>
    /// </summary>
    public interface IServerDefaults
    {
        /// <summary>
        /// Pass in an enum to have it mapped to the scalar GetDefaultExternalServerIDFor function input that provides default values for columns that reference the given value - now note that this 
        /// might be a scalability issue at some point if there are multiple references from separate tables (or no references at all! like in DQE) 
        /// </summary>
        /// <param name="field"></param>
        /// <returns>the currently configured ExternalDatabaseServer the user wants to use as the default for the supplied role or null if no default has yet been picked</returns>
        IExternalDatabaseServer GetDefaultFor(PermissableDefaults field);
        
        /// <summary>
        /// Sets the database <paramref name="toDelete"/> default to null (not configured)
        /// </summary>
        /// <param name="toDelete"></param>
        void ClearDefault(PermissableDefaults toDelete);

        /// <summary>
        /// Changes the database <paramref name="toChange"/> default to the specified server
        /// </summary>
        /// <param name="toChange"></param>
        /// <param name="externalDatabaseServer"></param>
        void SetDefault(PermissableDefaults toChange, IExternalDatabaseServer externalDatabaseServer);
    }
}