﻿using CommandLine;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
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
    private DiscoveredColumn[] _discoveredPKColumns;


    public ExecuteCommandPerformRegexRedactionOnCatalogue(IBasicActivateItems activator, ICatalogue catalogue, RegexRedactionConfiguration redactionConfiguration, List<ColumnInfo> columns)
    {
        _activator = activator;
        _catalogue = catalogue;
        _redactionConfiguration = redactionConfiguration;
        _columns = columns;
        _server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);

    }

    //public ExecuteCommandPerformRegexRedactionOnCatalogue(IBasicActivateItems activator)
    //{
    //    _activator = activator;
    //    _catalogue = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Catalogue>("Name", "Biochemistry").First();
    //    _redactionConfiguration = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<RegexRedactionConfiguration>().First();
    //    _columns = _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo).Where(c => c.Name == "[RDMP_ExampleData].[dbo].[Biochemistry].[SampleType]").ToList();
    //    _server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
    //}

    private string GetRedactionValue(string value)
    {
        var matches = Regex.Matches(value, _redactionConfiguration.RegexPattern);
        foreach (var match in matches)
        {
            var foundMatch = match.ToString();
            var startingIndex = value.IndexOf(foundMatch);
            string replacementValue = _redactionConfiguration.RedactionString;

            if (foundMatch.Length < _redactionConfiguration.RedactionString.Length)
            {
                throw new Exception($"Redaction string '{_redactionConfiguration.RedactionString}' is longer than found match '{foundMatch}'.");
            }
            var paddingValue = '>';
            while (foundMatch.Length > replacementValue.Length)
            {
                if (paddingValue == '>')
                {
                    replacementValue += paddingValue;
                }
                else
                {
                    replacementValue = paddingValue + replacementValue;
                }
                paddingValue = paddingValue == '>' ? '<' : '>';
            }
            value = value.Substring(0, startingIndex) + replacementValue + value.Substring(startingIndex + foundMatch.Length);

            //create redaction
            var redaction = new RegexRedaction(_activator.RepositoryLocator.CatalogueRepository, _redactionConfiguration.ID, startingIndex, foundMatch, replacementValue);
            redaction.SaveToDatabase();
        }

        return value;
    }

    private void Redact(DiscoveredTable table, ColumnInfo column, DataRow match, DataColumnCollection columns)
    {
        var updateHelper = _server.GetQuerySyntaxHelper().UpdateHelper;
        var index = columns.Cast<DataColumn>().Select(dc => dc.ColumnName).ToList().IndexOf(column.GetRuntimeName());
        var redactedValue = GetRedactionValue(match[index].ToString());

        var sqlLines = new List<CustomLine>
        {
            new CustomLine($"t1.{column.GetRuntimeName()} = '{redactedValue}'", QueryComponent.SET)
        };
        foreach (var pk in _discoveredPKColumns)
        {
            var matchValue = $"'{match[pk.GetRuntimeName()]}'";
            if (pk.DataType.SQLType == "datetime2" || pk.DataType.SQLType == "datetime" )
            {
                var x = DateTime.Parse(match[pk.GetRuntimeName()].ToString());
                var format = "yyyy-MM-dd HH:mm:ss:fff";
                matchValue = $"'{x.ToString(format)}'";
            }

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
                var cataloguePKs = _catalogue.CatalogueItems.Where(c => c.ColumnInfo.IsPrimaryKey);
                var qb = new QueryBuilder(null, null, null);
                qb.AddColumn(new ColumnInfoToIColumn(memoryRepo, catalogueItem.ColumnInfo));
                foreach (var pk in _discoveredPKColumns)
                {
                    var cataloguePk = cataloguePKs.FirstOrDefault(c => c.ColumnInfo.GetRuntimeName() == pk.GetRuntimeName());
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