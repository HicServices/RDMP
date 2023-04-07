// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi;
using FAnsi.Discovery;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Exceptions;
using ReusableLibraryCode.Settings;

namespace Rdmp.Core.DataLoad.Triggers.Implementations;

/// <summary>
/// Postgres triggers require an accompanying stored procedure.  This class handles creating the proc and trigger
/// </summary>
public class PostgreSqlTriggerImplementer : TriggerImplementer
{
    private string _schema;
    private string _triggerRuntimeName;
    private string _procedureNameFullyQualified;
    private string _procedureRuntimeName;

    /// <inheritdoc cref="TriggerImplementer(DiscoveredTable,bool)"/>
    public PostgreSqlTriggerImplementer(DiscoveredTable table, bool createDataLoadRunIDAlso):base(table,createDataLoadRunIDAlso)
    {
        _schema = string.IsNullOrWhiteSpace(_table.Schema) ? table.GetQuerySyntaxHelper().GetDefaultSchemaIfAny():_table.Schema;
        _triggerRuntimeName = _table.GetRuntimeName() + "_OnUpdate";
            
        _procedureRuntimeName = _table.GetRuntimeName() + "_OnUpdateProc";
        _procedureNameFullyQualified = _schema + ".\"" + _procedureRuntimeName + "\"";
    }

    public override void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger)
    {
        problemsDroppingTrigger = "";
        thingsThatWorkedDroppingTrigger = "";

        try
        {
            using (var con = _server.GetConnection())
            {
                con.Open();

                using(var cmd = _server.GetCommand("DROP TRIGGER IF EXISTS \"" + _triggerRuntimeName + "\" ON " + _table.GetFullyQualifiedName(), con))
                {
                    cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                    cmd.ExecuteNonQuery();
                }

                using(var cmd = _server.GetCommand("DROP FUNCTION IF EXISTS  " + _procedureNameFullyQualified, con))
                {
                    cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                    cmd.ExecuteNonQuery();
                }

                thingsThatWorkedDroppingTrigger = "Droppped trigger " + _triggerRuntimeName;
            }
        }
        catch (Exception exception)
        {
            //this is not a problem really since it is likely that DLE chose to recreate the trigger because it was FUBARed or missing, this is just belt and braces try and drop anything that is lingering, whether or not it is there
            problemsDroppingTrigger += "Failed to drop Trigger:" + exception.Message + Environment.NewLine;
        }
    }

    public override string CreateTrigger(ICheckNotifier notifier)
    {
        string creationSql = base.CreateTrigger(notifier);

        CreateProcedure(notifier);
            
        var sql = string.Format(@"CREATE TRIGGER ""{0}"" BEFORE UPDATE ON {1} FOR EACH ROW
EXECUTE PROCEDURE {2}();", 
            _triggerRuntimeName,
            _table.GetFullyQualifiedName(),
            _procedureNameFullyQualified);

        using (var con = _server.GetConnection())
        {
            con.Open();

            using(var cmd = _server.GetCommand(sql, con))
            {
                cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                cmd.ExecuteNonQuery();
            }
                    
        }

        return creationSql;
    }

    private void CreateProcedure(ICheckNotifier notifier)
    {
        var sql = string.Format(@"CREATE OR REPLACE FUNCTION {0}()
  RETURNS trigger AS
$$
{1}
$$
LANGUAGE 'plpgsql';"
            ,_procedureNameFullyQualified
            , CreateTriggerBody()
        );

        using (var con = _server.GetConnection())
        {
            con.Open();

            using(var cmd = _server.GetCommand(sql, con))
            {
                cmd.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                cmd.ExecuteNonQuery();
            }
                    
        }
    }

    protected string GetTriggerBody()
    {
        using (var con = _server.GetConnection())
        {
            con.Open();

            using(var cmd = _server.GetCommand($"select proname,prosrc from pg_proc where proname= '{_procedureRuntimeName}';", con))
            using (var r = cmd.ExecuteReader())
                return r.Read() ? r["prosrc"] as string:null;
        }
    }

    public override bool CheckUpdateTriggerIsEnabledAndHasExpectedBody()
    {
        base.CheckUpdateTriggerIsEnabledAndHasExpectedBody();

        var sqlThen = GetTriggerBody();
        var sqlNow = CreateTriggerBody();

        if(sqlThen != null)
            sqlThen = Regex.Replace(sqlThen,@"\s+", " ").Trim();

        if(sqlNow != null)
            sqlNow = Regex.Replace(sqlNow,@"\s+", " ").Trim();

        AssertTriggerBodiesAreEqual(sqlThen,sqlNow);

        return true;
    }

    private void AssertTriggerBodiesAreEqual(string sqlThen, string sqlNow)
    {
        if(!sqlNow.Equals(sqlThen))
            throw new ExpectedIdenticalStringsException("Sql body for trigger doesn't match expcted sql",sqlNow,sqlThen);
    }

    private string CreateTriggerBody()
    {
        var syntax = _table.GetQuerySyntaxHelper();

        return
            string.Format(@"BEGIN
            INSERT INTO {0}({1},""hic_validTo"",""hic_userID"",hic_status)
            VALUES({2},now(),current_user,'U');

            NEW.{3} := NOW();
 
            RETURN NEW;
            END;",
                _archiveTable.GetFullyQualifiedName()                                                                          
                , string.Join(",", _columns.Select(c => syntax.EnsureWrapped(c.GetRuntimeName()))),         
                string.Join(",", _columns.Select(c => "OLD." + syntax.EnsureWrapped(c.GetRuntimeName()))),
                syntax.EnsureWrapped(SpecialFieldNames.ValidFrom));

    }

    public override TriggerStatus GetTriggerStatus()
    {
        return string.IsNullOrWhiteSpace(GetTriggerBody()) ? TriggerStatus.Missing : TriggerStatus.Enabled;
    }
}