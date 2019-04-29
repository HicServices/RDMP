// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Rdmp.Core.CatalogueLibrary;
using Rdmp.Core.CatalogueLibrary.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime
{
    /// <summary>
    /// RuntimeTask that executes a single .sql file specified by the user in a ProcessTask with ProcessTaskType SQLFile.
    /// </summary>
    public class ExecuteSqlFileRuntimeTask : RuntimeTask
    {
        public string Filepath;
        private IProcessTask _task;

        Regex _regexEntity = new Regex(@"{([CT]):(\d+)}",RegexOptions.IgnoreCase);
        private IDataLoadJob _job;
        private LoadStage _loadStage;

        public ExecuteSqlFileRuntimeTask(IProcessTask task, RuntimeArgumentCollection args) : base(task, args)
        {
            _task = task;
            Filepath = task.Path;
        }

        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            var db = RuntimeArguments.StageSpecificArguments.DbInfo;
            _job = job;

            _loadStage = RuntimeArguments.StageSpecificArguments.LoadStage;

            if (!Exists())
                throw new Exception("The sql file " + Filepath + " does not exist");

            string commandText;
            try
            {
                commandText = File.ReadAllText(Filepath);

                // Any string arguments refer to tokens that are to be replaced in the SQL file
                foreach (var kvp in RuntimeArguments.GetAllArgumentsOfType<string>())
                {
                    var value = kvp.Value;
                    
                    if (value.Contains("<DatabaseServer>"))
                        value = value.Replace("<DatabaseServer>", RuntimeArguments.StageSpecificArguments.DbInfo.Server.Name);

                    if (value.Contains("<DatabaseName>"))
                        value = value.Replace("<DatabaseName>", RuntimeArguments.StageSpecificArguments.DbInfo.GetRuntimeName());
                    
                    commandText = commandText.Replace("##" + kvp.Key + "##", value);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Could not read the sql file at " + Filepath + ": " + e);
            }
            
            commandText = _regexEntity.Replace(commandText, GetEntityForMatch);

            try
            {
                Dictionary<int,Stopwatch> performance = new Dictionary<int, Stopwatch>();

                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Executing script " + Filepath + " (" + db.DescribeDatabase() + ")"));
                using (var con = db.Server.GetConnection())
                {
                    con.Open();
                    UsefulStuff.ExecuteBatchNonQuery(commandText, con, null, out performance, 600000);
                }

                foreach (KeyValuePair<int, Stopwatch> section in performance)
                    job.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Information,
                            "Batch ending on line  \"" + section.Key + "\" finished after " + section.Value.Elapsed));
            }
            catch (Exception e)
            {
                throw new Exception("Failed to execute the query from " + Filepath + ": " + e);
            }

            return ExitCodeType.Success;
        }

        private string GetEntityForMatch(Match match)
        {
            if (match.Groups.Count != 3)
                throw new ExecuteSqlFileRuntimeTaskException("Regex Match in Sql File had " + match.Groups.Count + " Groups, expected 3,  Match was:'" + match.Value + "'");

            char entity;
            int id;
            try
            {
                entity = match.Groups[1].Value.ToUpper()[0];
                id = int.Parse(match.Groups[2].Value);
            }
            catch (Exception e)
            {
                throw new ExecuteSqlFileRuntimeTaskException("Error performing substitution in Sql File, Failed to replace match " + match.Value + " due to parse expectations" ,e);
            }


            var tables = _job.RegularTablesToLoad.Union(_job.LookupTablesToLoad);


            var syntaxHelper = RuntimeArguments.StageSpecificArguments.DbInfo.Server.GetQuerySyntaxHelper();
            var namer = _job.Configuration.DatabaseNamer;

            switch (entity)
            {
                case 'T':
                    var toReturnTable = tables.SingleOrDefault(t => t.ID == id);

                    if (toReturnTable == null)
                        throw new ExecuteSqlFileRuntimeTaskException("Failed to find a TableInfo in the load with ID "+id + ".  All TableInfo IDs referenced in script " + Filepath + " must be part of the LoadMetadata");

                    return toReturnTable.GetRuntimeName(_loadStage, namer);

                case 'C':

                    var toReturnColumn = tables.SelectMany(t=>t.ColumnInfos).SingleOrDefault(t => t.ID == id);

                    if (toReturnColumn == null)
                        throw new ExecuteSqlFileRuntimeTaskException("Failed to find a ColumnInfo in the load with ID " + id + ".  All ColumnInfo IDs referenced in script " + Filepath + " must be part of the LoadMetadata");

                    var db = toReturnColumn.TableInfo.GetDatabaseRuntimeName(_loadStage, namer);
                    var tbl = toReturnColumn.TableInfo.GetRuntimeName(_loadStage, namer);
                    var col = toReturnColumn.GetRuntimeName(_loadStage);

                    return syntaxHelper.EnsureFullyQualified(db, null, tbl, col);

                default :
                    throw new ExecuteSqlFileRuntimeTaskException("Error performing substitution in Sql File, Unexpected Type char in regex:" + entity);
            }
        }


        public override bool Exists()
        {
            return File.Exists(Filepath);
        }
        
        public override void Abort(IDataLoadEventListener postLoadEventListener)
        {
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
            
        }

        public override void Check(ICheckNotifier notifier)
        {
            if (string.IsNullOrWhiteSpace(Filepath))
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs("ExecuteSqlFileTask " + _task + " does not have a path specified",
                        CheckResult.Fail));
                return;
            }
            
            if (!File.Exists(Filepath))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "File '" + Filepath +
                        "' does not exist! (the only time this would be legal is if you have an exe or a freaky plugin that creates this file)",
                        CheckResult.Warning));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Found File '" + Filepath + "'",
                    CheckResult.Success));
        }
    }
}
