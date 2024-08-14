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

    //public ExecuteCommandIdentifyRegexRedactionsInCatalogue(IBasicActivateItems activator, ICatalogue catalogue, RegexRedactionConfiguration redactionConfiguration, List<ColumnInfo> columns=null)
    //{
    //    _activator = activator;
    //    _catalogue = catalogue;
    //    _redactionConfiguration = redactionConfiguration;
    //    _columns = columns;
    //}


    public ExecuteCommandIdentifyRegexRedactionsInCatalogue(IBasicActivateItems activator)
    {
        _activator = activator;
        _catalogue = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Catalogue>("Name", "Biochemistry").First();
        _redactionConfiguration = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<RegexRedactionConfiguration>().First();
        _columns = _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo).ToList();

    }

    public override void Execute()
    {
        base.Execute();
        var memoryRepo = new MemoryCatalogueRepository();
        foreach (var catalogueItem in _catalogue.CatalogueItems.Where(ci => !ci.ColumnInfo.IsPrimaryKey && _columns.Contains(ci.ColumnInfo)))
        {
            var columnName = catalogueItem.ColumnInfo.Name;
            var table = catalogueItem.ColumnInfo.TableInfo.Name;
            var server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
            var pkColumns = _catalogue.CatalogueItems.Select(x => x.ColumnInfo).Where(x => x.IsPrimaryKey);
            if (pkColumns.Where(pkc => pkc.Name.Contains(table)).Any())
            {
                var qb = new QueryBuilder(null, null, null);
                qb.AddColumn(new ColumnInfoToIColumn(memoryRepo, catalogueItem.ColumnInfo));
                qb.AddColumnRange(pkColumns.Select(pk => new ColumnInfoToIColumn(memoryRepo, pk)).ToArray());
                var sql = qb.SQL;
                var dt = new DataTable();
                dt.BeginLoadData();
                using (var cmd = server.GetCommand(sql, server.GetConnection()))
                {
                    using var da = server.GetDataAdapter(cmd);
                    da.Fill(dt);
                }
                DataTable _regexMatches = new();
                _regexMatches.BeginLoadData();
                _regexMatches = dt.AsEnumerable().Where(row => Regex.IsMatch(row[catalogueItem.ColumnInfo.GetRuntimeName()].ToString(), _redactionConfiguration.RegexPattern)).CopyToDataTable();
                _regexMatches.EndLoadData();
                dt.EndLoadData();
            }
            else
            {
                //Unable to find any primary keys
                throw new Exception($"Unable to identify any primary keys in table '{table}'. Redactions cannot be performed on tables without primary keys");
            }
        }
    }
}
