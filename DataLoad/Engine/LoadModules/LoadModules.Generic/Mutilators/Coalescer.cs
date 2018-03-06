using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Job;
using DataLoadEngine.Mutilators;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Mutilators
{
    /// <summary>
    /// Resolves primary key collisions that are the result of non primary key fields being null in some records and not null in others (where primary keys of those records
    /// are the same).  Or to put it simpler, resolves primary key collisions by making records less null.  This can only be applied in the Adjust RAW stage of a data load.
    /// This creates deviation from ground truth of the data you are loading and reducing nullness might not always be correct according to your data.
    /// </summary>
    public class Coalescer : IPluginMutilateDataTables
    {
        [DemandsInitialization("All tables in RAW matching this pattern which have a TableInfo defined in the load will be affected by this mutilation",Mandatory = true,DefaultValue = ".*")]
        public Regex TableRegexPattern { get; set; }

        public void Check(ICheckNotifier notifier)
        {
        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
        }

        private DiscoveredDatabase _dbInfo;
        public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
        {
            if (loadStage != LoadStage.AdjustRaw)
                throw new Exception("Coalescer can only be used in a RAW environment, current load stage is:" + loadStage);

            _dbInfo = dbInfo;
        }

        public ExitCodeType Mutilate(IDataLoadEventListener job)
        {
            var j = (IDataLoadJob) job;
            foreach (var tableInfo in j.RegularTablesToLoad)
            {

                var tbl = _dbInfo.ExpectTable(tableInfo.GetRuntimeName());
                var tblName = tbl.GetRuntimeName();

                if (tbl.Exists() && TableRegexPattern.IsMatch(tblName))
                {
                    job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information,"About to run Coalese on table " + tbl));

                    var pks = tableInfo.ColumnInfos.Where(c => c.IsPrimaryKey).Select(c => c.GetRuntimeName()).ToArray();
                    var nonPks = tbl.DiscoverColumns().Select(c => c.GetRuntimeName()).Except(pks).ToArray();

                    if (!nonPks.Any())
                    {
                        job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning,"Skipping Coalesce on table "+ tbl + " because it has no non primary key columns"));
                        continue;
                    }

                    //Get an update command for each non primary key column
                    Dictionary<string,Task<int>> sqlCommands = new Dictionary<string, Task<int>>();

                    foreach (string nonPk in nonPks)
                        sqlCommands.Add(GetCommand(pks, nonPk, tblName),null);

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    _dbInfo.Server.EnableAsync();

                    using (var con = _dbInfo.Server.GetConnection())
                    {
                        con.Open();

                        foreach (var sql in sqlCommands.Keys.ToArray())
                            sqlCommands[sql] = _dbInfo.Server.GetCommand(sql, con).ExecuteNonQueryAsync();

                        Task.WaitAll(sqlCommands.Values.ToArray());
                    }

                    int affectedRows = sqlCommands.Values.Sum(t => t.Result);

                    sw.Stop();
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Coalesce on table '" + tbl + "' completed after " + sw.ElapsedMilliseconds.ToString("N0") + " ms (" + affectedRows + " rows affected)"));


                }

            }

            return ExitCodeType.Success;
        }

        private string GetCommand(string[] pks, string nonPk, string tblName)
        {

            string updateSql =  string.Format("t1.{0} = COALESCE(t1.{0},t2.{0})", nonPk);
            string whereSql = string.Format("(t1.{0} is null AND t2.{0} is not null)", nonPk);
            
            var joinSql = string.Join(" AND " + Environment.NewLine,
                pks
                .Select(pk => string.Format("t1.{0} = t2.{0}", pk)));

            return 
                    string.Format(
@"UPDATE t1
  SET 
    {0}
  FROM {1} AS t1
  INNER JOIN {1} AS t2
  ON {2}
WHERE
{3}",updateSql, tblName, joinSql,whereSql);
        }
    }
}
