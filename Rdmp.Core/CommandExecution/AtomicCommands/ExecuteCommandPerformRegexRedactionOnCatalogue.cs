using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Microsoft.Data.SqlClient;
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
    private readonly DataTable redactionsToSaveTable;
    private readonly DataTable pksToSave;
    private readonly int? _readLimit;
    private DataTable redactionUpates = new();

    public ExecuteCommandPerformRegexRedactionOnCatalogue(IBasicActivateItems activator, ICatalogue catalogue, RegexRedactionConfiguration redactionConfiguration, List<ColumnInfo> columns, int? readLimit = null)
    {
        _activator = activator;
        _catalogue = catalogue;
        _redactionConfiguration = redactionConfiguration;
        _columns = columns;
        _server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
        _readLimit = readLimit;
        redactionsToSaveTable = RegexRedactionHelper.GenerateRedactionsDataTable();
        pksToSave = RegexRedactionHelper.GeneratePKDataTable();
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
                if (_readLimit is not null)
                {
                    qb.TopX = (int)_readLimit;
                }
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
                timer.Stop();
                Console.WriteLine(timer.ElapsedMilliseconds);
                timer.Start();
                redactionUpates = dt.Clone();
                redactionUpates.BeginLoadData();
                foreach (DataRow row in dt.Rows)
                {
                    RegexRedactionHelper.Redact(columnInfo, row, _cataloguePKs, _redactionConfiguration, redactionsToSaveTable, pksToSave, redactionUpates);
                }
                redactionUpates.EndLoadData();
                timer.Stop();
                Console.WriteLine(timer.ElapsedMilliseconds);
                timer.Start();
                for (int i = 0; i < pksToSave.Rows.Count; i++)
                {
                    pksToSave.Rows[i]["ID"] = i + 1;
                }
                var t1 = _discoveredTable.Database.CreateTable("pksToSave_Temp", pksToSave);
                var t2 = _discoveredTable.Database.CreateTable("redactionsToSaveTable_Temp", redactionsToSaveTable);
                RegexRedactionHelper.SaveRedactions(_activator.RepositoryLocator.CatalogueRepository, t1, t2, _server);
                t1.Drop();
                t2.Drop();
                timer.Stop();
                Console.WriteLine(timer.ElapsedMilliseconds);
                timer.Start();
                if (dt.Rows.Count > 0)
                {
                    RegexRedactionHelper.DoJoinUpdate(columnInfo, _discoveredTable, _server, redactionUpates, _discoveredPKColumns);
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
