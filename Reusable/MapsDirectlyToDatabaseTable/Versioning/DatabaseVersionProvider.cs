using System;
using System.Data.Common;
using System.Data.SqlClient;
using ReusableLibraryCode;

namespace MapsDirectlyToDatabaseTable.Versioning
{
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