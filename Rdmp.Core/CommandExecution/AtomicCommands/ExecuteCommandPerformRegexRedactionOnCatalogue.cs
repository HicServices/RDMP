using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MongoDB.Driver;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Terminal.Gui;
using static System.Linq.Enumerable;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandPerformRegexRedactionOnCatalogue : BasicCommandExecution, IAtomicCommand
{
    private readonly IBasicActivateItems _activator;
    private readonly ICatalogue _catalogue;
    private readonly RegexRedactionConfiguration _redactionConfiguration;
    private readonly List<ColumnInfo> _columns;
    private readonly DiscoveredServer _server;
    private DiscoveredColumn[] _discoveredPKColumns;
    private DiscoveredTable _discoveredTable;
    private List<CatalogueItem> _cataloguePKs;
    private readonly DataTable redactionsToSaveTable = new();
    private readonly DataTable pksToSave = new();

    private DataTable redactionUpates = new();

    public ExecuteCommandPerformRegexRedactionOnCatalogue(IBasicActivateItems activator, ICatalogue catalogue, RegexRedactionConfiguration redactionConfiguration, List<ColumnInfo> columns)
    {
        _activator = activator;
        _catalogue = catalogue;
        _redactionConfiguration = redactionConfiguration;
        _columns = columns;
        _server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
        redactionsToSaveTable.Columns.Add("RedactionConfiguration_ID");
        redactionsToSaveTable.Columns.Add("ColumnInfo_ID");
        redactionsToSaveTable.Columns.Add("startingIndex");
        redactionsToSaveTable.Columns.Add("ReplacementValue");
        redactionsToSaveTable.Columns.Add("RedactedValue");
        pksToSave.Columns.Add("redactionsToSaveTable_Index");
        pksToSave.Columns.Add("ColumnInfo_ID");
        pksToSave.Columns.Add("Value");
    }


    private string GetRedactionValue(string value, ColumnInfo column, DataRow m)
    {
        Dictionary<ColumnInfo, string> pkLookup = Enumerable.Range(0, _cataloguePKs.Count).ToDictionary(i => _cataloguePKs[i].ColumnInfo, i => m[i + 1].ToString());
        var matches = Regex.Matches(value, _redactionConfiguration.RegexPattern);
        var offset = 0;
        foreach (var match in matches)
        {
            var foundMatch = match.ToString();
            var startingIndex = value.IndexOf(foundMatch);
            string replacementValue = _redactionConfiguration.RedactionString;

            var lengthDiff = (float)foundMatch.Length - replacementValue.Length;
            if (lengthDiff < 1)
            {
                throw new Exception($"Redaction string '{_redactionConfiguration.RedactionString}' is longer than found match '{foundMatch}'.");
            }
            if (lengthDiff > 0)
            {
                var start = (int)Math.Floor(lengthDiff / 2);
                var end = (int)Math.Ceiling(lengthDiff / 2);
                replacementValue = replacementValue.PadLeft(start + replacementValue.Length, '<');
                replacementValue = replacementValue.PadRight(end + replacementValue.Length, '>');
            }
            value = value[..startingIndex] + replacementValue + value[(startingIndex + foundMatch.Length)..];
            redactionsToSaveTable.Rows.Add([_redactionConfiguration.ID, column.ID, startingIndex, replacementValue, foundMatch]);
            foreach (var pk in pkLookup)
            {
                pksToSave.Rows.Add([redactionUpates.Rows.Count + offset, pk.Key.ID, pk.Value]);
            }
            offset++;
        }

        return value;
    }

    private void DoJoinUpdate(ColumnInfo column)
    {
        var redactionTable = _discoveredTable.Database.CreateTable("TEMP_RedactionUpdates", redactionUpates);
        var updateHelper = _server.GetQuerySyntaxHelper().UpdateHelper;

        var sqlLines = new List<CustomLine>
        {
            new CustomLine($"t1.{column.GetRuntimeName()} = t2.{column.GetRuntimeName()}", QueryComponent.SET)
        };
        foreach (var pk in _discoveredPKColumns)
        {
            sqlLines.Add(new CustomLine($"t1.{pk.GetRuntimeName()} = t2.{pk.GetRuntimeName()}", QueryComponent.WHERE));
            sqlLines.Add(new CustomLine(string.Format("t1.{0} = t2.{0}", pk.GetRuntimeName()), QueryComponent.JoinInfoJoin));

        }
        var sql = updateHelper.BuildUpdate(_discoveredTable, redactionTable, sqlLines);
        var conn = _server.GetConnection();
        conn.Open();
        using (var cmd = _server.GetCommand(sql, conn))
        {
            cmd.ExecuteNonQuery();
        }
        conn.Close();
        redactionTable.Drop();
    }

    private void Redact(ColumnInfo column, DataRow match)
    {

        var redactedValue = GetRedactionValue(match[0].ToString(), column, match);
        match[0] = redactedValue;
        redactionUpates.ImportRow(match);
    }

    private void DoInsertForRedactionKey(IEnumerable<DataRow> pks, DataTable ids)
    {

        var x = ids.Rows.Count;
        var y = pks.Count();
        var pkValues = string.Join(
           @",
            ", pks.Select(r => $"({ids.Rows[int.Parse(r[0].ToString())][0]},{r[1]},'{r[2]}')"));
        var pksql = $@"
              INSERT INTO RegexRedactionKey(RegexRedaction_ID,ColumnInfo_ID,Value)
              VALUES
              {pkValues}
            ";
        (_activator.RepositoryLocator.CatalogueRepository as TableRepository).Insert(pksql, null);
    }


    private void DoInsertForRedactions(IEnumerable<DataRow> rows, DataTable dt)
    {
        var insertValue = string.Join(
               @",
            ", rows.Select(r => $"({r[0]},{r[1]},{r[2]},'{r[3]}','{r[4]}')"));
        var sql = @$"
            DECLARE @output TABLE (id int)
            INSERT INTO RegexRedaction(RedactionConfiguration_ID,ColumnInfo_ID,startingIndex,ReplacementValue,RedactedValue) OUTPUT inserted.ID INTO @output
            VALUES
            {insertValue};
            SELECT * from @output
        ";
        var conn = (_activator.RepositoryLocator.CatalogueRepository as TableRepository).GetConnection();
        using (var cmd = _server.GetCommand(sql, conn.Connection))
        {
            using var da = _server.GetDataAdapter(cmd);
            da.Fill(dt);
        }
        conn.Connection.Close();
    }


    private void SaveRedactions()
    {
        DataTable dt = new();
        if (redactionsToSaveTable.Rows.Count > 1000)
        {
            var read = 0;
            while (read < redactionsToSaveTable.Rows.Count)
            {
                var rows = redactionsToSaveTable.AsEnumerable().Skip(read).Take(1000);
                var pkRows = pksToSave.AsEnumerable().Where(r => int.Parse(r[0].ToString()) >= read && int.Parse(r[0].ToString()) < (read + 1000));
                DoInsertForRedactions(rows, dt);
                read += 1000;
            }
        }
        else
        {
            DoInsertForRedactions(redactionsToSaveTable.AsEnumerable(), dt);
        }
        if (pksToSave.Rows.Count > 1000)
        {
            var read = 0;
            while (read < pksToSave.Rows.Count)
            {
                DoInsertForRedactionKey(pksToSave.AsEnumerable().Skip(read).Take(1000), dt);
                read += 1000;
            }
        }
        else
        {
            var x = dt.Rows.Count;
            var y = pksToSave.Rows.Count;
            DoInsertForRedactionKey(pksToSave.AsEnumerable(), dt);
        }

    }

    public override void Execute()
    {
        base.Execute();
        var memoryRepo = new MemoryCatalogueRepository();

        var timer = Stopwatch.StartNew();
        foreach (var columnInfo in _columns.Where(c => !c.IsPrimaryKey))
        {

            var columnName = columnInfo.Name;
            var table = columnInfo.TableInfo.Name;
            _discoveredTable = columnInfo.TableInfo.Discover(DataAccessContext.InternalDataProcessing);
            DiscoveredColumn[] discoveredColumns = _discoveredTable.DiscoverColumns();
            _discoveredPKColumns = discoveredColumns.Where(c => c.IsPrimaryKey).ToArray();
            if (_discoveredPKColumns.Length != 0)
            {
                _cataloguePKs = _catalogue.CatalogueItems.Where(c => c.ColumnInfo.IsPrimaryKey).ToList();
                var qb = new QueryBuilder(null, null, null);
                qb.AddColumn(new ColumnInfoToIColumn(memoryRepo, columnInfo));
                foreach (var pk in _discoveredPKColumns)
                {
                    var cataloguePk = _cataloguePKs.FirstOrDefault(c => c.ColumnInfo.GetRuntimeName() == pk.GetRuntimeName());
                    qb.AddColumn(new ColumnInfoToIColumn(memoryRepo, cataloguePk.ColumnInfo));
                }
                qb.AddCustomLine($"{columnInfo.GetRuntimeName()} LIKE '%{_redactionConfiguration.RegexPattern}%'", QueryComponent.WHERE);
                var sql = qb.SQL;
                var dt = new DataTable();
                dt.BeginLoadData();
                var conn = _server.GetConnection();
                conn.Open();
                using (var cmd = _server.GetCommand(sql, conn))
                {
                    using var da = _server.GetDataAdapter(cmd);
                    da.Fill(dt);
                }
                conn.Close();

                redactionUpates = dt.Clone();
                redactionUpates.BeginLoadData();
                foreach (DataRow row in dt.Rows)
                {
                    Redact(columnInfo, row);
                }
                redactionUpates.EndLoadData();
                SaveRedactions();
                if (dt.Rows.Count > 0)
                {
                    DoJoinUpdate(columnInfo);
                }
                timer.Stop();
                Console.WriteLine(timer.ElapsedMilliseconds);
                timer.Start();

            }
            else
            {
                throw new Exception($"Unable to identify any primary keys in table '{table}'. Redactions cannot be performed on tables without primary keys");
            }
        }
        timer.Stop();
        var x = timer.ElapsedMilliseconds;
    }
}
