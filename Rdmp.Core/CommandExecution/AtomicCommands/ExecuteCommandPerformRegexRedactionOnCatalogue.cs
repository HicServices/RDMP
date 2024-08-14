using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandPerformRegexRedactionOnCatalogue : BasicCommandExecution, IAtomicCommand
{
    private readonly IBasicActivateItems _activator;
    private readonly ICatalogue _catalogue;
    private readonly RegexRedactionConfiguration _redactionConfiguration;
    private readonly List<ColumnInfo> _columns;
    private readonly DiscoveredServer _server;



    //public ExecuteCommandPerformRegexRedactionOnCatalogue(IBasicActivateItems activator, ICatalogue catalogue, RegexRedactionConfiguration redactionConfiguration, List<ColumnInfo> columns) {
    //    _activator = activator;
    //    _catalogue = catalogue;
    //    _redactionConfiguration = redactionConfiguration;
    //    _columns = columns;
    //}

    public ExecuteCommandPerformRegexRedactionOnCatalogue(IBasicActivateItems activator)
    {
        _activator = activator;
        _catalogue = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Catalogue>("Name", "Biochemistry").First();
        _redactionConfiguration = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<RegexRedactionConfiguration>().First();
        _columns = _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo).ToList();
        _server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
    }

    private void Redact(DiscoveredTable table, ColumnInfo column, List<ColumnInfo> PKColumns, DataRow match, DataColumnCollection columns)
    {
        var updateHelper = _server.GetQuerySyntaxHelper().UpdateHelper;
        var typeTranslator = _server.GetQuerySyntaxHelper().TypeTranslater;
        var redactedValue = "TODO";

        var sqlLines = new List<CustomLine>
        {
            new CustomLine($"t1.{column.GetRuntimeName()} = '{redactedValue}'", QueryComponent.SET)
        };
        foreach (var pk in PKColumns)
        {
            //ColumnInfo 
            var matchValue = $"'{match[pk.GetRuntimeName()]}'";
            //doesn;t work for datePKS

            sqlLines.Add(new CustomLine($"t1.{pk.GetRuntimeName()} = {matchValue}", QueryComponent.WHERE));
            sqlLines.Add(new CustomLine(string.Format("t1.{0} = t2.{0}", pk.GetRuntimeName()), QueryComponent.JoinInfoJoin));

        }
        var sql = updateHelper.BuildUpdate(table, table, sqlLines);
        Console.WriteLine(sql);
    }

    public override void Execute()
    {
        base.Execute();
        var memoryRepo = new MemoryCatalogueRepository();
        foreach (var catalogueItem in _catalogue.CatalogueItems.Where(ci => !ci.ColumnInfo.IsPrimaryKey && _columns.Contains(ci.ColumnInfo)))
        {
            var columnName = catalogueItem.ColumnInfo.Name;
            var table = catalogueItem.ColumnInfo.TableInfo.Name;
            DiscoveredDatabase discoveredDb = _server.ExpectDatabase(catalogueItem.ColumnInfo.TableInfo.Database);
            DiscoveredTable discoveredTable = discoveredDb.ExpectTable(catalogueItem.ColumnInfo.TableInfo.GetRuntimeName());
            var pkColumns = _catalogue.CatalogueItems.Select(x => x.ColumnInfo).Where(x => x.IsPrimaryKey).ToList();
            if (pkColumns.Where(pkc => pkc.Name.Contains(table)).Any())
            {
                var qb = new QueryBuilder(null, null, null);
                qb.AddColumn(new ColumnInfoToIColumn(memoryRepo, catalogueItem.ColumnInfo));
                qb.AddColumnRange(pkColumns.Select(pk => new ColumnInfoToIColumn(memoryRepo, pk)).ToArray());
                var sql = qb.SQL;
                var dt = new DataTable();
                dt.BeginLoadData();
                using (var cmd = _server.GetCommand(sql, _server.GetConnection()))
                {
                    using var da = _server.GetDataAdapter(cmd);
                    da.Fill(dt);
                }
                DataTable _regexMatches = new();
                _regexMatches.BeginLoadData();
                _regexMatches = dt.AsEnumerable().Where(row => Regex.IsMatch(row[catalogueItem.ColumnInfo.GetRuntimeName()].ToString(), _redactionConfiguration.RegexPattern)).CopyToDataTable();
                _regexMatches.EndLoadData();
                dt.EndLoadData();

                foreach (DataRow row in _regexMatches.Rows)
                {
                    Redact(discoveredTable, catalogueItem.ColumnInfo, pkColumns, row, _regexMatches.Columns);
                }
            }
            else
            {
                //Unable to find any primary keys
                throw new Exception($"Unable to identify any primary keys in table '{table}'. Redactions cannot be performed on tables without primary keys");
            }
        }
    }
}
