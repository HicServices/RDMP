// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using TypeGuesser;

namespace Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;

/// <summary>
///     Substitutes identifiers in a DataTable for ANO mapped equivalents (for a single DataColumn/ANOTable only).  For
///     example storing all LabNumbers stored in
///     DataColumn LabNumber into the ANO Store database table and adding a new column to the DataTable called ANOLabNumber
///     and putting in the appropriate
///     replacement values.  All the heavy lifting (identifier allocation etc) is done by the stored procedure
///     SubstitutionStoredProcedure.
/// </summary>
public class ANOTransformer
{
    private readonly ANOTable _anoTable;

    private readonly IDataLoadEventListener _listener;

    //the following stored procedures have to exist in the target database;
    private readonly ExternalDatabaseServer _externalDatabaseServer;
    private readonly DiscoveredServer _server;

    private const string SubstitutionStoredProcedure = "sp_substituteANOIdentifiers";

    public ANOTransformer(ANOTable anoTable, IDataLoadEventListener listener = null)
    {
        _externalDatabaseServer = anoTable.Server;

        _server = DataAccessPortal.ExpectServer(_externalDatabaseServer, DataAccessContext.DataLoad);

        _anoTable = anoTable;
        _listener = listener;
    }

    public void Transform(DataTable table, DataColumn srcColumn, DataColumn destColumn, bool previewOnly = false)
    {
        var tableOfIdentifiersRequiringSubstitution = ColumnToDataTable(srcColumn, true);

        if (!destColumn.ColumnName.Equals(ANOTable.ANOPrefix + srcColumn.ColumnName))
            throw new Exception(
                $"Expected destination column {destColumn.ColumnName} to be {ANOTable.ANOPrefix}{srcColumn.ColumnName}");

        if (tableOfIdentifiersRequiringSubstitution.Columns.Count != 1)
            throw new Exception(
                "Expected only a single columns to be dispatched to SubstituteIdentifiersForANOEquivalents");

        //if there is no data to transform, don't bother
        if (table.Rows.Count == 0 || tableOfIdentifiersRequiringSubstitution.Rows.Count == 0)
            return;

        //translate all values into ANO equivalents
        var substitutionTable = GetSubstitutionsForANOEquivalents(tableOfIdentifiersRequiringSubstitution, previewOnly);

        //give the substitution table a primary key to make it faster.
        substitutionTable.PrimaryKey = new[] { substitutionTable.Columns[0] };

        if (substitutionTable.Columns.Count != 2)
            throw new Exception(
                "Expected only a two columns to be returned by SubstituteIdentifiersForANOEquivalents, the original primary key and the substitution identifier");

        if (substitutionTable.Columns[0].ColumnName.StartsWith(ANOTable.ANOPrefix))
            throw new Exception(
                $"Expected first column returned by SubstituteIdentifiersForANOEquivalents to be a primary key (not start with {ANOTable.ANOPrefix}) but it was:{substitutionTable.Columns[0].ColumnName}");

        if (!substitutionTable.Columns[1].ColumnName.StartsWith(ANOTable.ANOPrefix))
            throw new Exception(
                $"Expected second column returned by SubstituteIdentifiersForANOEquivalents to be an ANO identifier(start with {ANOTable.ANOPrefix}) but it was:{substitutionTable.Columns[1].ColumnName}");

        for (var i = 0; i < table.Rows.Count; i++)
        {
            // fill with ANO versions (found by primary key)
            var valueToReplace = table.Rows[i][srcColumn.ColumnName];

            //don't bother substituting nulls (because they won't have a sub!)
            if (valueToReplace == DBNull.Value)
                continue;

            //it's not null so look up the mapped value
            var substitutionRow = substitutionTable.Rows.Find(valueToReplace) ?? throw new Exception(
                $"Substitution table returned by {SubstitutionStoredProcedure} did not contain a mapping for identifier {valueToReplace}(Substitution Table had {substitutionTable.Rows.Count} rows)");
            var substitutionValue = substitutionRow[1]; //substitution value

            //overwrite the value with the substitution
            destColumn.Table.Rows[i][destColumn.ColumnName] = substitutionValue;
        }
    }

    private static DataTable ColumnToDataTable(DataColumn column, bool discardNulls)
    {
        var table = new DataTable();
        table.BeginLoadData();

        table.Columns.Add(column.ColumnName, column.DataType);

        foreach (DataRow r in column.Table.Rows)
        {
            var o = r[column.ColumnName];

            //if we are discarding nulls we choose to not add them to the return table
            if (discardNulls && (o == null || o == DBNull.Value))
                continue;

            table.Rows.Add(r[column.ColumnName]);
        }

        table.EndLoadData();
        return table;
    }

