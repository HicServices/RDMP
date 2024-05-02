// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using FAnsi.Implementations.MicrosoftSQL;
using Microsoft.Data.SqlClient;
using Rdmp.Core.DataLoad.Triggers.Exceptions;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Exceptions;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.DataLoad.Triggers.Implementations;

/// <summary>
///     Creates an _Archive table to match a live table and a Database Trigger On Update which moves old versions of
///     records to the _Archive table when the main table
///     is UPDATEd.  An _Archive table is an exact match of columns as the live table (which must have primary keys) but
///     also includes several audit fields (date it
///     was archived etc).  The _Archive table can be used to view the changes that occured during data loading (See
///     DiffDatabaseDataFetcher) and/or generate a
///     'way back machine' view of the data at a given date in the past (See CreateViewOldVersionsTableValuedFunction
///     method).
///     <para>
///         This class is super Microsoft Sql Server specific.  It is not suitable to create backup triggers on tables in
///         which you expect high volitility (lots of frequent
///         updates all the time).
///     </para>
///     <para>
///         Also contains methods for confirming that a trigger exists on a given table, that the primary keys still match
///         when it was created and the _Archive table hasn't
///         got a different schema to the live table (e.g. if you made a change to the live table this would pick up that
///         the _Archive wasn't updated).
///     </para>
/// </summary>
public class MicrosoftSQLTriggerImplementer : TriggerImplementer
{
    private readonly string _schema;
    private readonly string _triggerName;

    /// <inheritdoc cref="TriggerImplementer(DiscoveredTable,bool)" />
    public MicrosoftSQLTriggerImplementer(DiscoveredTable table, bool createDataLoadRunIDAlso = true) : base(table,
        createDataLoadRunIDAlso)
    {
        _schema = string.IsNullOrWhiteSpace(_table.Schema) ? "dbo" : _table.Schema;
        _triggerName = $"{_schema}.{GetTriggerName()}";
    }

