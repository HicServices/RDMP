// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using FAnsi.Discovery;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;

/// <summary>
///     Fetches the version number of a platform database.  Platform databases managed by an IPatcher whose assembly
///     contains both the object definitions (IMapsDirectlyToDatabaseTable)
///     and the SQL schema / patches for the database.  It is important that the version numbers of the host assembly and
///     database match.  To this end when the database is deployed or
///     patched (updated) the IPatcher assembly version is written into the database.
///     <para>This prevents running mismatched versions of the RDMP software with out dated object definitions.</para>
/// </summary>
public class DatabaseVersionProvider
{
    public static Version GetVersionFromDatabase(DiscoveredDatabase database)
    {
        var tbl = database.ExpectTable(MasterDatabaseScriptExecutor.RoundhouseVersionTable,
            MasterDatabaseScriptExecutor.GetRoundhouseSchemaName(database));

        //versions in the database (should only be 1)
        var versions = tbl.GetDataTable().Rows.Cast<DataRow>().Select(r =>
                r["version"] == DBNull.Value ? new Version(0, 0, 0, 0) : new Version(r["version"].ToString()))
            .ToArray();

        return versions.Length == 0 ? new Version(0, 0, 0, 0) : versions.Max();
    }
}