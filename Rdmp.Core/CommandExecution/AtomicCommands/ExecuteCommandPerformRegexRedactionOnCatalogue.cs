﻿using CommandLine;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MongoDB.Driver;
using NPOI.OpenXmlFormats;
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
using System.Threading.Tasks;
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
    private DbConnection _conn;
    private DataTable redactionsToSaveTable = new();


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
    }


    private string GetRedactionValue(string value, ColumnInfo column, Dictionary<ColumnInfo, string> pkLookup)
    {
        var matches = Regex.Matches(value, _redactionConfiguration.RegexPattern);
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
            //var redaction = new RegexRedaction(_activator.RepositoryLocator.CatalogueRepository, _redactionConfiguration.ID, startingIndex, foundMatch, replacementValue, column.ID, pkLookup);
            //redaction.SaveToDatabase();
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
        _conn = _server.GetConnection();
        _conn.Open();
        using (var cmd = _server.GetCommand(sql, _conn))
        {
            cmd.ExecuteNonQuery();
        }
        redactionTable.Drop();
    }

    private void Redact(DiscoveredTable table, ColumnInfo column, DataRow match)
    {
        int columnIndex = 0;// -1;
        Dictionary<ColumnInfo, string> pkLookup = Enumerable.Range(0, _cataloguePKs.Count).ToDictionary(i => _cataloguePKs[i].ColumnInfo, i => match[i + 1].ToString());
        var redactedValue = GetRedactionValue(match[columnIndex].ToString(), column, pkLookup);
        match[columnIndex] = redactedValue;
        redactionUpates.ImportRow(match);
    }



    private void SaveRedactions()
    {
        if (redactionsToSaveTable.Rows.Count > 1000)
        {
            var read = 0;
            while(read < redactionsToSaveTable.Rows.Count)
            {
                var rows = redactionsToSaveTable.AsEnumerable().Skip(read).Take(1000);
                var insertValue = string.Join(
               @",
            ", rows.Select(r => $"({r[0]},{r[1]},{r[2]},'{r[3]}','{r[4]}')"));
                var sql = @$"
            INSERT INTO RegexRedaction(RedactionConfiguration_ID,ColumnInfo_ID,startingIndex,ReplacementValue,RedactedValue)
            VALUES
            {insertValue}
        ";
                (_activator.RepositoryLocator.CatalogueRepository as TableRepository).Insert(
                    sql, new Dictionary<string, object>()
                    );
                read = read + 1000;
            }
            //dt drop 
        }
        else
        {

            var insertValue = string.Join(
                @",
            ", redactionsToSaveTable.AsEnumerable().Select(r => $"({r[0]},{r[1]},{r[2]},'{r[3]}','{r[4]}')"));
            var sql = @$"
            INSERT INTO RegexRedaction(RedactionConfiguration_ID,ColumnInfo_ID,startingIndex,ReplacementValue,RedactedValue)
            VALUES
            {insertValue}
        ";
            (_activator.RepositoryLocator.CatalogueRepository as TableRepository).Insert(
                sql, new Dictionary<string, object>()
                );
        }

    }

    public override void Execute()
    {
        base.Execute();
        var memoryRepo = new MemoryCatalogueRepository();
        _conn = _server.GetConnection();
        _conn.Open();
        var timer = Stopwatch.StartNew();
        foreach (var columnInfo in _columns.Where(c => !c.IsPrimaryKey))
        {

            var columnName = columnInfo.Name;
            var table = columnInfo.TableInfo.Name;
            _discoveredTable = columnInfo.TableInfo.Discover(DataAccessContext.InternalDataProcessing);
            DiscoveredColumn[] discoveredColumns = _discoveredTable.DiscoverColumns();
            _discoveredPKColumns = discoveredColumns.Where(c => c.IsPrimaryKey).ToArray();
            if (_discoveredPKColumns.Any())
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

                using (var cmd = _server.GetCommand(sql, _conn))
                {
                    using var da = _server.GetDataAdapter(cmd);
                    da.Fill(dt);
                }
                timer.Stop();
                var y = timer.ElapsedMilliseconds;
                timer.Start();

                redactionUpates = dt.Clone();
                redactionUpates.BeginLoadData();
                foreach (DataRow row in dt.Rows)
                {
                    Redact(_discoveredTable, columnInfo, row);
                }
                redactionUpates.EndLoadData();
                SaveRedactions();
                timer.Stop();
                Console.WriteLine(timer.ElapsedMilliseconds);
                timer.Start();
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
                //Unable to find any primary keys
                throw new Exception($"Unable to identify any primary keys in table '{table}'. Redactions cannot be performed on tables without primary keys");
            }
        }
        timer.Stop();
        var x = timer.ElapsedMilliseconds;
    }
}