// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;
using Org.BouncyCastle.Asn1.Crmf;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataViewing;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandIdentifyCHIInCatalogue : BasicCommandExecution, IAtomicCommand
{

    private ICatalogue _catalouge;
    private IBasicActivateItems _activator;
    private bool _bailOutEarly;
    private readonly Dictionary<string, List<string>> _allowLists = new();

    //TODO don;t show PKs
    public ExecuteCommandIdentifyCHIInCatalogue(IBasicActivateItems activator, [DemandsInitialization("The catalogue to search")] ICatalogue catalogue, bool bailOutEarly = false, string allowListLocation = null) : base(activator)
    {
        _catalouge = catalogue;
        _activator = activator;
        _bailOutEarly = bailOutEarly;
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



    private void handleFoundCHI(string foundChi, string contextValue, string columnName, string pkValue, string pkColumn)
    {
        if (pkColumn.Split(".").Last().Replace("[", "").Replace("]", "") == columnName.Replace("[", "").Replace("]", "")) return; //don't redact PKs, it gets messy
        //^TODO check this works and doesn;t need to munge the []^
        if (foundChis.Rows.Count == 0)
        {
            //init
            foundChis.Columns.Add("Potential CHI");
            foundChis.Columns.Add("Context");
            foundChis.Columns.Add("Source Column Name");
            foundChis.Columns.Add("PK Value");
            foundChis.Columns.Add("PK Column");
            foundChis.Columns.Add("replacementIndex");
        }
        var shrunkContext = WrapCHIInContext(foundChi, contextValue);
        foundChis.Rows.Add(foundChi, shrunkContext, columnName, pkValue, pkColumn, contextValue.IndexOf(foundChi));
    }
    public DataTable foundChis = new();

    public override void Execute()
    {
        base.Execute();
        List<string> columnAllowList = new();
        if (_allowLists.TryGetValue("RDMP_ALL", out var _extractionSpecificAllowances))
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
            var pkColumn = _catalouge.CatalogueItems.Select(x => x.ColumnInfo).Where(x => x.IsPrimaryKey).First().Name.Split(".").Last();
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
                    var pkValue = getPKValue(pkColumnInfo, row, dt);
                    handleFoundCHI(potentialCHI, value, item.Name, pkValue, pkColumnInfo.Name);
                    if (_bailOutEarly)
                    {
                        break;
                    }
                }


            }
        }
        Console.WriteLine($"Found {foundChis.Rows.Count} CHIs in the {_catalouge.Name} Catalogue.");
        foreach (DataRow row in foundChis.Rows)
        {
            Console.WriteLine($"{row["potential CHI"]} | {row["Context"]} | {row["Source Column Name"]}");

        }
    }

    private string getPKValue(ColumnInfo pkColumnInfo, DataRow row, DataTable dt)
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
