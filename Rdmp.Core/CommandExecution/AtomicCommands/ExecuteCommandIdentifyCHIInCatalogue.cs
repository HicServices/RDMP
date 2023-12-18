// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandIdentifyCHIInCatalogue : BasicCommandExecution, IAtomicCommand
{

    private readonly ICatalogue _catalouge;
    private readonly bool _bailOutEarly;
    private readonly Dictionary<string, List<string>> _allowLists = new();

    private readonly string PotentialCHI = "Potential CHI";
    private readonly string Context = "Context";
    private readonly string SourceColumnName = "Source Column Name";
    private readonly string PKValue = "PK Value";
    private readonly string PKColumn = "PK Column";
    private readonly string ReplacementIndex = "replacementIndex";
    private readonly string RDMP_ALL = "RDMP_ALL";

    public DataTable foundChis = new();


    public ExecuteCommandIdentifyCHIInCatalogue(IBasicActivateItems activator, [DemandsInitialization("The catalogue to search")] ICatalogue catalogue, bool bailOutEarly = false, string allowListLocation = null) : base(activator)
    {
        _catalouge = catalogue;
        _bailOutEarly = bailOutEarly;
        foundChis.Columns.Add(PotentialCHI);
        foundChis.Columns.Add(Context);
        foundChis.Columns.Add(SourceColumnName);
        foundChis.Columns.Add(PKValue);
        foundChis.Columns.Add(PKColumn);
        foundChis.Columns.Add(ReplacementIndex);
        if (!string.IsNullOrWhiteSpace(allowListLocation))
        {
            var allowListFileContent = File.ReadAllText(allowListLocation);
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize<Dictionary<string, List<string>>>(allowListFileContent);
            foreach (var (cat, columns) in yamlObject)
            {
                _allowLists.Add(cat, columns);
            }
        }
    }


    public static string WrapCHIInContext(string chi, string source, int padding = 25)
    {
        var foundIndex = source.IndexOf(chi);
        return $"{source[Math.Max(0, foundIndex - padding)..foundIndex]}{chi}{source[(foundIndex + chi.Length)..Math.Min(foundIndex + chi.Length + padding, source.Length)]}";
    }

    private void HandleFoundCHI(string foundChi, string contextValue, string columnName, string pkValue, string pkColumn)
    {
        if (pkColumn.Split(".").Last().Replace("[", "").Replace("]", "") == columnName.Replace("[", "").Replace("]", "")) return; //don't redact PKs, it gets messy
        var shrunkContext = WrapCHIInContext(foundChi, contextValue);
        foundChis.Rows.Add(foundChi, shrunkContext, columnName, pkValue, pkColumn, contextValue.IndexOf(foundChi));
    }

    public override void Execute()
    {
        base.Execute();
        List<string> columnAllowList = new();
        if (_allowLists.TryGetValue(RDMP_ALL, out var _extractionSpecificAllowances))
            columnAllowList.AddRange(_extractionSpecificAllowances);
        if (_allowLists.TryGetValue(_catalouge.Name, out var _catalogueSpecificAllowances))
            columnAllowList.AddRange(_catalogueSpecificAllowances.ToList());



        foreach (var item in _catalouge.CatalogueItems)
        {
            if (columnAllowList.Contains(item.Name)) continue;

            if (_bailOutEarly && foundChis.Rows.Count > 0)
            {
                break;
            }
            var column = item.ColumnInfo.Name;
            int idxOfLastSplit = column.LastIndexOf('.');
            var columnName = column[(idxOfLastSplit + 1)..];
            var server = _catalouge.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
            var pkColumns = _catalouge.CatalogueItems.Select(x => x.ColumnInfo).Where(x => x.IsPrimaryKey);
            if (pkColumns.Where(pkc => pkc.Name.Contains(columnName)).Any())
            {

                var pkColumn = pkColumns.First().Name.Split(".").Last();
                var sql = $"SELECT {columnName},{pkColumn} from {column[..idxOfLastSplit]}";
                var dt = new DataTable();
                dt.BeginLoadData();
                using (var cmd = server.GetCommand(sql, server.GetConnection()))
                {
                    using var da = server.GetDataAdapter(cmd);
                    da.Fill(dt);
                }
                dt.EndLoadData();
                foreach (DataRow row in dt.Rows)
                {

                    var value = row[dt.Columns[0].ColumnName].ToString();
                    var potentialCHI = CHIColumnFinder.GetPotentialCHI(value);
                    if (!string.IsNullOrWhiteSpace(potentialCHI))
                    {
                        var pkColumnInfo = _catalouge.CatalogueItems.Select(x => x.ColumnInfo).Where(x => x.IsPrimaryKey).First();
                        var pkValue = GetPKValue(pkColumnInfo, row, dt);
                        HandleFoundCHI(potentialCHI, value, item.Name, pkValue, pkColumnInfo.Name);
                        if (_bailOutEarly)
                        {
                            break;
                        }
                    }
                }
                dt.Dispose();
            }
            else
            {
                //todo this should probably warn the UI
                Console.WriteLine("Unable to find a primary key for this catalogue");
            }
        }
        Console.WriteLine($"Found {foundChis.Rows.Count} CHIs in the {_catalouge.Name} Catalogue.");
        foreach (DataRow row in foundChis.Rows)
        {
            Console.WriteLine($"{row[PotentialCHI]} | {row[Context]} | {row[SourceColumnName]}");

        }
    }

    private static string GetPKValue(ColumnInfo pkColumnInfo, DataRow row, DataTable dt)
    {
        //todo doesn't work with multitable catalogues

        if (pkColumnInfo != null)
        {
            var pkName = pkColumnInfo.Name.Split(".").Last().Replace("[", "").Replace("]", "");
            var arrayNames = (from DataColumn x
                              in dt.Columns.Cast<DataColumn>()
                              select x.ColumnName).ToList();
            var index = arrayNames.IndexOf(pkName);
            if (index != -1)
            {
                return row[index].ToString();
            }
        }
        return "Error: Unknown PK";
    }
}
