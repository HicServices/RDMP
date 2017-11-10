using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Xceed.Words.NET;

namespace CatalogueLibrary.Reports.DatabaseAccessPrivileges
{
    public class WordAccessRightsByDatabase:RequiresMicrosoftOffice
    {
        private readonly string _database;
        
        private readonly DiscoveredDatabase _dbInfo;

        //constructor
        public WordAccessRightsByDatabase(DiscoveredDatabase dbInfo)
        {
            _dbInfo = dbInfo;
            _database = _dbInfo.GetRuntimeName();
        }

        public void GenerateWordFile()
        {
            using (DocX document = DocX.Create("RightsByDatabase.docx"))
            {
                InsertTitle(document,"Database Access Report:" + _dbInfo.Server.Name);

                InsertHeader(document,"Administrators");

                CreateAdministratorsTable(document);

                foreach (string database in _dbInfo.Server.DiscoverDatabases().Select(d => d.GetRuntimeName()))
                {
                    if (database.Equals("master") || database.Equals("msdb") || database.Equals("tempdb") || database.Equals("ReportServer") || database.Equals("ReportServerTempDB"))
                        continue;

                    InsertHeader(document,"Database:" + database);
                    CreateDatabaseTable(document,database);
                }
            }
        }

     
        private void CreateAdministratorsTable(DocX document)
        {

            SqlConnection con = (SqlConnection) _dbInfo.Server.GetConnection();
            con.Open();
            SqlCommand cmdNumberOfAdmins = new SqlCommand("Select count(*) " + adminsFromAndWhere,con);

            int numberOfAdmins = int.Parse(cmdNumberOfAdmins.ExecuteScalar().ToString());

            Table table = document.AddTable(numberOfAdmins+1, 9);
            
            var fontSize = 5;
            table.AutoFit = AutoFit.Contents;
            
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

            SqlCommand cmdAdmins = new SqlCommand("Select * " + adminsFromAndWhere + " ORDER BY name",con);
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
            SqlConnection con = (SqlConnection) _dbInfo.Server.GetConnection();
            con.Open();
            SqlCommand cmdNumberOfUsers = GetCommandForDatabase(con, database, true);

            int numberOfUsers = int.Parse(cmdNumberOfUsers.ExecuteScalar().ToString());
            
            Table table = document.InsertTable(numberOfUsers + 1, 5);

            var fontSize = 5;
            table.AutoFit = AutoFit.Contents;

            int tableLine = 0;

            SetTableCell(table,tableLine, 0, "DatabaseName",fontSize);
            SetTableCell(table,tableLine, 1, "UserName",fontSize);
            SetTableCell(table,tableLine, 2, "Role",fontSize);
            SetTableCell(table,tableLine, 3, "PermissionType",fontSize);
            SetTableCell(table, tableLine, 4, "PermissionState", fontSize);
            tableLine++;

            SqlCommand cmdUsers = GetCommandForDatabase(con, database, false);
            SqlDataReader r = cmdUsers.ExecuteReader();

            while (r.Read())
            {
                SetTableCell(table,tableLine, 0,r["DatabaseName"].ToString(),fontSize);
                SetTableCell(table,tableLine, 1,r["DatabaseUserName"].ToString(),fontSize);
                SetTableCell(table,tableLine, 2,r["Role"].ToString(),fontSize);
                SetTableCell(table,tableLine, 3,r["PermissionType"].ToString(),fontSize);
                SetTableCell(table, tableLine, 4, r["PermissionState"].ToString(), fontSize);
                tableLine++;
            }

            r.Close();
            con.Close();
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
