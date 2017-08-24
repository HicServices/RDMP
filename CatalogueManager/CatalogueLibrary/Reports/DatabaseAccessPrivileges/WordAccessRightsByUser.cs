using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Reports.DatabaseAccessPrivileges
{
    public class WordAccessRightsByUser:RequiresMicrosoftOffice
    {
        public string Server { get; set; }

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

        private DiscoveredDatabase _dbInfo;
        private readonly bool _currentUsersOnly;

        public WordAccessRightsByUser(DiscoveredDatabase dbInfo, bool currentUsersOnly)
        {
            _dbInfo = dbInfo;
            _currentUsersOnly = currentUsersOnly;
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


            wordHelper.WriteLine("Database Access Report:" + Server, WdBuiltinStyle.wdStyleTitle);

            SqlConnection con = (SqlConnection)_dbInfo.Server.GetConnection();
            con.Open();

            foreach (string user in UserAccounts())
            {

                wordHelper.WriteLine(user, WdBuiltinStyle.wdStyleHeading1);

             
                SqlCommand cmd = getAdministratorStatus(user,con);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    wordHelper.WriteLine("Administrator Status", WdBuiltinStyle.wdStyleHeading2);
                    QueryToWordTable(reader, false);
                }

                if(!reader.IsClosed)
                    reader.Close();

                wordHelper.GoToEndOfDocument();

                cmd = getDatabaseAccessStatus(user, con);

                reader = cmd.ExecuteReader();

                wordHelper.WriteLine("Specific Database Access Rights", WdBuiltinStyle.wdStyleHeading2);

                if (reader.HasRows)
                    QueryToWordTable(reader,true);
                else
                    wordHelper.WriteLine("No Specific Database Access Rights", WdBuiltinStyle.wdStyleNormal);

                if (!reader.IsClosed)
                    reader.Close();

                wordHelper.GoToEndOfDocument();

            }

            con.Close();

            wrdApp.Visible = true;

        }

        private SqlCommand getDatabaseAccessStatus(string name, SqlConnection connection)
        {
            //get the current users
            string sql = @"
SELECT distinct
[DatabaseName]
          ,[validFrom]
		  ,'current' as ExpiryDate 
  FROM {0}.[dbo].[DatabasePermissions]
  WHERE
  (UserName IS NULL OR (UserName NOT LIKE '%##%' AND UserName NOT IN ('NT AUTHORITY\SYSTEM', 'NT SERVICE\MSSQLSERVER', 'NT SERVICE\SQLSERVERAGENT')))
and DatabaseName NOT IN ('msdb', 'ReportServer', 'ReportServerTempDB')
AND (ObjectName IS NULL OR ObjectName NOT IN ('fn_diagramobjects', 'sp_alterdiagram', 'sp_creatediagram', 'sp_dropdiagram', 'sp_helpdiagramdefinition', 'sp_helpdiagrams', 'sp_renamediagram'))
AND ObjectName IS NULL
and (UserName = @name or [DatabaseUserName] = @name)
and (PermissionState is null OR PermissionState <> 'DENY')
";

            //and the expired users
            if (!_currentUsersOnly)
                sql += @"UNION 
SELECT distinct
[DatabaseName]
          ,[validFrom]
		  ,CONVERT(varchar(50),validTo) as ExpiryDate 
  FROM {0}.[dbo].[DatabasePermissions_Archive]
  WHERE
  (UserName IS NULL OR (UserName NOT LIKE '%##%' AND UserName NOT IN ('NT AUTHORITY\SYSTEM', 'NT SERVICE\MSSQLSERVER', 'NT SERVICE\SQLSERVERAGENT')))
and DatabaseName NOT IN ('msdb', 'ReportServer', 'ReportServerTempDB')
AND (ObjectName IS NULL OR ObjectName NOT IN ('fn_diagramobjects', 'sp_alterdiagram', 'sp_creatediagram', 'sp_dropdiagram', 'sp_helpdiagramdefinition', 'sp_helpdiagrams', 'sp_renamediagram'))
AND ObjectName IS NULL
and (UserName = @name or [DatabaseUserName] = @name)
and (PermissionState is null OR PermissionState <> 'DENY')
and DatabaseName not in 
( 
SELECT DatabaseName FROM {0}.[dbo].[DatabasePermissions]
  WHERE
  (UserName IS NULL OR (UserName NOT LIKE '%##%' AND UserName NOT IN ('NT AUTHORITY\SYSTEM', 'NT SERVICE\MSSQLSERVER', 'NT SERVICE\SQLSERVERAGENT')))
and DatabaseName NOT IN ('msdb', 'ReportServer', 'ReportServerTempDB')
AND (ObjectName IS NULL OR ObjectName NOT IN ('fn_diagramobjects', 'sp_alterdiagram', 'sp_creatediagram', 'sp_dropdiagram', 'sp_helpdiagramdefinition', 'sp_helpdiagrams', 'sp_renamediagram'))
AND ObjectName IS NULL
and (UserName = @name or [DatabaseUserName] = @name)
and (PermissionState is null OR PermissionState <> 'DENY'))";

            SqlCommand cmd = new SqlCommand(string.Format(sql, _dbInfo.GetRuntimeName()), connection);
            cmd.Parameters.Add("@name", SqlDbType.VarChar);
            cmd.Parameters["@name"].Value = name;
            return cmd;
        }

        private SqlCommand getAdministratorStatus(string name,SqlConnection connection)
        {
            //get the current users
            string sql = @"
select name,sysadmin,securityadmin,serveradmin,setupadmin,processadmin,diskadmin,dbcreator,bulkadmin,validFrom, 'current' as ExpiryDate from {0}.[dbo].[Logins] 
where name = @name
and
hasaccess = 1
and 
(sysadmin+securityadmin+serveradmin+setupadmin+processadmin+diskadmin+dbcreator+bulkadmin>0) -- admin status people only otherwise return no rows.
";

            //UNION the expired users
            if (!_currentUsersOnly)
                sql += @"union
select
 name,sysadmin,securityadmin,serveradmin,setupadmin,processadmin,diskadmin,dbcreator,bulkadmin, validFrom,CONVERT(varchar(50),validTo) as ExpiryDate from {0}.[dbo].[Logins_Archive] 
where name = @name
and
hasaccess = 1
and 
(sysadmin+securityadmin+serveradmin+setupadmin+processadmin+diskadmin+dbcreator+bulkadmin>0)";


            SqlCommand cmd = new SqlCommand(string.Format(sql, _dbInfo.GetRuntimeName()), connection);
            cmd.Parameters.Add("@name", SqlDbType.VarChar);
            cmd.Parameters["@name"].Value = name;
            return cmd;
        }

        public void QueryToWordTable(SqlDataReader r, bool doubleUpColumns)
        {

            System.Data.DataTable dataTable = new System.Data.DataTable();
            dataTable.Load(r);

            object start = wrdDoc.Content.End - 1;
            object end = wrdDoc.Content.End - 1;

            Range tableLocation = wrdDoc.Range(ref start, ref end);

            Table wordTable;

            //doubling up columns is for long datasets with few columns e.g. 100 rows, 2 columns (e.g. name, age).  In this case we half the dataset and duplicate the columns so that the table looks like name,age,<empty column to break up space>,name,age
            //this allows 2 source rows to be fit into one Word table row and save space in the document.
            if (doubleUpColumns)
                wordTable = wrdDoc.Tables.Add(tableLocation, (int)((dataTable.Rows.Count/2.0f)+0.5f) + 1, dataTable.Columns.Count*2+1); //divide number of rows in table by 2 (as a float division), then add 0.5 to make any 1.5 into 2 and then make int (int cast will drop off any incorrectly added 0.5 e.g. 1+0.5 = 1 while 1.5+0.5= 2
            else
                wordTable = wrdDoc.Tables.Add(tableLocation, dataTable.Rows.Count + 1, dataTable.Columns.Count);


            wordTable.set_Style("Table Grid");
            wordTable.Range.Font.Size = 5;
            wordTable.AllowAutoFit = true;

            //make middle column invisible
            if(doubleUpColumns)
                for(int row = 0 ; row < wordTable.Rows.Count+1;row++)
                {
                    wordTable.Cell(row, dataTable.Columns.Count + 1).Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleNone;
                    wordTable.Cell(row, dataTable.Columns.Count + 1).Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleNone;
                }

            //do the table headers
            int tableLine = 1;
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                wordTable.Cell(tableLine, i + 1).Range.Text = dataTable.Columns[i].ColumnName;
                
                if(doubleUpColumns)
                    wordTable.Cell(tableLine, i + 2  + dataTable.Columns.Count).Range.Text = dataTable.Columns[i].ColumnName;    
            }

            //oops we are doubling up but theres only one row of data!
            if (doubleUpColumns && dataTable.Rows.Count == 1)
            {
                for (int row = 0; row < wordTable.Rows.Count + 1; row++)
                {
                    for (int col = dataTable.Columns.Count + 2; col < wordTable.Columns.Count + 1; col++)
                    {
                        //hide the cells
                        wordTable.Cell(row, col).Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleNone;
                        wordTable.Cell(row, col).Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleNone;
                        wordTable.Cell(row, col).Borders[WdBorderType.wdBorderLeft].LineStyle = WdLineStyle.wdLineStyleNone;
                        wordTable.Cell(row, col).Borders[WdBorderType.wdBorderRight].LineStyle = WdLineStyle.wdLineStyleNone;
                        wordTable.Cell(row, col).Range.Text = ""; //clear the text
                    }
                }
            }

            //do the table contents
            tableLine++;

            if(!doubleUpColumns)
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                        wordTable.Cell(tableLine, i + 1).Range.Text = Convert.ToString(row[i]); //output each cell

                    tableLine++;
                }
            else
            {
                //output left half
                int row = 0;
                while(row  < dataTable.Rows.Count)
                {
                   for (int i = 0; i < dataTable.Columns.Count; i++)
                      wordTable.Cell(tableLine, i + 1).Range.Text = Convert.ToString(dataTable.Rows[row][i]); // output left half of table

                    row++;//advance to next virtual row

                    if (row == dataTable.Rows.Count) //jump out if theres an odd number of rows and we just fell off end of array
                        break;

                    for (int i = 0; i < dataTable.Columns.Count; i++)
                        wordTable.Cell(tableLine, i + 2 + dataTable.Columns.Count).Range.Text = Convert.ToString(dataTable.Rows[row][i]); //output right half of table

                    row++;//advance to next virtual row

                    tableLine++; //take new line in actual word table
                }
            }


            r.Close();

        }

        
        private IEnumerable<string> UserAccounts()
        {
            SqlConnection c = (SqlConnection)_dbInfo.Server.GetConnection();

            c.Open();

            //get the current users with login rights or database permissions
            string sql =
                @"select distinct name from {0}.[dbo].[Logins] where name not like '%##%' and name not like '%NT %' and name not in ('sys','{{All Users}}','dbo','sa','guest','SQLAgentOperatorRole','SQLAgentReaderRole')
union
select distinct DatabaseUserName as name from {0}.[dbo].[DatabasePermissions] where DatabaseUserName not like '%##%' and DatabaseUserName not like '%NT %'  and DatabaseUserName not in ('sys','{{All Users}}','dbo','sa','guest','SQLAgentOperatorRole','SQLAgentReaderRole') 
";
            //and get the expired user accounts / expired permissions too
            if(!_currentUsersOnly)
                sql += @"union
select distinct name from {0}.[dbo].[Logins_Archive] where name not like '%##%' and name not like '%NT %'  and name not in ('sys','{{All Users}}','dbo','sa','guest','SQLAgentOperatorRole','SQLAgentReaderRole') 
union
select distinct DatabaseUserName as name  from {0}.[dbo].[DatabasePermissions_Archive] where DatabaseUserName not like '%##%'  and DatabaseUserName not like '%NT %' and DatabaseUserName not in ('sys','{{All Users}}','dbo','sa','guest','SQLAgentOperatorRole','SQLAgentReaderRole') ";

            SqlCommand cmd = new SqlCommand(
                string.Format(sql,_dbInfo.GetRuntimeName()), c);
            SqlDataReader r = cmd.ExecuteReader();

            while (r.Read())
                yield return r["name"] as string;
        }
    }
}
