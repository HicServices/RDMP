using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.DatabaseManagement;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.DataFlowPipeline.Components.Anonymisation
{
    public class ANOTransformer
    {
        private readonly ANOTable _anoTable;
        
        private readonly SqlConnection _con;
        private readonly IDataLoadEventListener _listener;

        //the following stored procedures have to exist in the target database;
        private ExternalDatabaseServer _externalDatabaseServer;
        private DiscoveredServer _server;

        private const string SubstitutionStoredprocedure = "sp_substituteANOIdentifiers";

        public ANOTransformer(ANOTable anoTable, IDataLoadEventListener listener= null)
        {
            _externalDatabaseServer = anoTable.Server;

            _server = DataAccessPortal.GetInstance().ExpectServer(_externalDatabaseServer, DataAccessContext.DataLoad);
            _con = (SqlConnection)_server.GetConnection();
            _con.InfoMessage+=_con_InfoMessage;
            
            _anoTable = anoTable;
            _listener = listener;
        }

        public void Transform(DataTable table, DataColumn srcColumn, DataColumn destColumn, bool previewOnly = false)
        {
            DataTable tableOfIdentifiersRequiringSubstitution = ColumnToDataTable(srcColumn,true);

            if (!destColumn.ColumnName.Equals(ANOTable.ANOPrefix + srcColumn.ColumnName))
               throw new Exception("Expected destination column " + destColumn.ColumnName + " to be " + ANOTable.ANOPrefix + srcColumn.ColumnName );

            if(tableOfIdentifiersRequiringSubstitution.Columns.Count != 1)
                throw new Exception("Expected only a single columns to be dispatched to SubstituteIdentifiersForANOEquivalents");

            //if there is no data to transform, don't bother
            if (table.Rows.Count == 0) 
                return;

            //translate all values into ANO equivalents
            DataTable substitutionTable = GetSubstitutionsForANOEquivalents(tableOfIdentifiersRequiringSubstitution, previewOnly);
          
            //give the substitution table a primary key to make it faster.
            substitutionTable.PrimaryKey = new[] {substitutionTable.Columns[0]};

            if(substitutionTable.Columns.Count != 2)
                throw new Exception("Expected only a two columns to be returned by SubstituteIdentifiersForANOEquivalents, the original primary key and the substitution identifier");
            
            if (substitutionTable.Columns[0].ColumnName.StartsWith(ANOTable.ANOPrefix))
                throw new Exception("Expected first column returned by SubstituteIdentifiersForANOEquivalents to be a primary key (not start with " + ANOTable.ANOPrefix + ") but it was:" + substitutionTable.Columns[0].ColumnName);
            
            if (!substitutionTable.Columns[1].ColumnName.StartsWith(ANOTable.ANOPrefix))
                throw new Exception("Expected second column returned by SubstituteIdentifiersForANOEquivalents to be an ANO identifier(start with " + ANOTable.ANOPrefix + ") but it was:" + substitutionTable.Columns[1].ColumnName);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                // fill with ANO versions (found by primary key)
                object valueToReplace = table.Rows[i][srcColumn.ColumnName];

                //dont bother substituting nulls (because they wont have a sub!)
                if (valueToReplace == DBNull.Value)
                    continue;
                
                //its not null so look up the mapped value
                DataRow substitutionRow = substitutionTable.Rows.Find(valueToReplace);

                if (substitutionRow == null)
                    throw new Exception("Substitution table returned by " + SubstitutionStoredprocedure + " did not contain a mapping for identifier " + valueToReplace + "(Substitution Table had " + substitutionTable.Rows.Count + " rows)");

                object substitutionValue = substitutionRow[1];//substitution value
                
                //overwrite the value with the substitution
                destColumn.Table.Rows[i][destColumn.ColumnName] = substitutionValue;
            }
        }

        private DataTable ColumnToDataTable(DataColumn column,bool discardNulls)
        {
            DataTable table = new DataTable();

            table.Columns.Add(column.ColumnName, column.DataType);

            foreach (DataRow r in column.Table.Rows)
            {
                var o = r[column.ColumnName];

                //if we are discarding nulls we choose to not add them to the return table
                if(discardNulls && (o == null || o == DBNull.Value))
                    continue;

                table.Rows.Add(new[] { r[column.ColumnName] });
            }

            return table;
        }

        private DataTable GetSubstitutionsForANOEquivalents(DataTable table, bool previewOnly)
        {
            try
            {
                if (table.Rows.Count == 0)
                    return table;

                _con.InfoMessage += _con_InfoMessage;

                SqlTransaction transaction = null;
                
                if (previewOnly)
                {
                    bool mustPush = !_anoTable.IsTablePushed();

                    _con.Open();
                    transaction = _con.BeginTransaction();//if it is preview only we will use a transaction which we will then rollback

                    if (mustPush)
                        _anoTable.PushToANOServerAsNewTable(
                            UsefulStuff.GetSQLDBTypeForCSharpType(table.Columns[0].DataType,
                            _anoTable.NumberOfIntegersToUseInAnonymousRepresentation 
                            + _anoTable.NumberOfCharactersToUseInAnonymousRepresentation),
                            new ThrowImmediatelyCheckNotifier(), _con, transaction);
                }

                string substituteForANOIdentifiersProc = SubstitutionStoredprocedure;
                
                SqlCommand cmdSubstituteIdentifiers = new SqlCommand(substituteForANOIdentifiersProc, _con);
                cmdSubstituteIdentifiers.CommandType = CommandType.StoredProcedure;
                cmdSubstituteIdentifiers.CommandTimeout = 500;
                cmdSubstituteIdentifiers.Transaction = transaction;

                cmdSubstituteIdentifiers.Parameters.Add("@batch", SqlDbType.Structured);
                cmdSubstituteIdentifiers.Parameters.Add("@tableName", SqlDbType.VarChar, 500);
                cmdSubstituteIdentifiers.Parameters.Add("@numberOfIntegersToUseInAnonymousRepresentation", SqlDbType.Int);
                cmdSubstituteIdentifiers.Parameters.Add("@numberOfCharactersToUseInAnonymousRepresentation", SqlDbType.Int);
                cmdSubstituteIdentifiers.Parameters.Add("@suffix", SqlDbType.VarChar,10);

                //table valued parameter
                cmdSubstituteIdentifiers.Parameters["@batch"].TypeName = "dbo.Batch";
                cmdSubstituteIdentifiers.Parameters["@batch"].Value = table;

                cmdSubstituteIdentifiers.Parameters["@tableName"].Value = _anoTable.TableName;
                cmdSubstituteIdentifiers.Parameters["@numberOfIntegersToUseInAnonymousRepresentation"].Value = _anoTable.NumberOfIntegersToUseInAnonymousRepresentation;
                cmdSubstituteIdentifiers.Parameters["@numberOfCharactersToUseInAnonymousRepresentation"].Value = _anoTable.NumberOfCharactersToUseInAnonymousRepresentation;
                cmdSubstituteIdentifiers.Parameters["@suffix"].Value = _anoTable.Suffix;

                SqlDataAdapter da = new SqlDataAdapter(cmdSubstituteIdentifiers);
                DataTable dtToReturn = new DataTable();
                
                da.Fill(dtToReturn);

                if (previewOnly)
                    transaction.Rollback();
                

                return dtToReturn;
                                                     
            }
            catch (Exception e)
            {
                throw new Exception(SubstitutionStoredprocedure + " failed to complete correctly: " + e);
            }
            finally
            {

                _con.Close();
            }
        }

        //for some reason this method seems to get sent the same message twice every time
        string lastMessage;
        void _con_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(e.Message))
                return;
            
            if (e.Message.Equals(lastMessage))
                return;

            lastMessage = e.Message;

            if(_listener != null)
                _listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,e.Message));
            else
                Console.WriteLine(e.Message);
        }
        
        public string GetDestinationColumnExpectedDataType()
        {
            return _anoTable.GetRuntimeDataType(LoadStage.PostLoad);
        }

        
        public static void ConfirmDependencies(DiscoveredDatabase database,ICheckNotifier notifier)
        {
            try
            {

                if (database.DiscoverStoredprocedures().Any(p => p.Name.Equals(SubstitutionStoredprocedure)))
                    notifier.OnCheckPerformed(new CheckEventArgs("successfully found " + SubstitutionStoredprocedure + " on "+database, CheckResult.Success, null));
                else
                    notifier.OnCheckPerformed(new CheckEventArgs("Failed to find " + SubstitutionStoredprocedure + " on " + database, CheckResult.Fail, null));
            }   
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Exception occurred when trying to find stored procedure " + SubstitutionStoredprocedure +
                    " on " + database, CheckResult.Fail, e));
            }
        }
    }
}