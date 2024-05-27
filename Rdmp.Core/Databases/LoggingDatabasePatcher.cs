// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using TypeGuesser;

namespace Rdmp.Core.Databases;

public sealed class LoggingDatabasePatcher : Patcher
{
    public LoggingDatabasePatcher() : base(2, "Databases.LoggingDatabase")
    {
        LegacyName = "HIC.Logging.Database";
        SqlServerOnly = false;
    }


    public override Patch GetInitialCreateScriptContents(DiscoveredDatabase db)
    {
        var header = GetHeader(db.Server.DatabaseType, InitialScriptName, new Version(1, 0, 0));

        var sql = new StringBuilder();

        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "DataSet", new[] { new DatabaseColumnRequest("dataSetID", new DatabaseTypeRequest(typeof(string), 150) { Unicode = true }) { IsPrimaryKey = true }, new DatabaseColumnRequest("name", new DatabaseTypeRequest(typeof(string), 2000) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("time_period", new DatabaseTypeRequest(typeof(string), 64) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("SLA_required", new DatabaseTypeRequest(typeof(string), 3) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("supplier_name", new DatabaseTypeRequest(typeof(string), 32) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("supplier_tel_no", new DatabaseTypeRequest(typeof(string), 32) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("supplier_email", new DatabaseTypeRequest(typeof(string), 64) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("contact_name", new DatabaseTypeRequest(typeof(string), 64) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("contact_position", new DatabaseTypeRequest(typeof(string), 64) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("currentContactInstitutions", new DatabaseTypeRequest(typeof(string), 64) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("contact_tel_no", new DatabaseTypeRequest(typeof(string), 32) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("contact_email", new DatabaseTypeRequest(typeof(string), 64) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("frequency", new DatabaseTypeRequest(typeof(string), 32) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("method", new DatabaseTypeRequest(typeof(string), 16) { Unicode = true }) { AllowNulls = true } }, null, false).TrimEnd()};");


        // foreign keys
        var datasetId = new DiscoveredColumn(db.ExpectTable("DataSet"), "dataSetID", false);
        DatabaseColumnRequest dataLoadTask_datasetID;
        var dataLoadTask_ID = new DiscoveredColumn(db.ExpectTable("DataLoadTask"), "ID", false);
        DatabaseColumnRequest dataLoadRun_dataLoadTaskID;
        var dataLoadRun_ID = new DiscoveredColumn(db.ExpectTable("DataLoadRun"), "ID", false);
        DatabaseColumnRequest tableLoadRun_dataLoadRunID;
        var tableLoadRun_ID = new DiscoveredColumn(db.ExpectTable("TableLoadRun"), "ID", false);
        DatabaseColumnRequest dataSource_tableLoadRunID;
        DatabaseColumnRequest fatalError_dataLoadRunID;
        DatabaseColumnRequest progressLog_dataLoadRunID;
        DatabaseColumnRequest rowError_tableLoadRunID;

        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "DataLoadTask", new[] { /*This is not an auto increment (see `GetMaxTaskID` method).  Not sure why it isn't but whatever */ new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }), new DatabaseColumnRequest("name", new DatabaseTypeRequest(typeof(string), 1000) { Unicode = true }), new DatabaseColumnRequest("createTime", new DatabaseTypeRequest(typeof(DateTime))) { Default = MandatoryScalarFunctions.GetTodaysDate }, /* TODO: does this have a default on it? */ new DatabaseColumnRequest("userAccount", new DatabaseTypeRequest(typeof(string), 500) { Unicode = true }), new DatabaseColumnRequest("statusID", new DatabaseTypeRequest(typeof(int))), new DatabaseColumnRequest("isTest", new DatabaseTypeRequest(typeof(bool))), dataLoadTask_datasetID = new DatabaseColumnRequest("dataSetID", new DatabaseTypeRequest(typeof(string), 150) { Unicode = true }) }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { dataLoadTask_datasetID, datasetId } }, true).TrimEnd()};");


        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "DataLoadRun", new[] { new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { IsAutoIncrement = true, AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }), new DatabaseColumnRequest("startTime", new DatabaseTypeRequest(typeof(DateTime))) { Default = MandatoryScalarFunctions.GetTodaysDate }, new DatabaseColumnRequest("endTime", new DatabaseTypeRequest(typeof(DateTime))) { AllowNulls = true }, dataLoadRun_dataLoadTaskID = new DatabaseColumnRequest("dataLoadTaskID", new DatabaseTypeRequest(typeof(int))), new DatabaseColumnRequest("isTest", new DatabaseTypeRequest(typeof(bool))), new DatabaseColumnRequest("packageName", new DatabaseTypeRequest(typeof(string), 750) { Unicode = true }), /* TODO: does this have a default on it? */ new DatabaseColumnRequest("userAccount", new DatabaseTypeRequest(typeof(string), 500) { Unicode = true }), new DatabaseColumnRequest("suggestedRollbackCommand", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }) { AllowNulls = true } }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { dataLoadRun_dataLoadTaskID, dataLoadTask_ID } }, true).TrimEnd()};");

        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "TableLoadRun", new[] { new DatabaseColumnRequest("startTime", new DatabaseTypeRequest(typeof(DateTime))) { Default = MandatoryScalarFunctions.GetTodaysDate }, new DatabaseColumnRequest("endTime", new DatabaseTypeRequest(typeof(DateTime))) { AllowNulls = true }, tableLoadRun_dataLoadRunID = new DatabaseColumnRequest("dataLoadRunID", new DatabaseTypeRequest(typeof(int))), new DatabaseColumnRequest("targetTable", new DatabaseTypeRequest(typeof(string), 200)), new DatabaseColumnRequest("expectedInserts", new DatabaseTypeRequest(typeof(long))) { AllowNulls = true }, new DatabaseColumnRequest("inserts", new DatabaseTypeRequest(typeof(long))) { AllowNulls = true }, new DatabaseColumnRequest("updates", new DatabaseTypeRequest(typeof(long))) { AllowNulls = true }, new DatabaseColumnRequest("deletes", new DatabaseTypeRequest(typeof(long))) { AllowNulls = true }, new DatabaseColumnRequest("errorRows", new DatabaseTypeRequest(typeof(long))) { AllowNulls = true }, new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { IsAutoIncrement = true, AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("duplicates", new DatabaseTypeRequest(typeof(long))) { AllowNulls = true }, new DatabaseColumnRequest("notes", new DatabaseTypeRequest(typeof(string), 8000)) { AllowNulls = true }, new DatabaseColumnRequest("suggestedRollbackCommand", new DatabaseTypeRequest(typeof(string), int.MaxValue)) { AllowNulls = true } }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { tableLoadRun_dataLoadRunID, dataLoadRun_ID } }, true).TrimEnd()};");

        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "DataSource", new[] { new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { IsAutoIncrement = true, AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("source", new DatabaseTypeRequest(typeof(string), int.MaxValue)), dataSource_tableLoadRunID = new DatabaseColumnRequest("tableLoadRunID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = true }, new DatabaseColumnRequest("archive", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("originDate", new DatabaseTypeRequest(typeof(DateTime))) { Default = MandatoryScalarFunctions.GetTodaysDate }, /*in old script this was binary(128) but I think string should be ok*/ new DatabaseColumnRequest("MD5", new DatabaseTypeRequest(typeof(string), 128)) { AllowNulls = true } }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { dataSource_tableLoadRunID, tableLoadRun_ID } }, true).TrimEnd()};");


        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "FatalError", new[] { new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { IsAutoIncrement = true, AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("time", new DatabaseTypeRequest(typeof(DateTime))) { Default = MandatoryScalarFunctions.GetTodaysDate }, new DatabaseColumnRequest("source", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }), new DatabaseColumnRequest("explanation", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }) { AllowNulls = true }, fatalError_dataLoadRunID = new DatabaseColumnRequest("dataLoadRunID", new DatabaseTypeRequest(typeof(int))), new DatabaseColumnRequest("statusID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = true }, new DatabaseColumnRequest("interestingToOthers", new DatabaseTypeRequest(typeof(bool))) { AllowNulls = true } }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { fatalError_dataLoadRunID, dataLoadRun_ID } }, true).TrimEnd()};");


        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "ProgressLog", new[] { progressLog_dataLoadRunID = new DatabaseColumnRequest("dataLoadRunID", new DatabaseTypeRequest(typeof(int))), new DatabaseColumnRequest("eventType", new DatabaseTypeRequest(typeof(string), 50) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }), new DatabaseColumnRequest("source", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("time", new DatabaseTypeRequest(typeof(DateTime))) { Default = MandatoryScalarFunctions.GetTodaysDate }, new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { IsAutoIncrement = true, AllowNulls = false, IsPrimaryKey = true } }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { progressLog_dataLoadRunID, dataLoadRun_ID } }, true).TrimEnd()};");

        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "RowError", new[] { new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { IsAutoIncrement = true, AllowNulls = false, IsPrimaryKey = true }, rowError_tableLoadRunID = new DatabaseColumnRequest("tableLoadRunID", new DatabaseTypeRequest(typeof(int))), new DatabaseColumnRequest("rowErrorTypeID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = true }, new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }), new DatabaseColumnRequest("locationOfRow", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }), new DatabaseColumnRequest("requiresReloading", new DatabaseTypeRequest(typeof(bool))) { AllowNulls = true }, new DatabaseColumnRequest("columnName", new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }) }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { rowError_tableLoadRunID, tableLoadRun_ID } }, true).TrimEnd()};");


        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "z_DataLoadTaskStatus", new[] { new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("status", new DatabaseTypeRequest(typeof(string), 50) { Unicode = true }) { AllowNulls = true }, new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), int.MaxValue)) { AllowNulls = true } }, null, true).TrimEnd()};");


        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "z_FatalErrorStatus", new[] { new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("status", new DatabaseTypeRequest(typeof(string), 20) { Unicode = true }) }, null, true).TrimEnd()};");


        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "z_RowErrorType", new[] { new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("type", new DatabaseTypeRequest(typeof(string), 20) { Unicode = true }) }, null, true).TrimEnd()};");

        var sh = db.Server.GetQuerySyntaxHelper();
        sql.AppendLine($"""

                        INSERT INTO {db.ExpectTable("z_DataLoadTaskStatus").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("status")}, {sh.EnsureWrapped("description")}) VALUES(1, 'Open', NULL);
                        INSERT INTO {db.ExpectTable("z_DataLoadTaskStatus").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("status")}, {sh.EnsureWrapped("description")}) VALUES(2, 'Ready', NULL);
                        INSERT INTO {db.ExpectTable("z_DataLoadTaskStatus").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("status")}, {sh.EnsureWrapped("description")}) VALUES(3, 'Committed', NULL);
                        INSERT INTO {db.ExpectTable("z_FatalErrorStatus").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("status")}) VALUES(1, 'Outstanding');
                        INSERT INTO {db.ExpectTable("z_FatalErrorStatus").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("status")}) VALUES(2, 'Resolved');
                        INSERT INTO {db.ExpectTable("z_FatalErrorStatus").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("status")}) VALUES(3, 'Blocked');
                        INSERT INTO {db.ExpectTable("z_RowErrorType").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("type")}) VALUES(1, 'LoadRow');
                        INSERT INTO {db.ExpectTable("z_RowErrorType").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("type")}) VALUES(2, 'Duplication');
                        INSERT INTO {db.ExpectTable("z_RowErrorType").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("type")}) VALUES(3, 'Validation');
                        INSERT INTO {db.ExpectTable("z_RowErrorType").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("type")}) VALUES(4, 'DatabaseOperation');
                        INSERT INTO {db.ExpectTable("z_RowErrorType").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("type")}) VALUES(5, 'Unknown');

                        /*create datasets*/
                        INSERT INTO {db.ExpectTable("DataSet").GetWrappedName()} ({sh.EnsureWrapped("dataSetID")}, {sh.EnsureWrapped("name")}, {sh.EnsureWrapped("description")}, {sh.EnsureWrapped("time_period")}, {sh.EnsureWrapped("SLA_required")}, {sh.EnsureWrapped("supplier_name")}, {sh.EnsureWrapped("supplier_tel_no")}, {sh.EnsureWrapped("supplier_email")}, {sh.EnsureWrapped("contact_name")}, {sh.EnsureWrapped("contact_position")}, {sh.EnsureWrapped("currentContactInstitutions")}, {sh.EnsureWrapped("contact_tel_no")}, {sh.EnsureWrapped("contact_email")}, {sh.EnsureWrapped("frequency")}, {sh.EnsureWrapped("method")}) VALUES(N'DataExtraction', 'DataExtraction', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
                        INSERT INTO {db.ExpectTable("DataSet").GetWrappedName()} ({sh.EnsureWrapped("dataSetID")}, {sh.EnsureWrapped("name")}, {sh.EnsureWrapped("description")}, {sh.EnsureWrapped("time_period")}, {sh.EnsureWrapped("SLA_required")}, {sh.EnsureWrapped("supplier_name")}, {sh.EnsureWrapped("supplier_tel_no")}, {sh.EnsureWrapped("supplier_email")}, {sh.EnsureWrapped("contact_name")}, {sh.EnsureWrapped("contact_position")}, {sh.EnsureWrapped("currentContactInstitutions")}, {sh.EnsureWrapped("contact_tel_no")}, {sh.EnsureWrapped("contact_email")}, {sh.EnsureWrapped("frequency")}, {sh.EnsureWrapped("method")}) VALUES(N'Internal', 'Internal', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

                        /*create tasks*/
                        INSERT INTO {db.ExpectTable("DataLoadTask").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("description")}, {sh.EnsureWrapped("name")}, {sh.EnsureWrapped("userAccount")}, {sh.EnsureWrapped("statusID")}, {sh.EnsureWrapped("isTest")}, {sh.EnsureWrapped("dataSetID")}) VALUES(1, 'Internal', 'Internal', 'Thomas', 1, 0, 'Internal');
                        INSERT INTO {db.ExpectTable("DataLoadTask").GetWrappedName()} ({sh.EnsureWrapped("ID")}, {sh.EnsureWrapped("description")}, {sh.EnsureWrapped("name")}, {sh.EnsureWrapped("userAccount")}, {sh.EnsureWrapped("statusID")}, {sh.EnsureWrapped("isTest")}, {sh.EnsureWrapped("dataSetID")}) VALUES(2, 'DataExtraction', 'DataExtraction', 'Thomas', 1, 0, 'DataExtraction');
                                               
                        """);


        return new Patch(InitialScriptName, header + sql);
    }

    public override SortedDictionary<string, Patch> GetAllPatchesInAssembly(DiscoveredDatabase db)
    {
        var basePatches = base.GetAllPatchesInAssembly(db);
        if (basePatches.Count > 3)
            throw new NotImplementedException(
                "Someone has added some patches, we need to think about how we handle those in MySql and Oracle! i.e. don't add them in '/LoggingDatabase/up' please");

        //this is empty because the only patch is already accounted for
        return new SortedDictionary<string, Patch>();
    }
}