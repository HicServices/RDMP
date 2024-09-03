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

    private void Redact(IDataLoadJob job, DiscoveredTable table, DataRow row, DiscoveredColumn[] columns, int index)
    {
        var currentValue = row[index].ToString();
        var matches = Regex.Matches(currentValue, redactionConfiguration.RegexPattern);
        var catalogues = job.LoadMetadata.GetAllCatalogues();
        foreach (var match in matches)
        {
            var foundMatch = match.ToString();
            var startingIndex = currentValue.IndexOf(foundMatch);
            string replacementValue = redactionConfiguration.RedactionString;
            var lengthDiff = (float)foundMatch.Length - replacementValue.Length;
            if (lengthDiff < 1)
            {
                throw new Exception($"Redaction string '{redactionConfiguration.RedactionString}' is longer than found match '{foundMatch}'.");
            }
            if (lengthDiff > 0)
            {
                var start = (int)Math.Floor(lengthDiff / 2);
                var end = (int)Math.Ceiling(lengthDiff / 2);
                replacementValue = replacementValue.PadLeft(start + replacementValue.Length, '<');
                replacementValue = replacementValue.PadRight(end + replacementValue.Length, '>');
            }
            currentValue = currentValue[..startingIndex] + replacementValue + currentValue[(startingIndex + foundMatch.Length)..];
        }
        var sqlLines = new List<CustomLine>
        {
            new CustomLine($"t1.{columns[index].GetRuntimeName()} = '{currentValue}'", QueryComponent.SET)
        };
        foreach (var pk in _discoveredPKColumns)
        {
            var matchValue = RegexRedactionHelper.ConvertPotentialDateTimeObject(row[pk.GetRuntimeName()].ToString(), pk.DataType.SQLType);

            sqlLines.Add(new CustomLine($"t1.{pk.GetRuntimeName()} = {matchValue}", QueryComponent.WHERE));
            sqlLines.Add(new CustomLine(string.Format("t1.{0} = t2.{0}", pk.GetRuntimeName()), QueryComponent.JoinInfoJoin));

        }
        var _server = table.Database.Server;
        var updateHelper = _server.GetQuerySyntaxHelper().UpdateHelper;
        var sql = updateHelper.BuildUpdate(table, table, sqlLines);
        var conn = _server.GetConnection();
        using (var cmd = _server.GetCommand(sql, conn))
        {
            cmd.ExecuteNonQuery();
        }


    }

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
        DataTable redactionsToSaveTable = new();
        DataTable pksToSave = new();
        redactionsToSaveTable.Columns.Add("RedactionConfiguration_ID");
        redactionsToSaveTable.Columns.Add("ColumnInfo_ID");
        redactionsToSaveTable.Columns.Add("startingIndex");
        redactionsToSaveTable.Columns.Add("ReplacementValue");
        redactionsToSaveTable.Columns.Add("RedactedValue");
        pksToSave.Columns.Add("redactionsToSaveTable_Index");
        pksToSave.Columns.Add("ColumnInfo_ID");
        pksToSave.Columns.Add("Value");

        var columns = table.DiscoverColumns();
        
        var relatedCatalogues = tableInfo.GetAllRelatedCatalogues();
        var cataloguePks = relatedCatalogues.SelectMany(c => c.CatalogueItems).Where(ci => ci.ColumnInfo.IsPrimaryKey).ToList();
        _discoveredPKColumns = columns.Where(c => cataloguePks.Select(cpk=>cpk.Name).Contains(c.GetRuntimeName())).ToArray(); //will have to figure out the pks
        //TODO should error/fail if there are no promary keys
        //do we tidy up if it is a replacement?
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
                RegexRedactionHelper.SaveRedactions(new ThrowImmediatelyActivator(job.RepositoryLocator), pksToSave, redactionsToSaveTable);
                RegexRedactionHelper.DoJoinUpdate(columnInfo, table, table.Database.Server, redactionUpates, _discoveredPKColumns);

            }
        }
        //if (!columns.Any(c => c.IsPrimaryKey))
        //{
        //    throw new Exception($"Table '{tableInfo}' has no IsPrimaryKey columns");
        //}
        //_discoveredPKColumns = columns.Where(c => c.IsPrimaryKey).ToArray();
        //var dt = table.GetDataTable();
        //var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        //foreach (DataRow row in dt.AsEnumerable())
        //{
        //    foreach (var column in columns.Select((value, index) => new { value, index }).Where(c => !c.value.IsPrimaryKey))
        //    {
        //        if (Regex.IsMatch(row[column.index].ToString(), redactionConfiguration.RegexPattern))
        //        {
        //            Redact(job, table, row, columns, column.index);
        //        }
        //    }
        //}
    }
}
