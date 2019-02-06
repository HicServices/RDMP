// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FAnsi.Discovery;
using Xceed.Words.NET;

namespace CatalogueLibrary.Reports.DatabaseAccessPrivileges
{
    /// <summary>
    /// Generates a historic report of which user accounts have access to which databases by database (requires AccessRightsReportPrerequisites to have been run on 
    /// your database server an for the snapshotting stored proceedure to have been called at least once)
    /// </summary>
    public class WordAccessRightsByDatabase:RequiresMicrosoftOffice
    {
        private readonly string _database;
        
        private readonly DiscoveredDatabase _dbInfo;

        /// <summary>
        /// Prepares to generate a report based on the Audit recorded in the supplied Audit database <paramref name="dbInfo"/> <see cref="AccessRightsReportPrerequisites"/>
        /// </summary>
        /// <param name="dbInfo">The Audit database</param>
        public WordAccessRightsByDatabase(DiscoveredDatabase dbInfo)
        {
            _dbInfo = dbInfo;
            _database = _dbInfo.GetRuntimeName();
        }

        /// <summary>
        /// Creates a new word document in a temp folder which contains aggregate data about which users have access to which databases.
        /// </summary>
        public void GenerateWordFile()
        {
            var f = GetUniqueFilenameInWorkArea("RightsByDatabase");

            using (DocX document = DocX.Create(f.FullName))
            {
                InsertHeader(document,"Database Access Report:" + _dbInfo.Server.Name);

                InsertHeader(document,"Administrators");

                CreateAdministratorsTable(document);

                foreach (string database in _dbInfo.Server.DiscoverDatabases().Select(d => d.GetRuntimeName()))
                {
                    if (database.Equals("master") || database.Equals("msdb") || database.Equals("tempdb") || database.Equals("ReportServer") || database.Equals("ReportServerTempDB"))
                        continue;

                    InsertHeader(document,"Database:" + database);
                    CreateDatabaseTable(document,database);
                }

                document.Save();
                ShowFile(f);
            }
        }

     
        private void CreateAdministratorsTable(DocX document)
        {

            using (var con = (SqlConnection) _dbInfo.Server.GetConnection())
            {
                con.Open();
                SqlCommand cmdNumberOfAdmins = new SqlCommand("Select count(*) " + adminsFromAndWhere, con);

                int numberOfAdmins = int.Parse(cmdNumberOfAdmins.ExecuteScalar().ToString());

                Table table = InsertTable(document, numberOfAdmins + 1, 9);

                var fontSize = 5;

                int tableLine = 0;

                SetTableCell(table, tableLine, 0, "name", fontSize);
                SetTableCell(table, tableLine, 1, "sysadmin", fontSize);
                SetTableCell(table, tableLine, 2, "securityadmin", fontSize);
                SetTableCell(table, tableLine, 3, "serveradmin", fontSize);
                SetTableCell(table, tableLine, 4, "setupadmin", fontSize);
                SetTableCell(table, tableLine, 5, "processadmin", fontSize);
                SetTableCell(table, tableLine, 6, "diskadmin", fontSize);
                SetTableCell(table, tableLine, 7, "dbcreator", fontSize);
                SetTableCell(table, tableLine, 8, "bulkadmin", fontSize);
                tableLine++;

                SqlCommand cmdAdmins = new SqlCommand("Select * " + adminsFromAndWhere + " ORDER BY name", con);
                SqlDataReader r = cmdAdmins.ExecuteReader();

                while (r.Read())
                {
                    SetTableCell(table, tableLine, 0, r["name"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 1, r["sysadmin"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 2, r["securityadmin"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 3, r["serveradmin"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 4, r["setupadmin"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 5, r["processadmin"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 6, r["diskadmin"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 7, r["dbcreator"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 8, r["bulkadmin"].ToString(), fontSize);
                    tableLine++;
                }

                r.Close();
                con.Close();
            }
            
        }

        private string adminsFromAndWhere {
            get
            {
                return @" from " + _database + @".[dbo].[Logins] where name not like '%##%' and  name not in ('NT AUTHORITY\SYSTEM', 'NT SERVICE\MSSQLSERVER', 'NT SERVICE\SQLSERVERAGENT') 
AND  hasaccess = 1 AND denylogin = 0 
AND sysadmin + securityadmin + serveradmin + setupadmin + processadmin + diskadmin + dbcreator + bulkadmin > 0 ";
            }
        }
            

        private void CreateDatabaseTable(DocX document, string database)
        {
            using (var con = (SqlConnection) _dbInfo.Server.GetConnection())
            {
                con.Open();
                SqlCommand cmdNumberOfUsers = GetCommandForDatabase(con, database, true);

                int numberOfUsers = int.Parse(cmdNumberOfUsers.ExecuteScalar().ToString());

                Table table = InsertTable(document, numberOfUsers + 1, 5);

                var fontSize = 5;

                int tableLine = 0;

                SetTableCell(table, tableLine, 0, "DatabaseName", fontSize);
                SetTableCell(table, tableLine, 1, "UserName", fontSize);
                SetTableCell(table, tableLine, 2, "Role", fontSize);
                SetTableCell(table, tableLine, 3, "PermissionType", fontSize);
                SetTableCell(table, tableLine, 4, "PermissionState", fontSize);
                tableLine++;

                SqlCommand cmdUsers = GetCommandForDatabase(con, database, false);
                SqlDataReader r = cmdUsers.ExecuteReader();

                while (r.Read())
                {
                    SetTableCell(table, tableLine, 0, r["DatabaseName"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 1, r["DatabaseUserName"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 2, r["Role"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 3, r["PermissionType"].ToString(), fontSize);
                    SetTableCell(table, tableLine, 4, r["PermissionState"].ToString(), fontSize);
                    tableLine++;
                }

                r.Close();
                con.Close();
            }
        }

        private SqlCommand GetCommandForDatabase(SqlConnection con,string database,bool countOnly)
        {
            string selectList = @"
  SELECT  [DatabaseName],
      CASE WHEN LEN([UserName]) > LEN([DatabaseUserName]) THEN UserName ELSE [DatabaseUserName] END AS DatabaseUserName
      ,[UserType]
      ,[Role]
      ,[PermissionType]
      ,[PermissionState]
      ,[ObjectType]
      ,[ObjectName]
      ,[ColumnName]
      ,[validFrom]";

            string fromBit = @"
  FROM "+_database+@".[dbo].[DatabasePermissions]
where (UserName IS NULL OR (UserName NOT LIKE '%##%' AND UserName NOT IN ('NT AUTHORITY\SYSTEM', 'NT SERVICE\MSSQLSERVER', 'NT SERVICE\SQLSERVERAGENT')))
and DatabaseName NOT IN ('msdb', 'ReportServer', 'ReportServerTempDB')
AND (ObjectName IS NULL OR ObjectName NOT IN ('fn_diagramobjects', 'sp_alterdiagram', 'sp_creatediagram', 'sp_dropdiagram', 'sp_helpdiagramdefinition', 'sp_helpdiagrams', 'sp_renamediagram'))
AND ObjectName IS NULL
AND DatabaseName = @DatabaseName
AND CASE WHEN LEN([UserName]) > LEN([DatabaseUserName]) THEN UserName ELSE [DatabaseUserName] END NOT IN ('dbo')";
            
            string orderBit = @"
ORDER BY UserType, CASE WHEN LEN([UserName]) > LEN([DatabaseUserName]) THEN UserName ELSE [DatabaseUserName] END, DatabaseName, ObjectName, PermissionState
";

            if (countOnly)
            {
                SqlCommand cmd = new  SqlCommand("select count(*) " + fromBit,con);
                cmd.Parameters.Add("@DatabaseName", SqlDbType.VarChar, 500);
                cmd.Parameters["@DatabaseName"].Value = database;
                
                return cmd;

            }
            else
            {
                SqlCommand cmd = new SqlCommand(selectList + fromBit + orderBit ,con);
                cmd.Parameters.Add("@DatabaseName", SqlDbType.VarChar, 500);
                cmd.Parameters["@DatabaseName"].Value = database;

                return cmd;
            }
        }
    }
}
