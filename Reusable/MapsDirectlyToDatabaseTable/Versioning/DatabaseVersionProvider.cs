// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using System.Data.SqlClient;
using ReusableLibraryCode;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    /// <summary>
    /// Fetches the version number of a platform database.  Platform databases are defined in an .Database assembly (e.g. CatalogueLibrary.Database) and are
    /// synced to a host assembly (e.g. CatalogueLibrary) which contains the object definitions (IMapsDirectlyToDatabaseTable).  It is important that the 
    /// version numbers of the host assembly and database assembly match.  To this end when the database is deployed or patched (updated) the .Database assembly
    /// version is written into the database.  
    /// 
    /// <para>This prevents running mismatched versions of the RDMP software with out dated object definitions.</para>
    /// </summary>
    public class DatabaseVersionProvider
    {
        public static Version GetVersionFromDatabase(DbConnectionStringBuilder builder)
        {
            using (var con = DatabaseCommandHelper.GetConnection(builder))
            {
                con.Open();

                var cmd = DatabaseCommandHelper.GetCommand("Select [dbo].[GetSoftwareVersion]()", con);
                Version version;

                try
                {
                    version = new Version(cmd.ExecuteScalar().ToString());
                }
                catch (DbException e)
                {
                    if (e.Message.Contains("GetSoftwareVersion") && e.Message.ToLower().Contains("cannot find"))
                    {
                        CreateSoftwareVersionFunction(builder);
                        version = new Version(cmd.ExecuteScalar().ToString());
                    }
                    else
                        throw;
                }

                return version;
            }
        }

        private static void CreateSoftwareVersionFunction(DbConnectionStringBuilder builder)
        {
            const string sql = @"CREATE FUNCTION [dbo].[GetSoftwareVersion]()
RETURNS nvarchar(50)
AS
BEGIN
	-- Return the result of the function
	RETURN (SELECT top 1 version from RoundhousE.Version order by version desc)
END";

            using (var con = DatabaseCommandHelper.GetConnection(builder))
            {
                con.Open();
                var cmd = DatabaseCommandHelper.GetCommand(sql, con);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
