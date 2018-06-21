using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Arguments;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    /// <summary>
    /// RuntimeTask that executes a single .sql file specified by the user in a ProcessTask with ProcessTaskType SQLFile.
    /// </summary>
    public class ExecuteSqlFileRuntimeTask : RuntimeTask
    {
        public string Filepath;
        private IProcessTask _task;

        public ExecuteSqlFileRuntimeTask(IProcessTask task, RuntimeArgumentCollection args) : base(task, args)
        {
            _task = task;
            Filepath = task.Path;
        }

        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            var db = RuntimeArguments.StageSpecificArguments.DbInfo;

            var newBuilder = new SqlConnectionStringBuilder(db.Server.Builder.ConnectionString);
            newBuilder.ConnectTimeout = 600;

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

            try
            {
                Dictionary<int,Stopwatch> performance = new Dictionary<int, Stopwatch>();

                UsefulStuff.ExecuteBatchNonQuery(commandText,new SqlConnection(newBuilder.ConnectionString),null,out performance,600000);
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Executing script " + Filepath + " (" + db.DescribeDatabase()+ ")"));

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
