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
    /// This prevents running mismatched versions of the RDMP software with out dated object definitions.
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