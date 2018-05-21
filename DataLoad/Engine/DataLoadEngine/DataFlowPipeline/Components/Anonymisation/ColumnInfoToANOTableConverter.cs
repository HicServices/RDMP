using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Triggers;
using CatalogueLibrary.Triggers.Implementations;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.DataFlowPipeline.Components.Anonymisation
{
    /// <summary>
    /// Engine class for converting a ColumnInfo and all the data in it into ANO equivalents (See ColumnInfoToANOTableConverterUI).
    /// </summary>
    public class ColumnInfoToANOTableConverter
    {
        private readonly ColumnInfo _colToNuke;
        private readonly ANOTable _toConformTo;
        private readonly TableInfo _tableInfo;
        private ColumnInfo _newANOColumnInfo;
        private readonly DataAccessPortal _dataAccessPortal;

        public ColumnInfoToANOTableConverter(ColumnInfo colToNuke, ANOTable toConformTo, DataAccessPortal dataAccessPortal)
        {
            _tableInfo = colToNuke.TableInfo;
            _colToNuke = colToNuke;
            _toConformTo = toConformTo;
            _dataAccessPortal = dataAccessPortal;
        }

        public bool ConvertEmptyColumnInfo(Func<string, bool> shouldApplySql, ICheckNotifier notifier)
        {
            int rowcount = _dataAccessPortal
                .ExpectDatabase(_tableInfo, DataAccessContext.DataLoad)
                .ExpectTable(_tableInfo.GetRuntimeName(LoadStage.PostLoad))
                .GetRowCount();
            
            if(rowcount>0)
                throw new NotSupportedException("Table " + _tableInfo + " contains " + rowcount + " rows of data, you cannot use ColumnInfoToANOTableConverter.ConvertEmptyColumnInfo on this table");

            using (var con = _dataAccessPortal.ExpectServer(_tableInfo, DataAccessContext.DataLoad).GetConnection())
            {
                con.Open();
                
                if (!IsOldColumnDroppable(con, notifier))
                    return false;

                EnsureNoTriggerOnTable();

                AddNewANOColumnInfo(shouldApplySql, con, notifier);

                DropOldColumn(shouldApplySql, con,null);


                //synchronize again
                new TableInfoSynchronizer(_tableInfo).Synchronize(notifier);
            }

            return true;
        }
        public bool ConvertFullColumnInfo(Func<string, bool> shouldApplySql, ICheckNotifier notifier)
        {
            using (var con = _dataAccessPortal.ExpectServer(_tableInfo, DataAccessContext.DataLoad).GetConnection())
            {
                con.Open();

                if (!IsOldColumnDroppable(con, notifier))
                    return false;

                EnsureNoTriggerOnTable();
                
                AddNewANOColumnInfo(shouldApplySql, con, notifier);

                MigrateExistingData(shouldApplySql,con, notifier);

                DropOldColumn(shouldApplySql, con,null);

                //synchronize again
                new TableInfoSynchronizer(_tableInfo).Synchronize(notifier);
            }

            return true;
        }

        private void EnsureNoTriggerOnTable()
        {
            var database = _dataAccessPortal.ExpectDatabase(_tableInfo, DataAccessContext.DataLoad);

            var triggerFactory = new TriggerImplementerFactory(database.Server.DatabaseType);
            

            var triggerImplementer = triggerFactory.Create(database.ExpectTable(_tableInfo.GetRuntimeName()));

            if (triggerImplementer.GetTriggerStatus() != TriggerStatus.Missing)
                throw new NotSupportedException("Table " + _tableInfo + " has a backup trigger on it, this will destroy performance and break when we add the ANOColumn, dropping the trigger is not an option because of the _Archive table still containing identifiable data (and other reasons)");
        }

        private void MigrateExistingData(Func<string, bool> shouldApplySql, DbConnection con, ICheckNotifier notifier)
        {
            string from = _colToNuke.GetRuntimeName(LoadStage.PostLoad);
            string to = _newANOColumnInfo.GetRuntimeName(LoadStage.PostLoad);
            string tableName = _tableInfo.GetRuntimeName();

            //create an empty table for the anonymised data
            DbCommand cmdCreateTempMap = DatabaseCommandHelper.GetCommand(string.Format("SELECT top 0 {0},{1} into TempANOMap from {2}", from, to, tableName),con);

            if(!shouldApplySql(cmdCreateTempMap.CommandText))
                throw new Exception("User decided not to create the TempANOMap table");

            cmdCreateTempMap.ExecuteNonQuery();
            try
            {
                //get the existing data
                DbCommand cmdGetExistingData = DatabaseCommandHelper.GetCommand(string.Format("SELECT {0},{1} from {2}",from,to,tableName),con);
            
                DbDataAdapter da = DatabaseCommandHelper.GetDataAdapter(cmdGetExistingData);

                DataTable dt = new DataTable();
                da.Fill(dt);//into memory

                //transform it in memory
                ANOTransformer transformer = new ANOTransformer(_toConformTo, new FromCheckNotifierToDataLoadEventListener(notifier));
                transformer.Transform(dt,dt.Columns[0],dt.Columns[1]);

                //move it into the TempANOMap
                SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection) con);
                bulkCopy.DestinationTableName = "TempANOMap";
                bulkCopy.ColumnMappings.Add(dt.Columns[0].ColumnName, dt.Columns[0].ColumnName);
                bulkCopy.ColumnMappings.Add(dt.Columns[1].ColumnName, dt.Columns[1].ColumnName);

                UsefulStuff.BulkInsertWithBetterErrorMessages(bulkCopy, dt,DataAccessPortal.GetInstance().ExpectServer(_tableInfo, DataAccessContext.DataLoad));


                //create an empty table for the anonymised data
                DbCommand cmdUpdateMainTable = DatabaseCommandHelper.GetCommand(string.Format("UPDATE source set source.{1} = map.{1} from {2} source join TempANOMap map on source.{0}=map.{0}", from, to, tableName), con);

                if (!shouldApplySql(cmdUpdateMainTable.CommandText))
                    throw new Exception("User decided not to perform update on table");
                cmdUpdateMainTable.ExecuteNonQuery();

            }
            finally
            {
                //always drop the temp anomap
                DbCommand dropMappingTable = DatabaseCommandHelper.GetCommand("DROP TABLE TempANOMap", con);
                dropMappingTable.ExecuteNonQuery();
            }
        }



        private void DropOldColumn(Func<string, bool> shouldApplySql, DbConnection con, DbTransaction transaction)
        {
            string alterSql = "ALTER TABLE " + _tableInfo.Name + " Drop column " + _colToNuke.GetRuntimeName(LoadStage.PostLoad);

            if (shouldApplySql(alterSql))
            {
                var cmd = DatabaseCommandHelper.GetCommand(alterSql, con);
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();
            }
            else
            {
                throw new Exception("User chose not to drop the old column " + _colToNuke);
            }
        }


        private bool IsOldColumnDroppable(DbConnection con, ICheckNotifier notifier)
        {
            try
            {
                DbTransaction transaction = con.BeginTransaction();

                //try dropping it within a transaction
                DropOldColumn((s) => true, con, transaction);

                //it is droppable - rollback that drop!
                transaction.Rollback();
                transaction.Dispose();

                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Could not perform transformation because column " + _colToNuke + " is not droppable",
                            CheckResult.Fail, e));
                return false;
            }
        }

        private void AddNewANOColumnInfo(Func<string, bool> shouldApplySql, DbConnection con, ICheckNotifier notifier)
        {
            string anoColumnNameWillBe = "ANO" + _colToNuke.GetRuntimeName(LoadStage.PostLoad);

            string alterSql = "ALTER TABLE " + _tableInfo.Name + " ADD " + anoColumnNameWillBe + " " + _toConformTo.GetRuntimeDataType(LoadStage.PostLoad);

            if (shouldApplySql(alterSql))
            {
                var cmd = DatabaseCommandHelper.GetCommand(alterSql, con);
                cmd.ExecuteNonQuery();

                TableInfoSynchronizer synchronizer = new TableInfoSynchronizer(_tableInfo);
                synchronizer.Synchronize(notifier);

                //now get the new ANO columninfo
                _newANOColumnInfo = _tableInfo.ColumnInfos.Single(c => c.GetRuntimeName().Equals(anoColumnNameWillBe));
                _newANOColumnInfo.ANOTable_ID = _toConformTo.ID;
                _newANOColumnInfo.SaveToDatabase();
            }
            else
                throw new Exception("User chose not to apply part of the operation");


        }
    }

}