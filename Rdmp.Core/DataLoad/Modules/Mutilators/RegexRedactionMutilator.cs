using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax.Update;
using FAnsi.Discovery.QuerySyntax;
using MongoDB.Driver.Core.Servers;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.CommandExecution;
using System.Diagnostics;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

public class RegexRedactionMutilator : MatchingTablesMutilatorWithDataLoadJob
{
    [DemandsInitialization("the regex redaction configuration to use")]
    public RegexRedactionConfiguration redactionConfiguration { get; set; }

    [DemandsInitialization(
       "All Columns matching this pattern which have a ColumnInfo defined in the load will be affected by this mutilation",
       DefaultValue = ".*")]
    public Regex ColumnRegexPattern { get; set; }

    [DemandsInitialization(
        "Overrides ColumnRegexPattern.  If this is set then the columns chosen will be mutilated instead")]
    public ColumnInfo[] OnlyColumns { get; set; }

    private DiscoveredColumn[] _discoveredPKColumns;


    public RegexRedactionMutilator() : base([LoadStage.AdjustRaw, LoadStage.AdjustStaging]) { }

    private bool ColumnMatches(DiscoveredColumn column)
    {
        if (OnlyColumns.Length > 0)
        {
            return OnlyColumns.Select(c => c.GetRuntimeName()).Contains(column.GetRuntimeName());
        }
        if (ColumnRegexPattern != null)
        {
            ColumnRegexPattern = new Regex(ColumnRegexPattern.ToString(), RegexOptions.IgnoreCase);
            return ColumnRegexPattern.IsMatch(column.GetRuntimeName());
        }
        return false;
    }

    protected override void MutilateTable(IDataLoadJob job, ITableInfo tableInfo, DiscoveredTable table)
    {
        var timer = Stopwatch.StartNew();
        DataTable redactionsToSaveTable = new();
        DataTable pksToSave = new();
        redactionsToSaveTable.Columns.Add("RedactionConfiguration_ID");
        redactionsToSaveTable.Columns.Add("ColumnInfo_ID");
        redactionsToSaveTable.Columns.Add("startingIndex");
        redactionsToSaveTable.Columns.Add("ReplacementValue");
        redactionsToSaveTable.Columns.Add("RedactedValue");
        pksToSave.Columns.Add("RegexRedaction_ID");
        pksToSave.Columns.Add("ColumnInfo_ID");
        pksToSave.Columns.Add("Value");

        var columns = table.DiscoverColumns();
        
        var relatedCatalogues = tableInfo.GetAllRelatedCatalogues();
        var cataloguePks = relatedCatalogues.SelectMany(c => c.CatalogueItems).Where(ci => ci.ColumnInfo.IsPrimaryKey).ToList();
        _discoveredPKColumns = columns.Where(c => cataloguePks.Select(cpk=>cpk.Name).Contains(c.GetRuntimeName())).ToArray();
        if(_discoveredPKColumns.Length == 0)
        {
            job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning,"No Primary Keys found. Redaction cannot be perfomed without a promary key."));
        }
        var pkColumnInfos = cataloguePks.Select(c => c.ColumnInfo);
        var memoryRepo = new MemoryCatalogueRepository();
        foreach (var column in columns.Where(c => !pkColumnInfos.Select(c => c.GetRuntimeName()).Contains(c.GetRuntimeName())))
        {
            if (ColumnMatches(column))
            {
                var pkSeparator = pkColumnInfos.Count() > 0 ? "," : "";
                var sql = @$"
                    SELECT {column.GetRuntimeName()} {pkSeparator} {string.Join(", ", pkColumnInfos.Select(c => c.GetRuntimeName()))}
                    FROM {table.GetRuntimeName()}
                    WHERE {column.GetRuntimeName()} LIKE '%{redactionConfiguration.RegexPattern}%'
                    ";
                var dt = new DataTable();
                dt.BeginLoadData();
                var conn = table.Database.Server.GetConnection();
                conn.Open();
                using (var cmd = table.Database.Server.GetCommand(sql, conn))
                {
                    using var da = table.Database.Server.GetDataAdapter(cmd);
                    da.Fill(dt);
                }
                dt.EndLoadData();
                var redactionUpates = dt.Clone();
                var columnInfo = relatedCatalogues.SelectMany(c => c.CatalogueItems).ToArray().Select(ci => ci.ColumnInfo).Where(ci => ci.GetRuntimeName() == column.GetRuntimeName()).First();
                foreach (DataRow row in dt.Rows)
                {
                    RegexRedactionHelper.Redact(columnInfo, row, cataloguePks, redactionConfiguration, redactionsToSaveTable, pksToSave, redactionUpates);
                }
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Regex Redactions generated redactions in {timer.ElapsedMilliseconds}ms and found {dt.Rows.Count} redactions."));

                pksToSave.Columns.Add("ID", typeof(int));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    pksToSave.Rows[i]["ID"] = i + 1;
                }
                redactionsToSaveTable.Columns.Add("ID", typeof(int));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    redactionsToSaveTable.Rows[i]["ID"] = i + 1;
                }
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Creating Temporary tables"));
                var t1 = table.Database.CreateTable("pksToSave_Temp", pksToSave);
                var t2 = table.Database.CreateTable("redactionsToSaveTable_Temp", redactionsToSaveTable);
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Saving Redactions"));
                RegexRedactionHelper.SaveRedactions(job.RepositoryLocator.CatalogueRepository, t1, t2);
                t1.Drop();
                t2.Drop();
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Performing join update"));
                var b = timer.ElapsedMilliseconds;
                RegexRedactionHelper.DoJoinUpdate(columnInfo, table, table.Database.Server, redactionUpates, _discoveredPKColumns);
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Regex Redactions tool {timer.ElapsedMilliseconds}ms and found {dt.Rows.Count} redactions."));
            }
        }
    }
}