    public override void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger)
    {
        using var con = _server.GetConnection();
        con.Open();

        problemsDroppingTrigger = "";
        thingsThatWorkedDroppingTrigger = "";

        using (var cmdDropTrigger = _server.GetCommand($"DROP TRIGGER {_triggerName}", con))
        {
            try
            {
                cmdDropTrigger.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                thingsThatWorkedDroppingTrigger += $"Dropped Trigger successfully{Environment.NewLine}";
                cmdDropTrigger.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                //this is not a problem really since it is likely that DLE chose to recreate the trigger because it was FUBARed or missing, this is just belt and braces try and drop anything that is lingering, whether or not it is there
                problemsDroppingTrigger += $"Failed to drop Trigger:{exception.Message}{Environment.NewLine}";
            }
        }

        using (var cmdDropArchiveIndex = _server.GetCommand(
                   $"DROP INDEX PKsIndex ON {_archiveTable.GetRuntimeName()}", con))
        {
            try
            {
                cmdDropArchiveIndex.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                cmdDropArchiveIndex.ExecuteNonQuery();

                thingsThatWorkedDroppingTrigger +=
                    $"Dropped index PKsIndex on Archive table successfully{Environment.NewLine}";
            }
            catch (Exception exception)
            {
                problemsDroppingTrigger += $"Failed to drop Archive Index:{exception.Message}{Environment.NewLine}";
            }
        }

        using var cmdDropArchiveLegacyView = _server.GetCommand($"DROP FUNCTION {_table.GetRuntimeName()}_Legacy", con);
        try
        {
            cmdDropArchiveLegacyView.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
            cmdDropArchiveLegacyView.ExecuteNonQuery();
            thingsThatWorkedDroppingTrigger += $"Dropped Legacy Table View successfully{Environment.NewLine}";
        }
        catch (Exception exception)
        {
            problemsDroppingTrigger +=
                $"Failed to drop Legacy Table View:{exception.Message}{Environment.NewLine}";
        }
    }

    public override string CreateTrigger(ICheckNotifier notifier)
    {
        var createArchiveTableSQL = base.CreateTrigger(notifier);

        using var con = _server.GetConnection();
        con.Open();

        var trigger = GetCreateTriggerSQL();

        using (var cmdAddTrigger = _server.GetCommand(trigger, con))
        {
            cmdAddTrigger.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
            cmdAddTrigger.ExecuteNonQuery();
        }


        //Add key so that we can more easily do comparisons on primary key between main table and archive
        var idxCompositeKeyBody = "";

        foreach (var key in _primaryKeys)
            idxCompositeKeyBody += $"[{key.GetRuntimeName()}] ASC,";

        //remove trailing comma
        idxCompositeKeyBody = idxCompositeKeyBody.TrimEnd(',');

        var createIndexSQL =
            $@"CREATE NONCLUSTERED INDEX [PKsIndex] ON {_archiveTable.GetFullyQualifiedName()} ({idxCompositeKeyBody})";
        using (var cmdCreateIndex = _server.GetCommand(createIndexSQL, con))
        {
            try
            {
                cmdCreateIndex.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                cmdCreateIndex.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Could not create index on archive table because of timeout, possibly your _Archive table has a lot of data in it",
                    CheckResult.Fail, e));

                return null;
            }
        }

        CreateViewOldVersionsTableValuedFunction(createArchiveTableSQL, con);

        return createArchiveTableSQL;
    }

    private string GetCreateTriggerSQL()
    {
        if (!_primaryKeys.Any())
            throw new TriggerException("There must be at least 1 primary key");

        //this is the SQL to join on the main table to the deleted to record the hic_validFrom
        var updateValidToWhere =
            $" UPDATE [{_table}] SET {SpecialFieldNames.ValidFrom} = GETDATE() FROM deleted where ";

        //its a combo field so join on both when filling in hic_validFrom
        foreach (var key in _primaryKeys)
            updateValidToWhere += $"[{_table}].[{key}] = deleted.[{key}] AND ";

        //trim off last AND
        updateValidToWhere = updateValidToWhere[..^"AND ".Length];

        var InsertedToDeletedJoin = "JOIN inserted i ON ";
        InsertedToDeletedJoin += GetTableToTableEqualsSqlWithPrimaryKeys("i", "d");

        var equalsSqlTableToInserted = GetTableToTableEqualsSqlWithPrimaryKeys(_table.GetRuntimeName(), "inserted");
        var equalsSqlTableToDeleted = GetTableToTableEqualsSqlWithPrimaryKeys(_table.GetRuntimeName(), "deleted");

        var columnNames = _columns.Select(c => c.GetRuntimeName()).ToList();

        if (!columnNames.Contains(SpecialFieldNames.DataLoadRunID, StringComparer.CurrentCultureIgnoreCase))
            columnNames.Add(SpecialFieldNames.DataLoadRunID);

        if (!columnNames.Contains(SpecialFieldNames.ValidFrom, StringComparer.CurrentCultureIgnoreCase))
            columnNames.Add(SpecialFieldNames.ValidFrom);

        var colList = string.Join(",", columnNames.Select(c => $"[{c}]"));
        var dDotColList = string.Join(",", columnNames.Select(c => $"d.[{c}]"));

        return
            $@"
CREATE TRIGGER {_triggerName} ON {_table.GetFullyQualifiedName()}
AFTER UPDATE, DELETE
AS BEGIN

SET NOCOUNT ON

declare @isPrimaryKeyChange bit = 0

--it will be a primary key change if deleted and inserted do not agree on primary key values
IF exists ( select 1 FROM deleted d RIGHT {InsertedToDeletedJoin} WHERE d.[{_primaryKeys.First().GetRuntimeName()}] is null)
begin
	UPDATE [{_table}] SET {SpecialFieldNames.ValidFrom} = GETDATE() FROM inserted where {equalsSqlTableToInserted}
	set @isPrimaryKeyChange = 1
end
else
begin
	UPDATE [{_table}] SET {SpecialFieldNames.ValidFrom} = GETDATE() FROM deleted where {equalsSqlTableToDeleted}
	set @isPrimaryKeyChange = 0
end

{updateValidToWhere}

INSERT INTO [{_archiveTable.GetRuntimeName()}] ({colList},hic_validTo,hic_userID,hic_status) SELECT {dDotColList}, GETDATE(), SYSTEM_USER, CASE WHEN @isPrimaryKeyChange = 1 then 'K' WHEN i.[{_primaryKeys.First().GetRuntimeName()}] IS NULL THEN 'D' WHEN d.[{_primaryKeys.First().GetRuntimeName()}] IS NULL THEN 'I' ELSE 'U' END
FROM deleted d 
LEFT {InsertedToDeletedJoin}

SET NOCOUNT OFF

RETURN
END  
";
    }

    private string GetTableToTableEqualsSqlWithPrimaryKeys(string table1, string table2)
    {
        var toReturn = "";

        foreach (var key in _primaryKeys)
            toReturn += $" [{table1}].[{key.GetRuntimeName()}] = [{table2}].[{key.GetRuntimeName()}] AND ";

        //trim off last AND
        toReturn = toReturn[..^"AND ".Length];

        return toReturn;
    }

    private void CreateViewOldVersionsTableValuedFunction(string sqlUsedToCreateArchiveTableSQL, DbConnection con)
    {
        var columnsInArchive = "";

        var syntaxHelper = MicrosoftQuerySyntaxHelper.Instance;

        var matchStartColumnExtraction = Regex.Match(sqlUsedToCreateArchiveTableSQL, "CREATE TABLE .*\\(");

        if (!matchStartColumnExtraction.Success)
            throw new Exception("Could not find regex match at start of Archive table CREATE SQL");

        var startExtractingColumnsAt = matchStartColumnExtraction.Index + matchStartColumnExtraction.Length;
        //trim off excess crud at start and we should have just the columns bit of the create (plus crud at the end)
        columnsInArchive = sqlUsedToCreateArchiveTableSQL[startExtractingColumnsAt..];

        //trim off excess crud at the end
        columnsInArchive = columnsInArchive.Trim(')', '\r', '\n');

        var sqlToRun = string.Format("CREATE FUNCTION [" + _schema + "].[{0}_Legacy]",
            QuerySyntaxHelper.MakeHeaderNameSensible(_table.GetRuntimeName()));
        sqlToRun += Environment.NewLine;
        sqlToRun += $"({Environment.NewLine}";
        sqlToRun += $"\t@index DATETIME{Environment.NewLine}";
        sqlToRun += $"){Environment.NewLine}";
        sqlToRun += $"RETURNS @returntable TABLE{Environment.NewLine}";
        sqlToRun += $"({Environment.NewLine}";
        sqlToRun += $"/*the return table will follow the structure of the Archive table*/{Environment.NewLine}";
        sqlToRun += columnsInArchive;

        //these were added during transaction so we have to specify them again here because transaction will not have been committed yet
        sqlToRun = sqlToRun.Trim();
        sqlToRun += $",{Environment.NewLine}";
        sqlToRun += $"\thic_validTo datetime,{Environment.NewLine}";
        sqlToRun += "\thic_userID varchar(128),";
        sqlToRun += "\thic_status char(1)";


        sqlToRun += $"){Environment.NewLine}";
        sqlToRun += $"AS{Environment.NewLine}";
        sqlToRun += $"BEGIN{Environment.NewLine}";
        sqlToRun += Environment.NewLine;

        var liveCols = _columns.Select(c => $"[{c.GetRuntimeName()}]").Union(new[]
        {
            $"[{SpecialFieldNames.DataLoadRunID}]", $"[{SpecialFieldNames.ValidFrom}]"
        }).ToArray();

        var archiveCols = $"{string.Join(",", liveCols)},hic_validTo,hic_userID,hic_status";
        var cDotArchiveCols = string.Join(",", liveCols.Select(s => $"c.{s}"));


        sqlToRun += $"\tINSERT @returntable{Environment.NewLine}";
        sqlToRun += string.Format(
            "\tSELECT " + archiveCols + " FROM [{0}] WHERE @index BETWEEN ISNULL(" + SpecialFieldNames.ValidFrom +
            ", '1899/01/01') AND hic_validTo" + Environment.NewLine, _archiveTable);
        sqlToRun += Environment.NewLine;

        sqlToRun += $"\tINSERT @returntable{Environment.NewLine}";
        sqlToRun +=
            $"\tSELECT {cDotArchiveCols},NULL AS hic_validTo, NULL AS hic_userID, 'C' AS hic_status{Environment.NewLine}"; //c is for current
        sqlToRun += string.Format("\tFROM [{0}] c" + Environment.NewLine, _table.GetRuntimeName());
        sqlToRun += $"\tLEFT OUTER JOIN @returntable a ON {Environment.NewLine}";

        for (var index = 0; index < _primaryKeys.Length; index++)
        {
            sqlToRun += string.Format("\ta.{0}=c.{0} " + Environment.NewLine,
                syntaxHelper.EnsureWrapped(_primaryKeys[index].GetRuntimeName())); //add the primary key joins

            if (index + 1 < _primaryKeys.Length)
                sqlToRun += $"\tAND{Environment.NewLine}"; //add an AND because there are more coming
        }

        sqlToRun += string.Format("\tWHERE a.[{0}] IS NULL -- where archive record doesn't exist" + Environment.NewLine,
            _primaryKeys.First().GetRuntimeName());
        sqlToRun += $"\tAND @index > ISNULL(c.{SpecialFieldNames.ValidFrom}, '1899/01/01'){Environment.NewLine}";

        sqlToRun += Environment.NewLine;
        sqlToRun += $"RETURN{Environment.NewLine}";
        sqlToRun += $"END{Environment.NewLine}";

        using var cmd = _server.GetCommand(sqlToRun, con);
        cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
        cmd.ExecuteNonQuery();
    }

    public override TriggerStatus GetTriggerStatus()
    {
        var updateTriggerName = GetTriggerName();

        var queryTriggerIsItDisabledOrMissing =
            $"USE [{_table.Database.GetRuntimeName()}]; \r\nif exists (select 1 from sys.triggers WHERE name=@triggerName) SELECT is_disabled  FROM sys.triggers WHERE name=@triggerName else select -1 is_disabled";

        try
        {
            using var conn = _server.GetConnection();
            conn.Open();
            using var cmd = _server.GetCommand(queryTriggerIsItDisabledOrMissing, conn);
            cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
            cmd.Parameters.Add(new SqlParameter("@triggerName", SqlDbType.VarChar));
            cmd.Parameters["@triggerName"].Value = updateTriggerName;

            var result = Convert.ToInt32(cmd.ExecuteScalar());

            return result switch
            {
                0 => TriggerStatus.Enabled,
                1 => TriggerStatus.Disabled,
                -1 => TriggerStatus.Missing,
                _ => throw new NotSupportedException($"Query returned unexpected value:{result}")
            };
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to check if trigger {updateTriggerName} is enabled: {e}");
        }
    }

    private string GetTriggerName()
    {
        return $"{QuerySyntaxHelper.MakeHeaderNameSensible(_table.GetRuntimeName())}_OnUpdate";
    }

    public override bool CheckUpdateTriggerIsEnabledAndHasExpectedBody()
    {
        var baseResult = base.CheckUpdateTriggerIsEnabledAndHasExpectedBody();

        if (!baseResult)
            return false;

        //now check the definition of it! - make sure it relates to primary keys etc
        var updateTriggerName = GetTriggerName();
        var query =
            $"USE [{_table.Database.GetRuntimeName()}];SELECT OBJECT_DEFINITION (object_id) FROM sys.triggers WHERE name='{updateTriggerName}' and is_disabled=0";

        try
        {
            using var con = _server.GetConnection();
            string result;

            con.Open();
            using (var cmd = _server.GetCommand(query, con))
            {
                cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                result = cmd.ExecuteScalar() as string;
            }


            if (string.IsNullOrWhiteSpace(result))
                throw new TriggerMissingException(
                    $"Trigger {updateTriggerName} does not have an OBJECT_DEFINITION or is missing or is disabled");

            var expectedSQL = GetCreateTriggerSQL();

            expectedSQL = expectedSQL.Trim();
            result = result.Trim();

            if (!expectedSQL.Equals(result))
                throw new ExpectedIdenticalStringsException($"Trigger {updateTriggerName} is corrupt",
                    expectedSQL, result);
        }
        catch (ExpectedIdenticalStringsException)
        {
            throw;
        }
        catch (IrreconcilableColumnDifferencesInArchiveException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Failed to check if trigger {updateTriggerName} is enabled.  See InnerException for details", e);
        }

        return true;
    }
}