    private DataTable GetSubstitutionsForANOEquivalents(DataTable table, bool previewOnly)
    {
        using var con = (SqlConnection)_server.GetConnection();
        con.InfoMessage += _con_InfoMessage;

        if (table.Rows.Count == 0)
            return table;
        try
        {
            con.Open();
            using var
                transaction =
                    con.BeginTransaction(); //if it is preview only we will use a transaction which we will then rollback

            if (previewOnly)
            {
                var mustPush = !_anoTable.IsTablePushed();

                if (mustPush)
                {
                    var cSharpType =
                        new DatabaseTypeRequest(table.Columns[0].DataType,
                            _anoTable.NumberOfIntegersToUseInAnonymousRepresentation
                            + _anoTable.NumberOfCharactersToUseInAnonymousRepresentation);

                    //we want to use this syntax
                    var syntaxHelper = _server.Helper.GetQuerySyntaxHelper();

                    //push to the destination server
                    _anoTable.PushToANOServerAsNewTable(
                        //turn the csharp type into an SQL type e.g. string 30 becomes varchar(30)
                        syntaxHelper.TypeTranslater.GetSQLDBTypeForCSharpType(cSharpType),
                        ThrowImmediatelyCheckNotifier.Quiet, con, transaction);
                }
            }

            var substituteForANOIdentifiersProc = SubstitutionStoredProcedure;

            using var cmdSubstituteIdentifiers = new SqlCommand(substituteForANOIdentifiersProc, con)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 500,
                Transaction = transaction
            };

            cmdSubstituteIdentifiers.Parameters.Add("@batch", SqlDbType.Structured);
            cmdSubstituteIdentifiers.Parameters.Add("@tableName", SqlDbType.VarChar, 500);
            cmdSubstituteIdentifiers.Parameters.Add("@numberOfIntegersToUseInAnonymousRepresentation", SqlDbType.Int);
            cmdSubstituteIdentifiers.Parameters.Add("@numberOfCharactersToUseInAnonymousRepresentation", SqlDbType.Int);
            cmdSubstituteIdentifiers.Parameters.Add("@suffix", SqlDbType.VarChar, 10);

            //table valued parameter
            cmdSubstituteIdentifiers.Parameters["@batch"].TypeName = "dbo.Batch";
            cmdSubstituteIdentifiers.Parameters["@batch"].Value = table;

            cmdSubstituteIdentifiers.Parameters["@tableName"].Value = _anoTable.TableName;
            cmdSubstituteIdentifiers.Parameters["@numberOfIntegersToUseInAnonymousRepresentation"].Value =
                _anoTable.NumberOfIntegersToUseInAnonymousRepresentation;
            cmdSubstituteIdentifiers.Parameters["@numberOfCharactersToUseInAnonymousRepresentation"].Value =
                _anoTable.NumberOfCharactersToUseInAnonymousRepresentation;
            cmdSubstituteIdentifiers.Parameters["@suffix"].Value = _anoTable.Suffix;

            var da = new SqlDataAdapter(cmdSubstituteIdentifiers);
            var dtToReturn = new DataTable();
            dtToReturn.BeginLoadData();
            da.Fill(dtToReturn);
            dtToReturn.EndLoadData();

            if (previewOnly)
                transaction.Rollback();
            else
                transaction.Commit();


            return dtToReturn;
        }
        catch (Exception e)
        {
            throw new Exception($"{SubstitutionStoredProcedure} failed to complete correctly: {e}");
        }
    }

    //for some reason this method seems to get sent the same message twice every time
    private string lastMessage;

    private void _con_InfoMessage(object sender, SqlInfoMessageEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Message))
            return;

        if (e.Message.Equals(lastMessage))
            return;

        lastMessage = e.Message;

        if (_listener != null)
            _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, e.Message));
        else
            Console.WriteLine(e.Message);
    }

    public string GetDestinationColumnExpectedDataType()
    {
        return _anoTable.GetRuntimeDataType(LoadStage.PostLoad);
    }


    public static void ConfirmDependencies(DiscoveredDatabase database, ICheckNotifier notifier)
    {
        try
        {
            if (database.DiscoverStoredprocedures().Any(p => p.Name.Equals(SubstitutionStoredProcedure)))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"successfully found {SubstitutionStoredProcedure} on {database}", CheckResult.Success));
            else
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Failed to find {SubstitutionStoredProcedure} on {database}", CheckResult.Fail));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Exception occurred when trying to find stored procedure {SubstitutionStoredProcedure} on {database}",
                CheckResult.Fail, e));
        }
    }
}