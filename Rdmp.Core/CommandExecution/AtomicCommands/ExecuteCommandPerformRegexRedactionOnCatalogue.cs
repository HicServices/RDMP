using CommandLine;
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
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
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
    private List<CatalogueItem> _cataloguePKs;


    public ExecuteCommandPerformRegexRedactionOnCatalogue(IBasicActivateItems activator, ICatalogue catalogue, RegexRedactionConfiguration redactionConfiguration, List<ColumnInfo> columns)
    {
        _activator = activator;
        _catalogue = catalogue;
        _redactionConfiguration = redactionConfiguration;
        _columns = columns;
        _server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);

    }

    private string GetRedactionValue(string value, ColumnInfo column, Dictionary<ColumnInfo, string> pkLookup)
    {
        var matches = Regex.Matches(value, _redactionConfiguration.RegexPattern);
        foreach (var match in matches)
        {
            var foundMatch = match.ToString();
            var startingIndex = value.IndexOf(foundMatch);
            string replacementValue = _redactionConfiguration.RedactionString;

            int lengthDiff = foundMatch.Length - replacementValue.Length;
            if (lengthDiff < 1)
            {
                throw new Exception($"Redaction string '{_redactionConfiguration.RedactionString}' is longer than found match '{foundMatch}'.");
            }
            if (lengthDiff > 0)
            {
                var start = (int)Math.Floor((float)(lengthDiff / 2));
                var end = (int)Math.Ceiling((float)(lengthDiff / 2));
                replacementValue = replacementValue.PadLeft(start, '<').PadRight(end, '>');
            }
            value = value[..startingIndex] + replacementValue + value[(startingIndex + foundMatch.Length)..];

            var redaction = new RegexRedaction(_activator.RepositoryLocator.CatalogueRepository, _redactionConfiguration.ID, startingIndex, foundMatch, replacementValue, column.ID, pkLookup);
            redaction.SaveToDatabase();
        }

        return value;
    }

    private void Redact(DiscoveredTable table, ColumnInfo column, DataRow match, DataColumnCollection columns)
    {
        var updateHelper = _server.GetQuerySyntaxHelper().UpdateHelper;
        int columnIndex = -1;
        var dcColumnIndexes = columns.Cast<DataColumn>().Select((item, index) =>
        {
            if (item.ColumnName == column.GetRuntimeName())
            {
                columnIndex = index;
            }
            return index;
        });
        var columnNames = columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        Dictionary<ColumnInfo, string> pkLookup = new();
        foreach (int i in dcColumnIndexes.Where(n => n != columnIndex))
        {
            //these are all the PK values
            var name = columnNames[i];
            var pkCatalogueItem = _cataloguePKs.Where(c => c.Name == name).First();
            pkLookup.Add(pkCatalogueItem.ColumnInfo, match[i].ToString());
        }

        var redactedValue = GetRedactionValue(match[columnIndex].ToString(), column, pkLookup);

        var sqlLines = new List<CustomLine>
        {
            new CustomLine($"t1.{column.GetRuntimeName()} = '{redactedValue}'", QueryComponent.SET)
        };
        foreach (var pk in _discoveredPKColumns)
        {
            var matchValue = RegexRedactionHelper.ConvertPotentialDateTimeObject(match[pk.GetRuntimeName()].ToString(), pk.DataType.SQLType);

            sqlLines.Add(new CustomLine($"t1.{pk.GetRuntimeName()} = {matchValue}", QueryComponent.WHERE));
            sqlLines.Add(new CustomLine(string.Format("t1.{0} = t2.{0}", pk.GetRuntimeName()), QueryComponent.JoinInfoJoin));

        }
        var sql = updateHelper.BuildUpdate(table, table, sqlLines);
        var conn = _server.GetConnection();
        using (var cmd = _server.GetCommand(sql, conn))
        {
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public override void Execute()
    {
        base.Execute();
        var memoryRepo = new MemoryCatalogueRepository();
        foreach (var catalogueItem in _catalogue.CatalogueItems.Where(ci => !ci.ColumnInfo.IsPrimaryKey && _columns.Contains(ci.ColumnInfo)))
        {
            var columnName = catalogueItem.ColumnInfo.Name;
            var table = catalogueItem.ColumnInfo.TableInfo.Name;
            DiscoveredTable discoveredTable = catalogueItem.ColumnInfo.TableInfo.Discover(DataAccessContext.InternalDataProcessing);
            DiscoveredColumn[] discoveredColumns = discoveredTable.DiscoverColumns();
            _discoveredPKColumns = discoveredColumns.Where(c => c.IsPrimaryKey).ToArray();
            if (_discoveredPKColumns.Any())
            {
                _cataloguePKs = _catalogue.CatalogueItems.Where(c => c.ColumnInfo.IsPrimaryKey).ToList();
                var qb = new QueryBuilder(null, null, null);
                qb.AddColumn(new ColumnInfoToIColumn(memoryRepo, catalogueItem.ColumnInfo));
                foreach (var pk in _discoveredPKColumns)
                {
                    var cataloguePk = _cataloguePKs.FirstOrDefault(c => c.ColumnInfo.GetRuntimeName() == pk.GetRuntimeName());
                    qb.AddColumn(new ColumnInfoToIColumn(memoryRepo, cataloguePk.ColumnInfo));
                }
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
                    Redact(discoveredTable, catalogueItem.ColumnInfo, row, _regexMatches.Columns);
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
