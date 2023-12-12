// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdmp.Core.Curation.Data;
using YamlDotNet.Serialization;
using System.IO;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System.Data;
using static NPOI.HSSF.Util.HSSFColor;
using Rdmp.Core.Curation.Data.Defaults;
using TB.ComponentModel;
using FAnsi.Discovery.TableCreation;
using Rdmp.Core.DataFlowPipeline;
using Microsoft.Data.SqlClient;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandRedactCHIsFromCatalogue : BasicCommandExecution, IAtomicCommand
{

    private ICatalogue _catalogue;
    private IBasicActivateItems _activator;
    private readonly Dictionary<string, List<string>> _allowLists = new();
    public int redactionCount = 0;
    private string _allowListLocation = "";

    public ExecuteCommandRedactCHIsFromCatalogue(IBasicActivateItems activator, [DemandsInitialization("The catalogue to search")] ICatalogue catalogue, string allowListLocation = null) : base(activator)
    {
        _catalogue = catalogue;
        _activator = activator;
        _allowListLocation = allowListLocation;
        //if (!string.IsNullOrWhiteSpace(allowListLocation))
        //{
        //    var allowListFileContent = File.ReadAllText(allowListLocation);
        //    var deserializer = new DeserializerBuilder().Build();
        //    var yamlObject = deserializer.Deserialize<Dictionary<string, List<string>>>(allowListFileContent);
        //    foreach (var (cat, columns) in yamlObject)
        //    {
        //        _allowLists.Add(cat, columns);
        //    }
        //}
    }
    //private void handleFoundCHI(string foundChi, string table, string column, string columnValue)
    //{
    //    //check for PK
    //    Console.WriteLine("Found CHI!");
    //    redactionCount++;
    //    var rc = new RedactedCHI(_activator.RepositoryLocator.CatalogueRepository, foundChi, ExecuteCommandIdentifyCHIInCatalogue.WrapCHIInContext(foundChi, columnValue, 20), $"{table}.{column}");
    //    rc.SaveToDatabase();
    //    var redactedValue = columnValue.Replace(foundChi, $"REDACTED_CHI_{rc.ID}");
    //    //TODO can be smarted about how we wrote tothe db, can share a connection etc
    //    var sql = $"UPDATE {table} SET {column}='{redactedValue}' where {column}='{columnValue}'";
    //    var server = _catalouge.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
    //    var conn = server.GetConnection();
    //    conn.Open();
    //    using (var cmd = server.GetCommand(sql, conn))
    //    {
    //        cmd.ExecuteNonQuery();
    //        conn.Close();
    //    }
    //}


    public override void Execute()
    {
        base.Execute();
        var cmd = new ExecuteCommandIdentifyCHIInCatalogue(_activator, _catalogue, false, _allowListLocation);
        cmd.Execute();
        DataTable results = cmd.foundChis;
        foreach(DataRow row in results.Rows)
        {
            redactionCount++;
            var result = row;
            var foundChi = result.ItemArray[0].ToString();
            var columnValue = result.ItemArray[1].ToString();
            var column = result.ItemArray[2].ToString();
            var catalogueItem = _catalogue.CatalogueItems.Where(ci => ci.Name == column).First();
            var name = catalogueItem.ColumnInfo.Name;
            var pkValue = result.ItemArray[3].ToString();
            var replacementIdex = int.Parse(result.ItemArray[5].ToString());
            var table = name.Replace($".[{column}]", "");
            var pkColumn = result.ItemArray[4].ToString().Replace(table, "").Replace(".", "");
            var rc = new RedactedCHI(_catalogue.CatalogueRepository, foundChi, replacementIdex, table, pkValue, pkColumn, $"[{column}]");
            rc.SaveToDatabase();

            var redactedString = new string('#', foundChi.Length);
            var fetchSQL = $"select TOP 1 {column} from {table} where {pkColumn} = '{pkValue}' and charindex('{foundChi}',{column},{replacementIdex}) >0";
            var existingResultsDT = new DataTable();
            var columnInfo = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ColumnInfo>().Where(ci => ci.Name == $"{table}.[{column}]").First();
            var catalogue = columnInfo.CatalogueItems.FirstOrDefault().Catalogue;
            using (var con = (SqlConnection)catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false).GetConnection())
            {
                con.Open();
                var da = new SqlDataAdapter(new SqlCommand(fetchSQL, con));
                da.Fill(existingResultsDT);
                if (existingResultsDT.Rows.Count > 0 && existingResultsDT.Rows[0].ItemArray.Length > 0)
                {
                    var currentContext = existingResultsDT.Rows[0].ItemArray[0].ToString();
                    var newContext = currentContext.Replace(foundChi, redactedString);
                    var updateSQL = $"update {table} set {column}='{newContext}' where {pkColumn} = '{pkValue}' and {column}='{currentContext}'";
                    var updateCmd = new SqlCommand(updateSQL, con);
                    updateCmd.ExecuteNonQuery();
                }
            }
        }
        //List<string> columnAllowList = new();
        //if (_allowLists.TryGetValue("RDMP_ALL", out var _extractionSpecificAllowances))
        //    columnAllowList.AddRange(_extractionSpecificAllowances);
        //if (_allowLists.TryGetValue(_catalouge.Name, out var _catalogueSpecificAllowances))
        //    columnAllowList.AddRange(_catalogueSpecificAllowances.ToList());
        //foreach (var item in _catalouge.CatalogueItems)
        //{
        //    if (columnAllowList.Contains(item.Name)) continue;

        //    var column = item.ColumnInfo.Name;
        //    int idxOfLastSplit = column.LastIndexOf('.');
        //    string table = column[..idxOfLastSplit];
        //    var columnName = column[(idxOfLastSplit + 1)..];
        //    var server = _catalouge.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
        //    var sql = $"SELECT {columnName} from {table}";
        //    var dt = new DataTable();
        //    dt.BeginLoadData();
        //    using (var cmd = server.GetCommand(sql, server.GetConnection()))
        //    {
        //        using var da = server.GetDataAdapter(cmd);
        //        da.Fill(dt);
        //    }
        //    dt.EndLoadData();
        //    foreach (DataRow row in dt.Rows)
        //    {

        //        var value = row[dt.Columns[0].ColumnName].ToString();
        //        var potentialCHI = CHIColumnFinder.GetPotentialCHI(value);
        //        if (!string.IsNullOrWhiteSpace(potentialCHI))
        //        {
        //            handleFoundCHI(potentialCHI, table,columnName,value);
        //        }
        //    }
        //}
    }
}
