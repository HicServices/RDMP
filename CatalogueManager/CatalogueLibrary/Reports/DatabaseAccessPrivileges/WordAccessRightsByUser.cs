using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Xceed.Words.NET;

namespace CatalogueLibrary.Reports.DatabaseAccessPrivileges
{
    public class WordAccessRightsByUser:RequiresMicrosoftOffice
    {
        public string Server { get; set; }
        
        private DiscoveredDatabase _dbInfo;
        private readonly bool _currentUsersOnly;

        public WordAccessRightsByUser(DiscoveredDatabase dbInfo, bool currentUsersOnly)
        {
            _dbInfo = dbInfo;
            _currentUsersOnly = currentUsersOnly;
        }

        public void GenerateWordFile()
        {
            var f = GetUniqueFilenameInWorkArea("DatabaseAccessRightsByUser");

            using (DocX document = DocX.Create(f.FullName))
            {
                InsertHeader(document,"Database Access Report:" + Server);

                SqlConnection con = (SqlConnection)_dbInfo.Server.GetConnection();
                con.Open();

                foreach (string user in UserAccounts())
                {

                    InsertHeader(document,user);

                    SqlCommand cmd = getAdministratorStatus(user, con);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        InsertHeader(document, "Administrator Status",2);
                        QueryToWordTable(document,reader, false);
                    }

                    if (!reader.IsClosed)
                        reader.Close();

                    cmd = getDatabaseAccessStatus(user, con);

                    reader = cmd.ExecuteReader();

                    InsertHeader(document,"Specific Database Access Rights",2);

                    if (reader.HasRows)
                        QueryToWordTable(document,reader, true);
                    else
                        InsertParagraph(document,"No Specific Database Access Rights");

                    if (!reader.IsClosed)
                        reader.Close();
                }

                con.Close();

                document.Save();
                ShowFile(f);
            }
            

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

        public void QueryToWordTable(DocX document, SqlDataReader r, bool doubleUpColumns)
        {

            System.Data.DataTable dataTable = new DataTable();
            dataTable.Load(r);

            

            Table wordTable;

            //doubling up columns is for long datasets with few columns e.g. 100 rows, 2 columns (e.g. name, age).  In this case we half the dataset and duplicate the columns so that the table looks like name,age,<empty column to break up space>,name,age
            //this allows 2 source rows to be fit into one Word table row and save space in the document.
            if (doubleUpColumns)
                wordTable = InsertTable(document,(int)((dataTable.Rows.Count/2.0f)+0.5f) + 1, dataTable.Columns.Count*2+1); //divide number of rows in table by 2 (as a float division), then add 0.5 to make any 1.5 into 2 and then make int (int cast will drop off any incorrectly added 0.5 e.g. 1+0.5 = 1 while 1.5+0.5= 2
            else
                wordTable = InsertTable(document, dataTable.Rows.Count + 1, dataTable.Columns.Count);

            var fontSize = 5;
            
            //make middle column invisible
            /*if(doubleUpColumns)
                for(int row = 0 ; row < wordTable.Rows.Count+1;row++)
                {
                    wordTable.Cell(row, dataTable.Columns.Count + 1).Borders[WdBorderType.wdBorderTop].LineStyle = WdLineStyle.wdLineStyleNone;
                    wordTable.Cell(row, dataTable.Columns.Count + 1).Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleNone;
                }*/

            //do the table headers
            int tableLine = 0;
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                SetTableCell(wordTable, tableLine, i, dataTable.Columns[i].ColumnName, fontSize);

                if (doubleUpColumns)
                    SetTableCell(wordTable, tableLine, i + 1 + dataTable.Columns.Count, dataTable.Columns[i].ColumnName, fontSize);
            }

            /*
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
            */

            //do the table contents
            tableLine++;

            if(!doubleUpColumns)
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                        SetTableCell(wordTable,tableLine, i ,Convert.ToString(row[i]),fontSize); //output each cell

                    tableLine++;
                }
            else
            {
                //output left half
                int row = 0;
                while(row  < dataTable.Rows.Count)
                {
                   for (int i = 0; i < dataTable.Columns.Count; i++)
                       SetTableCell(wordTable,tableLine, i , Convert.ToString(dataTable.Rows[row][i]),fontSize); // output left half of table

                    row++;//advance to next virtual row

                    if (row == dataTable.Rows.Count) //jump out if theres an odd number of rows and we just fell off end of array
                        break;

                    for (int i = 0; i < dataTable.Columns.Count; i++)
                        SetTableCell(wordTable, tableLine, i + 1 + dataTable.Columns.Count, Convert.ToString(dataTable.Rows[row][i]), fontSize); //output right half of table

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
