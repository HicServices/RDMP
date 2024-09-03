using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MongoDB.Driver.Core.Servers;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandIdentifyRegexRedactionsInCatalogue : BasicCommandExecution, IAtomicCommand
{
    private readonly IBasicActivateItems _activator;
    private readonly ICatalogue _catalogue;
    private readonly RegexRedactionConfiguration _redactionConfiguration;
    private readonly List<ColumnInfo> _columns;
    private readonly int? _readLimit;
    public DataTable results;

    public ExecuteCommandIdentifyRegexRedactionsInCatalogue(IBasicActivateItems activator, ICatalogue catalogue, RegexRedactionConfiguration redactionConfiguration, List<ColumnInfo> columns = null, int? readLimit = null)
    {
        _activator = activator;
        _catalogue = catalogue;
        _redactionConfiguration = redactionConfiguration;
        _columns = columns;
        _readLimit = readLimit;
    }

    public override void Execute()
    {
        base.Execute();
        var memoryRepo = new MemoryCatalogueRepository();
        foreach (var columnInfo in _columns.Where(c => !c.IsPrimaryKey))
        {
            var server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
            var columnName = columnInfo.Name;
            var table = columnInfo.TableInfo.Name;
            var _discoveredTable = columnInfo.TableInfo.Discover(DataAccessContext.InternalDataProcessing);
            DiscoveredColumn[] discoveredColumns = _discoveredTable.DiscoverColumns();
            var _discoveredPKColumns = discoveredColumns.Where(c => c.IsPrimaryKey).ToArray();
            if (_discoveredPKColumns.Length != 0)
            {
                var _cataloguePKs = _catalogue.CatalogueItems.Where(c => c.ColumnInfo.IsPrimaryKey).ToList();
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
                using (var cmd = server.GetCommand(sql, server.GetConnection()))
                {
                    using var da = server.GetDataAdapter(cmd);
                    da.Fill(dt);
                }
                dt.Columns.Add("Proposed Redaction");
                foreach(DataRow row in dt.Rows)
                {
                    row["Proposed Redaction"] = "TODO";
                }
                results = dt;
            }
            else
            {
                //Unable to find any primary keys
                throw new Exception($"Unable to identify any primary keys in table '{table}'. Redactions cannot be performed on tables without primary keys");
            }
        }
    }
}
