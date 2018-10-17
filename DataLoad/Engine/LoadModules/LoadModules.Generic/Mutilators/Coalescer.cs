using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.Mutilators
{
    /// <summary>
    /// Resolves primary key collisions that are the result of non primary key fields being null in some records and not null in others (where primary keys of those records
    /// are the same).  Or to put it simpler, resolves primary key collisions by making records less null.  This can only be applied in the Adjust RAW stage of a data load.
    /// This creates deviation from ground truth of the data you are loading and reducing nullness might not always be correct according to your data.
    /// </summary>
    public class Coalescer : MatchingTablesMutilator
    {
        [DemandsInitialization("Pass true to create an index on the primary keys which are joined together (can improve performance)",DefaultValue=false)]
        public bool CreateIndex { get; set; }

        public Coalescer():base(LoadStage.AdjustRaw)
        {
            
        }
        
        protected override void MutilateTable(IDataLoadEventListener job, TableInfo tableInfo, DiscoveredTable table)
        {
            var server = table.Database.Server;

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to run Coalese on table " + table));

            var allCols = table.DiscoverColumns();

            var pkColumnInfos = tableInfo.ColumnInfos.Where(c => c.IsPrimaryKey).Select(c => c.GetRuntimeName()).ToArray();
            var nonPks = allCols.Where(c => !pkColumnInfos.Contains(c.GetRuntimeName())).ToArray();
            var pks = allCols.Except(nonPks).ToArray();

            if (!pkColumnInfos.Any())
                throw new Exception("Table '" + tableInfo + "' has no IsPrimaryKey columns");

            if (allCols.Length == pkColumnInfos.Length)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Skipping Coalesce on table " + table + " because it has no non primary key columns"));
                return;
            }

            //Get an update command for each non primary key column
            Dictionary<string, Task<int>> sqlCommands = new Dictionary<string, Task<int>>();

            foreach (DiscoveredColumn nonPk in nonPks)
                sqlCommands.Add(GetCommand(table, pks, nonPk), null);
            
            server.EnableAsync();

            using (var con = table.Database.Server.GetConnection())
            {
                con.Open();

                if (CreateIndex)
                {
                    var idxCmd = server.GetCommand(string.Format(@"CREATE INDEX IX_PK_{0} ON {0}({1});", table.GetRuntimeName(), string.Join(",", pks.Select(p => p.GetRuntimeName()))), con);
                    idxCmd.CommandTimeout = Timeout;
                    idxCmd.ExecuteNonQuery();
                }

                foreach (var sql in sqlCommands.Keys.ToArray())
                {
                    var cmd = server.GetCommand(sql, con);
                    cmd.CommandTimeout = Timeout;
                    sqlCommands[sql] = cmd.ExecuteNonQueryAsync();
                }

                Task.WaitAll(sqlCommands.Values.ToArray());
            }

            int affectedRows = sqlCommands.Values.Sum(t => t.Result);

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Coalesce on table '" + table + "' completed (" + affectedRows + " rows affected)"));
            
        }

        private string GetCommand(DiscoveredTable table, DiscoveredColumn[] pks, DiscoveredColumn nonPk)
        {
            List<CustomLine> sqlLines = new List<CustomLine>();
            sqlLines.Add(new CustomLine(string.Format("(t1.{0} is null AND t2.{0} is not null)", nonPk.GetRuntimeName()), QueryComponent.WHERE));
            sqlLines.Add(new CustomLine(string.Format("t1.{0} = COALESCE(t1.{0},t2.{0})", nonPk.GetRuntimeName()),QueryComponent.SET));
            sqlLines.AddRange(pks.Select(p=>new CustomLine(string.Format("t1.{0} = t2.{0}", p.GetRuntimeName()),QueryComponent.JoinInfoJoin)));

            var updateHelper = table.Database.Server.GetQuerySyntaxHelper().UpdateHelper;

            return updateHelper.BuildUpdate(
                table,
                table,
                sqlLines);
        }
    }
}
