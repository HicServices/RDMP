// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data.EntityNaming;

/// <summary>
///     Provides service for determining/checking a table's name at a particular stage in the load process, as the same
///     canonical table name may be different at different stages.
///     For example, the 'Data' table may be called 'Data' in live but 'LoadID_Data_STAGING' in staging if a single staging
///     database is being used for all data loads
/// </summary>
public interface INameDatabasesAndTablesDuringLoads
{
    /// <summary>
    ///     Gets the database name to give to the LIVE database during the given DLE load stage (e.g. RAW / STAGING/) e.g.
    ///     STAGING might always be DLE_STAGING regardless of the
    ///     LIVE database
    /// </summary>
    /// <param name="rootDatabaseName">The LIVE database name</param>
    /// <param name="convention">The stage for which you want to know the corresponding database name</param>
    /// <returns></returns>
    string GetDatabaseName(string rootDatabaseName, LoadBubble convention);

    /// <summary>
    ///     Determines what name to give to passed LIVE table in the given DLE load bubble (e.g. RAW / STAGING)
    /// </summary>
    /// <param name="tableName">The LIVE table name</param>
    /// <param name="convention">
    ///     The stage for which you want to know the corresponding tables name, this may not change at all
    ///     depending on implementation
    /// </param>
    /// <returns></returns>
    string GetName(string tableName, LoadBubble convention);
}