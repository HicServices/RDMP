// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Exceptions;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.DataLoad.Triggers.Implementations;

/// <summary>
///     Postgres triggers require an accompanying stored procedure.  This class handles creating the proc and trigger
/// </summary>
public class PostgreSqlTriggerImplementer : TriggerImplementer
{
    private readonly string _triggerRuntimeName;
    private readonly string _procedureNameFullyQualified;
    private readonly string _procedureRuntimeName;

    /// <inheritdoc cref="TriggerImplementer(DiscoveredTable,bool)" />
    public PostgreSqlTriggerImplementer(DiscoveredTable table, bool createDataLoadRunIDAlso) : base(table,
        createDataLoadRunIDAlso)
    {
        var schema = string.IsNullOrWhiteSpace(_table.Schema)
            ? table.GetQuerySyntaxHelper().GetDefaultSchemaIfAny()
            : _table.Schema;
        _triggerRuntimeName = $"{_table.GetRuntimeName()}_OnUpdate";

        _procedureRuntimeName = $"{_table.GetRuntimeName()}_OnUpdateProc";
        _procedureNameFullyQualified = $"{schema}.\"{_procedureRuntimeName}\"";
    }

    public override void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger)
    {
        problemsDroppingTrigger = "";
        thingsThatWorkedDroppingTrigger = "";

        try
        {
            using var con = _server.GetConnection();
            con.Open();

            using (var cmd = _server.GetCommand(
                       $"DROP TRIGGER IF EXISTS \"{_triggerRuntimeName}\" ON {_table.GetFullyQualifiedName()}", con))
            {
                cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                cmd.ExecuteNonQuery();
            }

            using (var cmd = _server.GetCommand($"DROP FUNCTION IF EXISTS  {_procedureNameFullyQualified}", con))
            {
                cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                cmd.ExecuteNonQuery();
            }

            thingsThatWorkedDroppingTrigger = $"Dropped trigger {_triggerRuntimeName}";
        }
        catch (Exception exception)
        {
            //this is not a problem really since it is likely that DLE chose to recreate the trigger because it was FUBARed or missing, this is just belt and braces try and drop anything that is lingering, whether or not it is there
            problemsDroppingTrigger += $"Failed to drop Trigger:{exception.Message}{Environment.NewLine}";
        }
    }

    public override string CreateTrigger(ICheckNotifier notifier)
    {
        var creationSql = base.CreateTrigger(notifier);
        using var con = _server.GetConnection();
        con.Open();

        CreateProcedure(con);

        var sql =
            $@"CREATE TRIGGER ""{_triggerRuntimeName}"" BEFORE UPDATE ON {_table.GetFullyQualifiedName()} FOR EACH ROW
EXECUTE PROCEDURE {_procedureNameFullyQualified}();";

        using var cmd = _server.GetCommand(sql, con);
        cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
        cmd.ExecuteNonQuery();

        return creationSql;
    }

    private void CreateProcedure(DbConnection con)
    {
        var sql = $@"CREATE OR REPLACE FUNCTION {_procedureNameFullyQualified}()
  RETURNS trigger AS
$$
{CreateTriggerBody()}
$$
LANGUAGE 'plpgsql';";

        using var cmd = _server.GetCommand(sql, con);
        cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
        cmd.ExecuteNonQuery();
    }

    protected string GetTriggerBody()
    {
        using var con = _server.GetConnection();
        con.Open();

        using var cmd =
            _server.GetCommand($"select proname,prosrc from pg_proc where proname= '{_procedureRuntimeName}';", con);
        using var r = cmd.ExecuteReader();
        return r.Read() ? r["prosrc"] as string : null;
    }

    public override bool CheckUpdateTriggerIsEnabledAndHasExpectedBody()
    {
        base.CheckUpdateTriggerIsEnabledAndHasExpectedBody();

        var sqlThen = GetTriggerBody();
        var sqlNow = CreateTriggerBody();

        if (sqlThen != null)
            sqlThen = Regex.Replace(sqlThen, @"\s+", " ").Trim();

        if (sqlNow != null)
            sqlNow = Regex.Replace(sqlNow, @"\s+", " ").Trim();

        AssertTriggerBodiesAreEqual(sqlThen, sqlNow);

        return true;
    }

    private static void AssertTriggerBodiesAreEqual(string sqlThen, string sqlNow)
    {
        if (!sqlNow.Equals(sqlThen))
            throw new ExpectedIdenticalStringsException("Sql body for trigger doesn't match expected sql", sqlNow,
                sqlThen);
    }

    private string CreateTriggerBody()
    {
        var syntax = _table.GetQuerySyntaxHelper();

        return
            $@"BEGIN
            INSERT INTO {_archiveTable.GetFullyQualifiedName()}({string.Join(",", _columns.Select(c => syntax.EnsureWrapped(c.GetRuntimeName())))},""hic_validTo"",""hic_userID"",hic_status)
            VALUES({string.Join(",", _columns.Select(c => $"OLD.{syntax.EnsureWrapped(c.GetRuntimeName())}"))},now(),current_user,'U');

            NEW.{syntax.EnsureWrapped(SpecialFieldNames.ValidFrom)} := NOW();
 
            RETURN NEW;
            END;";
    }

    public override TriggerStatus GetTriggerStatus()
    {
        return string.IsNullOrWhiteSpace(GetTriggerBody()) ? TriggerStatus.Missing : TriggerStatus.Enabled;
    }
}