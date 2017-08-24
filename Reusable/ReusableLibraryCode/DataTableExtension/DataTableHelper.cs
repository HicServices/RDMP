using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace ReusableLibraryCode.DataTableExtension
{

    /// <summary>
    /// This class knows about how to move C# DataTables around, it includes the ability to push the DataTable to a database or tempdb
    /// 
    /// It also knows how to inspect it's own data which might be nothing but Strings and come up with sensible data types.  As part of this
    /// you can pass a dictionary of column name - explicit datatype to overwrite the default behaviour and force a specific datatype (a use
    /// case for this is if you have a column which is numbers but you want to record it as a varchar(x)).
    /// </summary>
    public class DataTableHelper
    {
        public DataTable DataTable { get; protected set; }
        
        protected int _padCharacterFieldsOutToMinimumLength;
        protected Dictionary<DataColumn, DataTypeComputer> typeDictionary;

        /// <summary>
        /// Dictionary of columns expected to be in DataTable and the datatype you want when writing them to a server e.g. you have a column called CHI which is System.Int but you force it to be varchar(10) in your database.
        /// </summary>
        public Dictionary<string, string> ExplicitWriteTypes = new Dictionary<string, string>();

        public DataTableHelper()
        {
            
        }

        public DataTableHelper(DataTable dt)
        {
            DataTable = dt;
        }


        /// <summary>
        /// This class wraps a DataTable and monitors the contents of columns in order to make estimations about what datatypes it should use when/if it ultimately uploads
        /// the contents of the DataTable onto the server.  This method updates the datatype computers based on the contents of the columns
        /// </summary>
        /// <param name="alsoChangeDataTypesOfColumns">true if you want to take the output of the datatype computers and use it to edit the DataTypes of the columns in the DataTable to the new types - helps for when you create a DataTable with a bunch of string columns but load strongly typed data into it</param>
        /// <param name="ExplicitReadTypes">This lets you override the determinations of the DataTypeComputers by setting the DataTable to freaky types e.g. load 102 as a string</param>
        protected void AdjustDataTableTypes(bool alsoChangeDataTypesOfColumns, Dictionary<string, Type> ExplicitReadTypes)
        {
            typeDictionary = new Dictionary<DataColumn, DataTypeComputer>();

            foreach (DataColumn col in DataTable.Columns)
            {
                DataTypeComputer dc = new DataTypeComputer(_padCharacterFieldsOutToMinimumLength);
                typeDictionary.Add(col, dc);

                //if we have an explicit read type
                if (ExplicitReadTypes != null && ExplicitReadTypes.ContainsKey(col.ColumnName))
                    dc.CurrentEstimate = ExplicitReadTypes[col.ColumnName];//override what it thinks it is to what we want it to be
                else
                    foreach (DataRow row in DataTable.Rows)
                        dc.AdjustToCompensateForValue(row[col]);
            }

            if (alsoChangeDataTypesOfColumns)
            {
                DataTable dtCloned = DataTable.Clone();

                //adjust types
                foreach (DataColumn dataColumn in dtCloned.Columns.Cast<DataColumn>().ToArray())
                {
                    var clonedFromColumn = DataTable.Columns.Cast<DataColumn>().Single(c=>c.ColumnName.Equals(dataColumn.ColumnName));
                    
                    //set the datatype to the one the computer thinks it is (based on the above evaluation of it's contents)
                    dataColumn.DataType = typeDictionary[clonedFromColumn].CurrentEstimate;
                }

                foreach (DataRow row in DataTable.Rows)
                    dtCloned.ImportRow(row);

                //now use the new strongly typed one
                DataTable = dtCloned;
            }
        }


        virtual public string GetTableName()
        {
            if(DataTable == null)
                throw new Exception("DataTable not initialized yet");

            if(string.IsNullOrWhiteSpace(DataTable.TableName))
                throw new NullReferenceException("DataTable must have a name - so it can be inserted into temp db etc");

            return DataTable.TableName;
        }



        /// <summary>
        /// Commits the DataTable loaded from the CSV into TempDB in the cohort server, returns the name of the temp table.  This operates in a transaction meaning 
        /// that if values are rejected by the database after the CREATE has happened the table is properly rollbacked to not existing.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="useOldDateTimes"></param>
        /// <returns>The name of the table created in TempDB</returns>
        public string CommitDataTableToTempDB(DiscoveredServer server, bool useOldDateTimes)
        {
            using (var con = (SqlConnection)server.GetConnection())
            {
                con.Open();
                con.ChangeDatabase("tempdb");
                
                SqlTransaction transaction = con.BeginTransaction();
             
                string toReturn = CommitDataTableToTempDB(server, con, useOldDateTimes, transaction);
                transaction.Commit();
                return toReturn;
            }
        }
        public string CommitDataTableToTempDB(DiscoveredServer server,SqlConnection con, bool useOldDateTimes, SqlTransaction transaction = null)
        {
            //we cannot submit it to a server until it has a name but the user wants to submit it to tempdb so it likely won't be hanging around forever so lets give it a nice GUID name
            if(string.IsNullOrWhiteSpace(DataTable.TableName))
                DataTable.TableName = "TT" + Guid.NewGuid().ToString().Replace("-", "");
            
            con.ChangeDatabase("tempdb");

            string tableName;
            UploadFileToConnection(server,con, out tableName, transaction, true, useOldDateTimes);
            return tableName;
        }

        public void CreateTables(DiscoveredServer server, DbConnection con, out string tableName, DbTransaction transaction,
            bool dropIfAlreadyExists, bool useOldDateTimes)
        {
            string whoCares;
            CreateTables(server,con,out tableName,transaction,dropIfAlreadyExists,useOldDateTimes,out whoCares);
        }

        public void CreateTables(DiscoveredServer server, DbConnection con, out string tableName, DbTransaction transaction, bool dropIfAlreadyExists, bool useOldDateTimes, out string sql)
        {
            tableName = GetTableName();
            
            sql = GetCreateTableSql(server,tableName, dropIfAlreadyExists, server.GetTableHelper());

            if (useOldDateTimes && server.DatabaseType == DatabaseType.MicrosoftSQLServer)
                sql = sql.Replace("datetime2", "datetime");

            try
            {
                DbCommand cmdCreate = server.GetCommand(sql, con);
                cmdCreate.Transaction = transaction;
                cmdCreate.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to execute create table SQL:" + Environment.NewLine + sql,e);
            }
        }

        public int UploadFileToConnection(DiscoveredServer server, DbConnection con, out string tableName, DbTransaction transaction, bool dropIfAlreadyExists, bool useOldDateTimes)
        {
            CreateTables(server,con, out tableName, transaction, dropIfAlreadyExists, useOldDateTimes);

            SqlBulkCopy bulkcopy = new SqlBulkCopy((SqlConnection) con, SqlBulkCopyOptions.KeepIdentity, (SqlTransaction) transaction);
            bulkcopy.BulkCopyTimeout = 50000;
            bulkcopy.DestinationTableName = tableName;

            return UsefulStuff.BulkInsertWithBetterErrorMessages(bulkcopy, DataTable, serverForLineByLineInvestigation: transaction != null?null:server);//if there is a transaction do not let the method replay it because it probably won't work the second time around either and error will be more cryptic than ever

        }

        public string GetCreateTableSql(DiscoveredServer server,bool dropIfExists, IDiscoveredTableHelper tableHelper)
        {
            return GetCreateTableSql(server,GetTableName(), dropIfExists, tableHelper);
        }

        protected string GetCreateTableSql(DiscoveredServer server, string tableName, bool dropIfAlreadyExists, IDiscoveredTableHelper tableHelper)
        {

            if (typeDictionary == null)
                AdjustDataTableTypes(false,null);

            string headerSql = "";
            string bodySql = "";

             //if table exists
            if (dropIfAlreadyExists)
                headerSql = tableHelper.WrapStatementWithIfTableExistanceMatches(true,new StringLiteralSqlInContext("DROP TABLE " + tableName,false), tableName);

            bodySql += "CREATE TABLE " + tableName + "(" + Environment.NewLine;
            
            foreach (DataColumn col in DataTable.Columns)
            {
                string datatype = null;

                //if there is an explicit override setting for the datatype
                if (ExplicitWriteTypes != null && ExplicitWriteTypes.ContainsKey(col.ColumnName))
                    datatype = ExplicitWriteTypes[col.ColumnName];
                else
                    datatype = typeDictionary[col].GetSqlDBType(server);
                
                //add the column name and accompanying datatype
                bodySql += SqlSyntaxHelper.GetRuntimeName(col.ColumnName) + " " + datatype + "," + Environment.NewLine;
            }
            bodySql = bodySql.TrimEnd('\r', '\n', ',');

            bodySql += ")" + Environment.NewLine;

            //if table not exists 
            bodySql = tableHelper.WrapStatementWithIfTableExistanceMatches(false,new StringLiteralSqlInContext(bodySql,false),tableName);

            return headerSql + bodySql;
        }


        public void SetDataTable(DataTable resultTable, bool adjustDataTypes, Dictionary<string,Type> explicityTypes)
        {
            DataTable = resultTable;
            
            if (adjustDataTypes)
                AdjustDataTableTypes(true, explicityTypes);
        }

        public Dictionary<DataColumn, DataTypeComputer> GetTypeDictionary()
        {
            return typeDictionary;
        }
    }
}