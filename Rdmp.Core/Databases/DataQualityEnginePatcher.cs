// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using FAnsi.Discovery;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using TypeGuesser;

namespace Rdmp.Core.Databases;

public sealed class DataQualityEnginePatcher : Patcher
{
    public DataQualityEnginePatcher() : base(2, "Databases.DataQualityEngineDatabase")
    {
        LegacyName = "DataQualityEngine.Database";
        SqlServerOnly = false;
    }

    public override Patch GetInitialCreateScriptContents(DiscoveredDatabase db)
    {
        var header = GetHeader(db.Server.DatabaseType, InitialScriptName, new Version(1, 0, 0));

        var sql = new StringBuilder();

        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "Evaluation", new[] { new DatabaseColumnRequest("DateOfEvaluation", new DatabaseTypeRequest(typeof(DateTime))), new DatabaseColumnRequest("CatalogueID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { IsAutoIncrement = true, IsPrimaryKey = true } }, null, false, null).TrimEnd()};");

        // foreign keys
        var evaluationId = new DiscoveredColumn(db.ExpectTable("Evaluation"), "ID", false);
        DatabaseColumnRequest columnState_Evaluation_ID;
        DatabaseColumnRequest rowState_Evaluation_ID;
        DatabaseColumnRequest periodicityState_Evaluation_ID;
        DatabaseColumnRequest annotation_Evaluation_ID;

        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "ColumnState", new[] { new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { IsAutoIncrement = true, AllowNulls = false, IsPrimaryKey = true }, columnState_Evaluation_ID = new DatabaseColumnRequest("Evaluation_ID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("TargetProperty", new DatabaseTypeRequest(typeof(string), 500)) { AllowNulls = true }, new DatabaseColumnRequest("DataLoadRunID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = true }, new DatabaseColumnRequest("CountCorrect", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("CountDBNull", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("ItemValidatorXML", new DatabaseTypeRequest(typeof(string))) { AllowNulls = true }, new DatabaseColumnRequest("CountMissing", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("CountWrong", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("CountInvalidatesRow", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("PivotCategory", new DatabaseTypeRequest(typeof(string), 50) { Unicode = true }) { AllowNulls = false } }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { columnState_Evaluation_ID, evaluationId } }, true, null).TrimEnd()};");

        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "RowState", new[] { rowState_Evaluation_ID = new DatabaseColumnRequest("Evaluation_ID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("Correct", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("Missing", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("Wrong", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("Invalid", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("DataLoadRunID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("ValidatorXML", new DatabaseTypeRequest(typeof(string)) { Unicode = true }) { AllowNulls = false }, new DatabaseColumnRequest("PivotCategory", new DatabaseTypeRequest(typeof(string), 50) { Unicode = true }) { AllowNulls = false, IsPrimaryKey = true } }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { rowState_Evaluation_ID, evaluationId } }, true, null).TrimEnd()};");


        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "PeriodicityState", new[] { periodicityState_Evaluation_ID = new DatabaseColumnRequest("Evaluation_ID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("Year", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("Month", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("CountOfRecords", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("RowEvaluation", new DatabaseTypeRequest(typeof(string), 50) { Unicode = true }) { AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("PivotCategory", new DatabaseTypeRequest(typeof(string), 50) { Unicode = true }) { AllowNulls = false, IsPrimaryKey = true } }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { periodicityState_Evaluation_ID, evaluationId } }, true, null).TrimEnd()};");

        sql.AppendLine(
            $"{db.Helper.GetCreateTableSql(db, "DQEGraphAnnotation", new[] { new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int))) { IsAutoIncrement = true, AllowNulls = false, IsPrimaryKey = true }, new DatabaseColumnRequest("StartX", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("StartY", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("EndX", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("EndY", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("Text", new DatabaseTypeRequest(typeof(string), 1000)) { AllowNulls = false }, annotation_Evaluation_ID = new DatabaseColumnRequest("Evaluation_ID", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false }, new DatabaseColumnRequest("Username", new DatabaseTypeRequest(typeof(string), 1000)) { AllowNulls = false }, new DatabaseColumnRequest("CreationDate", new DatabaseTypeRequest(typeof(DateTime))) { AllowNulls = false }, new DatabaseColumnRequest("AnnotationIsForGraph", new DatabaseTypeRequest(typeof(string), 100)) { AllowNulls = false }, new DatabaseColumnRequest("PivotCategory", new DatabaseTypeRequest(typeof(string), 1000)) { AllowNulls = false } }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn> { { annotation_Evaluation_ID, evaluationId } }, true, null).TrimEnd()};");

        return new Patch(InitialScriptName, header + sql);
    }

    public override SortedDictionary<string, Patch> GetAllPatchesInAssembly(DiscoveredDatabase db)
    {
        var basePatches = base.GetAllPatchesInAssembly(db);
        if (basePatches.Count > 5)
            throw new NotImplementedException(
                "Someone has added some patches, we need to think about how we handle those in MySql and Oracle! i.e. don't add them in '/QueryCachingDatabase/up' please");

        //this is empty because the only patch is already accounted for
        return new SortedDictionary<string, Patch>();
    }
}