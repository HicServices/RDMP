using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Reports.DatabaseAccessPrivileges
{
    public class WordAccessRightsByDatabase:RequiresMicrosoftOffice
    {
        private readonly string _database;
        #region stuff for Word
        object oTrue = true;
        object oFalse = false;
        Object oMissing = System.Reflection.Missing.Value;

        Microsoft.Office.Interop.Word.Application wrdApp;
        Microsoft.Office.Interop.Word._Document wrdDoc;
        #endregion


        /// <summary>
        /// Set this to true if you want microsoft word to be visible while it is running Interop commands (will be very confusing for users so never ship this with true)
        /// </summary>
        public bool DEBUG_WORD = false;

        private readonly DiscoveredDatabase _dbInfo;

        //constructor
        public WordAccessRightsByDatabase(DiscoveredDatabase dbInfo)
        {
            _dbInfo = dbInfo;
            _database = _dbInfo.GetRuntimeName();
        }

        public void GenerateWordFile()
        {

            // Create an instance of Word  and make it visible.=
            wrdApp = new Microsoft.Office.Interop.Word.Application();

            //normally we hide word and suppress popups but it might be that word is being broken in which case we would want to watch it as it outputs stuff
            if (!DEBUG_WORD)
            {
                wrdApp.Visible = false;
                wrdApp.DisplayAlerts = WdAlertLevel.wdAlertsNone;
            }
            else
            {
                wrdApp.Visible = true;
            }
            //add blank new word
            wrdDoc = wrdApp.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);

            try
            {
                wrdDoc.Select();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("RETRYLATER"))
                    Thread.Sleep(2000);

                wrdDoc.Select();
            }


            WordHelper wordHelper = new WordHelper(wrdApp);


            wordHelper.WriteLine("Database Access Report:" + _dbInfo.Server.Name, WdBuiltinStyle.wdStyleTitle);

            wordHelper.WriteLine("Administrators" , WdBuiltinStyle.wdStyleHeading1);
            CreateAdministratorsTable();
            
            wordHelper.GoToEndOfDocument();

            foreach (string database in _dbInfo.Server.DiscoverDatabases().Select(d => d.GetRuntimeName()))
            {
                if (database.Equals("master") || database.Equals("msdb") || database.Equals("tempdb") || database.Equals("ReportServer") || database.Equals("ReportServerTempDB"))
                    continue;

                wordHelper.WriteLine("Database:" + database , WdBuiltinStyle.wdStyleHeading1);
                CreateDatabaseTable(database);

                wordHelper.GoToEndOfDocument();

            }


            wrdApp.Visible = true;

        }

     
        private void CreateAdministratorsTable()
        {

            object start = wrdDoc.Content.End - 1;
            object end = wrdDoc.Content.End - 1;

            Range tableLocation = wrdDoc.Range(ref start, ref end);

            SqlConnection con = (SqlConnection) _dbInfo.Server.GetConnection();
            con.Open();
            SqlCommand cmdNumberOfAdmins = new SqlCommand("Select count(*) " + adminsFromAndWhere,con);

            int numberOfAdmins = int.Parse(cmdNumberOfAdmins.ExecuteScalar().ToString());


            Table table = wrdDoc.Tables.Add(tableLocation, numberOfAdmins+1, 9);
            
            table.set_Style("Table Grid");
            table.Range.Font.Size = 5;
            table.AllowAutoFit = true;

            int tableLine = 1;

            table.Cell(tableLine, 1).Range.Text = "name";
            table.Cell(tableLine, 2).Range.Text = "sysadmin";
            table.Cell(tableLine, 3).Range.Text = "securityadmin";
            table.Cell(tableLine, 4).Range.Text = "serveradmin";
            table.Cell(tableLine, 5).Range.Text = "setupadmin";
            table.Cell(tableLine, 6).Range.Text = "processadmin";
            table.Cell(tableLine, 7).Range.Text = "diskadmin";
            table.Cell(tableLine, 8).Range.Text = "dbcreator";
            table.Cell(tableLine, 9).Range.Text = "bulkadmin";
            tableLine++;

            SqlCommand cmdAdmins = new SqlCommand("Select * " + adminsFromAndWhere + " ORDER BY name",con);
            SqlDataReader r = cmdAdmins.ExecuteReader();

            while (r.Read())
            {
                table.Cell(tableLine, 1).Range.Text = r["name"].ToString();
                table.Cell(tableLine, 2).Range.Text = r["sysadmin"].ToString();
                table.Cell(tableLine, 3).Range.Text = r["securityadmin"].ToString();
                table.Cell(tableLine, 4).Range.Text = r["serveradmin"].ToString();
                table.Cell(tableLine, 5).Range.Text = r["setupadmin"].ToString();
                table.Cell(tableLine, 6).Range.Text = r["processadmin"].ToString();
                table.Cell(tableLine, 7).Range.Text = r["diskadmin"].ToString();
                table.Cell(tableLine, 8).Range.Text = r["dbcreator"].ToString();
                table.Cell(tableLine, 9).Range.Text = r["bulkadmin"].ToString();
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
            

        private void CreateDatabaseTable(string database)
        {
   
            object start = wrdDoc.Content.End - 1;
            object end = wrdDoc.Content.End - 1;

            Range tableLocation = wrdDoc.Range(ref start, ref end);

            SqlConnection con = (SqlConnection) _dbInfo.Server.GetConnection();
            con.Open();
            SqlCommand cmdNumberOfUsers = GetCommandForDatabase(con, database, true);


            int numberOfUsers = int.Parse(cmdNumberOfUsers.ExecuteScalar().ToString());


            Table table = wrdDoc.Tables.Add(tableLocation, numberOfUsers + 1, 5);

            table.set_Style("Table Grid");
            table.Range.Font.Size = 5;
            table.AllowAutoFit = true;

            int tableLine = 1;

            table.Cell(tableLine, 1).Range.Text = "DatabaseName";
            table.Cell(tableLine, 2).Range.Text = "UserName";
            table.Cell(tableLine, 3).Range.Text = "Role";
            table.Cell(tableLine, 4).Range.Text = "PermissionType";
            table.Cell(tableLine, 5).Range.Text = "PermissionState";
            tableLine++;

            SqlCommand cmdUsers = GetCommandForDatabase(con, database, false);
            SqlDataReader r = cmdUsers.ExecuteReader();

            while (r.Read())
            {
                table.Cell(tableLine, 1).Range.Text = r["DatabaseName"].ToString();
                table.Cell(tableLine, 2).Range.Text = r["DatabaseUserName"].ToString();
                table.Cell(tableLine, 3).Range.Text = r["Role"].ToString();
                table.Cell(tableLine, 4).Range.Text = r["PermissionType"].ToString();
                table.Cell(tableLine, 5).Range.Text = r["PermissionState"].ToString();
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
