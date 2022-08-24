// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Versioning;
using System;
using System.Collections.Generic;
using System.Text;
using TypeGuesser;

namespace Rdmp.Core.Databases
{
    public sealed class LoggingDatabasePatcher:Patcher
    {
        public LoggingDatabasePatcher():base(2,"Databases.LoggingDatabase")
        {
            LegacyName = "HIC.Logging.Database";
            SqlServerOnly = false;
        }


        public override Patch GetInitialCreateScriptContents(DiscoveredDatabase db)
        {
            var header = GetHeader(InitialScriptName, new Version(1, 0, 0));
            /*
             * 
             * 
DataLoadRun
DataLoadTask - Done
DataSet - Done
DataSource
FatalError
ProgressLog
RowError
TableLoadRun
z_DataLoadTaskStatus
z_FatalErrorStatus
z_RowErrorType
             * 
             * */

            var sql = new StringBuilder();

            sql.AppendLine(db.Helper.GetCreateTableSql(db, "DataSet", new[]
            {
                new DatabaseColumnRequest("dataSetID",new DatabaseTypeRequest(typeof(string),450){Unicode = true}){IsPrimaryKey = true},
                new DatabaseColumnRequest("name",new DatabaseTypeRequest(typeof(string),2000){Unicode = true}),
                new DatabaseColumnRequest("description",new DatabaseTypeRequest(typeof(string),int.MaxValue){Unicode = true}),
                new DatabaseColumnRequest("time_period",new DatabaseTypeRequest(typeof(string),64){Unicode = true}),
                new DatabaseColumnRequest("SLA_required",new DatabaseTypeRequest(typeof(string),3){Unicode = true}),
                new DatabaseColumnRequest("supplier_name",new DatabaseTypeRequest(typeof(string),32){Unicode = true}),
                new DatabaseColumnRequest("supplier_tel_no",new DatabaseTypeRequest(typeof(string),32){Unicode = true}),
                new DatabaseColumnRequest("supplier_email",new DatabaseTypeRequest(typeof(string),64){Unicode = true}),
                new DatabaseColumnRequest("contact_name",new DatabaseTypeRequest(typeof(string),64){Unicode = true}),
                new DatabaseColumnRequest("contact_position",new DatabaseTypeRequest(typeof(string),64){Unicode = true}),
                new DatabaseColumnRequest("currentContactInstitutions",new DatabaseTypeRequest(typeof(string),64){Unicode = true}),
                new DatabaseColumnRequest("contact_tel_no",new DatabaseTypeRequest(typeof(string),32){Unicode = true}),
                new DatabaseColumnRequest("contact_email",new DatabaseTypeRequest(typeof(string),64){Unicode = true}),
                new DatabaseColumnRequest("frequency",new DatabaseTypeRequest(typeof(string),32){Unicode = true}),
                new DatabaseColumnRequest("method",new DatabaseTypeRequest(typeof(string),16){Unicode = true})
            }, null, false, null));

            // foreign keys
            var datasetId = new DiscoveredColumn(db.ExpectTable("DataSet"), "dataSetID", false);
            DatabaseColumnRequest dataLoadTask_datasetID;

            sql.AppendLine(db.Helper.GetCreateTableSql(db, "DataLoadTask", new[]
            {
                new DatabaseColumnRequest("ID",new DatabaseTypeRequest(typeof(int))){IsAutoIncrement = true, AllowNulls = false, IsPrimaryKey = true},
                new DatabaseColumnRequest("description",new DatabaseTypeRequest(typeof(string),int.MaxValue){Unicode = true}),
                new DatabaseColumnRequest("name",new DatabaseTypeRequest(typeof(string),1000){Unicode = true}),
                new DatabaseColumnRequest("createTime", new DatabaseTypeRequest(typeof(DateTime))){Default = FAnsi.Discovery.QuerySyntax.MandatoryScalarFunctions.GetTodaysDate},
                
                // TODO: does this have a default on it?
                new DatabaseColumnRequest("userAccount",new DatabaseTypeRequest(typeof(string),500){Unicode = true}),
                new DatabaseColumnRequest("statusID", new DatabaseTypeRequest(typeof(int))),
                new DatabaseColumnRequest("isTest", new DatabaseTypeRequest(typeof(bool))),
                dataLoadTask_datasetID = new DatabaseColumnRequest("dataSetID", new DatabaseTypeRequest(typeof(string), 450) { Unicode = true }),
            }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn>
            {
                {dataLoadTask_datasetID ,datasetId }
            }, true, null));

            /*
            sql.AppendLine(db.Helper.GetCreateTableSql(db, "RowState", new[]
            {
                rowState_Evaluation_ID = new DatabaseColumnRequest("Evaluation_ID",new DatabaseTypeRequest(typeof(int))){AllowNulls = false, IsPrimaryKey = true },
                new DatabaseColumnRequest("Correct",new DatabaseTypeRequest(typeof(int))){AllowNulls = false},
                new DatabaseColumnRequest("Missing",new DatabaseTypeRequest(typeof(int))){AllowNulls = false},
                new DatabaseColumnRequest("Wrong",new DatabaseTypeRequest(typeof(int))){AllowNulls = false},
                new DatabaseColumnRequest("Invalid",new DatabaseTypeRequest(typeof(int))){AllowNulls = false},
                new DatabaseColumnRequest("DataLoadRunID",new DatabaseTypeRequest(typeof(int))){AllowNulls = false, IsPrimaryKey = true},
                new DatabaseColumnRequest("ValidatorXML",new DatabaseTypeRequest(typeof(string)){Unicode = true}){AllowNulls = false},
                new DatabaseColumnRequest("PivotCategory",new DatabaseTypeRequest(typeof(string),50){Unicode = true}){AllowNulls = false, IsPrimaryKey = true},
            }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn>
            {
                {rowState_Evaluation_ID ,evaluationId }
            }, true, null));


            sql.AppendLine(db.Helper.GetCreateTableSql(db, "PeriodicityState", new[]
            {
                periodicityState_Evaluation_ID = new DatabaseColumnRequest("Evaluation_ID", new DatabaseTypeRequest(typeof(int))){AllowNulls = false, IsPrimaryKey = true },
                new DatabaseColumnRequest("Year", new DatabaseTypeRequest(typeof(int))){AllowNulls = false, IsPrimaryKey = true},
                new DatabaseColumnRequest("Month", new DatabaseTypeRequest(typeof(int))){AllowNulls = false, IsPrimaryKey = true},
                new DatabaseColumnRequest("CountOfRecords", new DatabaseTypeRequest(typeof(int))){AllowNulls = false},
                new DatabaseColumnRequest("RowEvaluation", new DatabaseTypeRequest(typeof(string),50){Unicode = true}){AllowNulls = false, IsPrimaryKey = true},
                new DatabaseColumnRequest("PivotCategory", new DatabaseTypeRequest(typeof(string),50){Unicode = true}){AllowNulls = false, IsPrimaryKey = true},
            }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn>
            {
                {periodicityState_Evaluation_ID ,evaluationId }
            }, true, null));

            sql.AppendLine(db.Helper.GetCreateTableSql(db, "DQEGraphAnnotation", new[]
            {
                new DatabaseColumnRequest("ID",new DatabaseTypeRequest(typeof(int))){IsAutoIncrement = true, AllowNulls = false, IsPrimaryKey = true},
                new DatabaseColumnRequest("StartX",new DatabaseTypeRequest(typeof(int))){AllowNulls = false},
                new DatabaseColumnRequest("StartY",new DatabaseTypeRequest(typeof(int))){AllowNulls = false},
                new DatabaseColumnRequest("EndX",new DatabaseTypeRequest(typeof(int))){AllowNulls = false},
                new DatabaseColumnRequest("EndY",new DatabaseTypeRequest(typeof(int))){AllowNulls = false},
                new DatabaseColumnRequest("Text",new DatabaseTypeRequest(typeof(string),1000)){AllowNulls = false},
                annotation_Evaluation_ID = new DatabaseColumnRequest("Evaluation_ID",new DatabaseTypeRequest(typeof(int))){AllowNulls = false},
                new DatabaseColumnRequest("Username",new DatabaseTypeRequest(typeof(string),1000)){AllowNulls = false},
                new DatabaseColumnRequest("CreationDate",new DatabaseTypeRequest(typeof(DateTime))){AllowNulls = false},
                new DatabaseColumnRequest("AnnotationIsForGraph",new DatabaseTypeRequest(typeof(string),100)){AllowNulls = false},
                new DatabaseColumnRequest("PivotCategory",new DatabaseTypeRequest(typeof(string),1000)){AllowNulls = false },
            }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn>
            {
                {annotation_Evaluation_ID ,evaluationId }
            }, true, null));
            */
            return new Patch(InitialScriptName, header + sql);
        }

        public override SortedDictionary<string, Patch> GetAllPatchesInAssembly(DiscoveredDatabase db)
        {
            var basePatches = base.GetAllPatchesInAssembly(db);
            if (basePatches.Count > 3)
                throw new NotImplementedException("Someone has added some patches, we need to think about how we handle those in MySql and Oracle! i.e. don't add them in '/LoggingDatabase/up' please");

            //this is empty because the only patch is already accounted for
            return new SortedDictionary<string, Patch>();
        }
    }
}
