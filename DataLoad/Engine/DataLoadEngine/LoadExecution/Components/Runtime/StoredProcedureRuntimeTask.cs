// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Management.Instrumentation;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Arguments;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    /// <summary>
    /// RuntimeTask that executes a single stored procedure specified by the user in ProcessTask.Path.  The StoredProcedure must be declared in the database
    /// appropriate to the stage the ProcessTask is executing at e.g. if it is in AdjustRAW then the proc must be declared in the RAW database.
    /// </summary>
    public class StoredProcedureRuntimeTask : RuntimeTask
    {
        private readonly CatalogueRepository _repository;
        private readonly string _storedProcedureName;
        public string ConnectionString { get; set; }

        public StoredProcedureRuntimeTask(IProcessTask task, RuntimeArgumentCollection args, CatalogueRepository repository)
            : base(task, args)
        {
            _repository = repository;
            _storedProcedureName = task.Path;
        }


        override public ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to run Task '" + ProcessTask.Name + "'"));

            using (var con = _repository.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand(_storedProcedureName, con.Connection, con.Transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                
                RuntimeArguments.IterateAllArguments((key, value) =>
                {
                    DatabaseCommandHelper.AddParameterWithValueToCommand(key, cmd, value);
                    return true;
                });

                try
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Created stored procedure command"));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Error executing stored procedure", e));
                    return ExitCodeType.Error;
                }
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Executed stored procedure successfully"));
                return ExitCodeType.Success;
            }
        }

        override public bool Exists()
        {
            using (var con = _repository.GetConnection())
            {
                var cmd =
                    DatabaseCommandHelper.GetCommand(
                        "SELECT CASE WHEN EXISTS ((SELECT * FROM sysobjects WHERE type='P' AND name='" +
                        _storedProcedureName + "')) THEN 1 ELSE 0", con.Connection, con.Transaction);
                try
                {
                    return (int)cmd.ExecuteScalar() == 1;
                }
                catch (Exception e)
                {
                    throw new Exception("Could not test existence of stored procedure '" + _storedProcedureName + "' - " + e);
                }
            }
        }

        public override void Abort(IDataLoadEventListener postLoadEventListener)
        {
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
            
        }

        public override void Check(ICheckNotifier checker)
        {
        }

    }
}