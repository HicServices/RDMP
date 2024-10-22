// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MongoDB.Driver;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    private DataTable redactionsToSaveTable;
    private DataTable pksToSave;
    private readonly int? _readLimit;
    private DataTable redactionUpates = new();
    public int resultCount = 0;

    public ExecuteCommandPerformRegexRedactionOnCatalogue(IBasicActivateItems activator, ICatalogue catalogue, RegexRedactionConfiguration redactionConfiguration, List<ColumnInfo> columns, int? readLimit = null)
    {
        _activator = activator;
        _catalogue = catalogue;
        _redactionConfiguration = redactionConfiguration;
        _columns = columns;
        _server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
        _readLimit = readLimit;
        if (_server.DatabaseType != FAnsi.DatabaseType.MicrosoftSQLServer)
        {
            //This should be implimented for all db types after UAT for the initial purpose
            SetImpossible("Regex Redaction are currently only supported on MSSQL databases");
        }
    }

    public override void Execute()
    {
        base.Execute();
        var memoryRepo = new MemoryCatalogueRepository();

        foreach (var columnInfo in _columns.Where(c => !c.IsPrimaryKey))
        {
            redactionsToSaveTable = RegexRedactionHelper.GenerateRedactionsDataTable();
            pksToSave = RegexRedactionHelper.GeneratePKDataTable();

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
                    cmd.CommandTimeout = 60000;
                    using var da = _server.GetDataAdapter(cmd);
                    da.Fill(dt);
                }
                conn.Close();
                
                redactionUpates = dt.Clone();
                redactionUpates.BeginLoadData();
                foreach (DataRow row in dt.Rows)
                {
                    RegexRedactionHelper.Redact(columnInfo, row, _cataloguePKs, _redactionConfiguration, redactionsToSaveTable, pksToSave, redactionUpates);
                }
                redactionUpates.EndLoadData();
                if(pksToSave.Rows.Count == 0)
                {
                    _activator.Show("Unable to find any matching Redactions");
                    return;
                }
                for (int i = 0; i < pksToSave.Rows.Count; i++)
                {
                    pksToSave.Rows[i][nameof(RegexRedactionHelper.Constants.ID)] = i + 1;
                }
                var existing = _discoveredTable.Database.ExpectTable(nameof(RegexRedactionHelper.Constants.pksToSave_Temp));
                if (existing.Exists())
                {
                    existing.Drop();
                }
                var t1 = _discoveredTable.Database.CreateTable(nameof(RegexRedactionHelper.Constants.pksToSave_Temp), pksToSave);
                existing = _discoveredTable.Database.ExpectTable(nameof(RegexRedactionHelper.Constants.redactionsToSaveTable_Temp));
                if (existing.Exists())
                {
                    existing.Drop();
                }
                var t2 = _discoveredTable.Database.CreateTable(nameof(RegexRedactionHelper.Constants.redactionsToSaveTable_Temp), redactionsToSaveTable);

                RegexRedactionHelper.SaveRedactions(_activator.RepositoryLocator.CatalogueRepository, t1, t2, _server);
                t1.Drop();
                t2.Drop();
                if (dt.Rows.Count > 0)
                {
                    RegexRedactionHelper.DoJoinUpdate(columnInfo, _discoveredTable, _server, redactionUpates, _discoveredPKColumns);
                }
            }
            else
            {
                throw new Exception($"Unable to identify any primary keys in table '{table}'. Redactions cannot be performed on tables without primary keys");
            }
            resultCount += redactionUpates.Rows.Count;
        }
    }
}
