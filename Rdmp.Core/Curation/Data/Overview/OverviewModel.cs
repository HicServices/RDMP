// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MongoDB.Driver;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Overview;

/// <summary>
/// Used to populate information about a catalogue for use in the overview UI
/// </summary>
public class OverviewModel
{

    private readonly ICatalogue _catalogue;
    private readonly IBasicActivateItems _activator;
    private CatalogueOverview _catalogueOverview;

    public OverviewModel(IBasicActivateItems activator, ICatalogue catalogue)
    {
        _activator = activator;
        _catalogue = catalogue;
        _catalogueOverview = activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CatalogueOverview>("Catalogue_ID", catalogue.ID).FirstOrDefault();
    }

    public void Generate()
    {
        var recordCount = GetRecordCount();
        var peopleCount = GetPeopleCount();
        if (_catalogueOverview is null)
        {
            _catalogueOverview = new CatalogueOverview(_activator.RepositoryLocator.CatalogueRepository, _catalogue.ID, recordCount, peopleCount);
        }
        //data load date
        _catalogueOverview.LastDataLoad = GetDataLoadDate();
        //extraction date
        _catalogueOverview.LastExtractionTime = GetExtractionTime();
        //date range
        var dates = GetDates();
        _catalogueOverview.StartDate = dates.Item1;
        _catalogueOverview.EndDate = dates.Item2;
        _catalogueOverview.SaveToDatabase();

        GetCountsByDate();
        //todo generate graph data
    }

    private int GetRecordCount()
    {
        var column = _catalogue.CatalogueItems.Where(ci => ci.ExtractionInformation.IsExtractionIdentifier).FirstOrDefault();
        if (column is null)
        {
            column = _catalogue.CatalogueItems.FirstOrDefault();
        }
        if (column is null) return 0;
        var discoveredColumn = column.ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
        return discoveredColumn.Table.GetRowCount();
    }

    private int GetPeopleCount()
    {
        var column = _catalogue.CatalogueItems.Where(ci => ci.ExtractionInformation.IsExtractionIdentifier).FirstOrDefault();
        if (column is null) return 0;
        var discoveredColumn = column.ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
        var qb = new QueryBuilder("DISTINCT", null, null);
        var memRepo = new MemoryRepository();
        qb.AddColumn(new ColumnInfoToIColumn(memRepo, column.ColumnInfo));
        var server = discoveredColumn.Table.Database.Server;
        using var con = server.GetConnection();
        con.Open();
        using var cmd = server.GetCommand($"SELECT count(*) from ({qb.SQL})as dt", con);
        return int.Parse(cmd.ExecuteScalar().ToString());
    }

    private DateTime? GetDataLoadDate()
    {
        var column = _catalogue.CatalogueItems.Where(c => c.Name == SpecialFieldNames.ValidFrom).FirstOrDefault();
        if (column is null) return null;
        var discoveredColumn = column.ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
        var qb = new QueryBuilder("DISTINCT", null, null);
        var memRepo = new MemoryRepository();
        qb.AddColumn(new ColumnInfoToIColumn(memRepo, column.ColumnInfo));
        var server = discoveredColumn.Table.Database.Server;
        using var con = server.GetConnection();
        con.Open();
        using var cmd = server.GetCommand(qb.SQL, con);
        using var da = server.GetDataAdapter(cmd);
        var dt = new DataTable();
        da.Fill(dt);
        var rows = dt.AsEnumerable().Where(r => !string.IsNullOrEmpty(r[0].ToString()));
        if (!rows.Any()) return null;
        return rows.Select(r => DateTime.Parse(r[0].ToString())).Max();
    }

    private DateTime? GetExtractionTime()
    {
        var datasets = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Select(d => d.ID);
        var results = _activator.RepositoryLocator.DataExportRepository.GetAllObjects<CumulativeExtractionResults>().Where(result => datasets.Contains(result.ExtractableDataSet_ID)).ToList();
        if (!results.Any()) return null;
        return results.Select(r => r.DateOfExtraction).Max();

    }


    private Tuple<DateTime?, DateTime?> GetDates()
    {
        var column = _catalogue.CatalogueItems.Where(c => c.ColumnInfo.Data_type == "datetime2").FirstOrDefault();//temp
        if (column == null) return null;
        if (column is null) return null;
        var discoveredColumn = column.ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
        var qb = new QueryBuilder("DISTINCT", null, null);
        var memRepo = new MemoryRepository();
        qb.AddColumn(new ColumnInfoToIColumn(memRepo, column.ColumnInfo));
        var server = discoveredColumn.Table.Database.Server;
        using var con = server.GetConnection();
        con.Open();
        using var cmd = server.GetCommand(qb.SQL, con);
        using var da = server.GetDataAdapter(cmd);
        var dt = new DataTable();
        da.Fill(dt);
        var rows = dt.AsEnumerable().Where(r => !string.IsNullOrEmpty(r[0].ToString()));
        if (!rows.Any()) return null;
        var dateTimeRows = rows.Select(r => DateTime.Parse(r[0].ToString()));
        var max = dateTimeRows.Max();
        var min = dateTimeRows.Min();
        return new Tuple<DateTime?, DateTime?>(min, max);
    }
    //public void Regen(string whereClause)
    //{
    //    DataTable dt = new();
    //    bool hasExtractionIdentifier = true;
    //    var column = _catalogue.CatalogueItems.Where(ci => ci.ExtractionInformation.IsExtractionIdentifier).FirstOrDefault();
    //    if (column is null)
    //    {
    //        column = _catalogue.CatalogueItems.FirstOrDefault();
    //        hasExtractionIdentifier = false;
    //    }
    //    if (column is null) return;
    //    var discoveredColumn = column.ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
    //    var server = discoveredColumn.Table.Database.Server;
    //    using var con = server.GetConnection();
    //    con.Open();
    //    string populatedWhere = !string.IsNullOrWhiteSpace(whereClause) ? $"WHERE {whereClause}" : "";
    //    var sql = $"SELECT {column.ColumnInfo.GetRuntimeName()} FROM {discoveredColumn.Table.GetRuntimeName()} {populatedWhere}";
    //    using var cmd = server.GetCommand(sql, con);
    //    cmd.CommandTimeout = 30000;
    //    using var da = server.GetDataAdapter(cmd);
    //    dt.BeginLoadData();
    //    da.Fill(dt);
    //    dt.EndLoadData();
    //    con.Dispose();
    //    _numberOfRecords = dt.Rows.Count;
    //    _numberOfPeople = hasExtractionIdentifier ? dt.DefaultView.ToTable(true, column.ColumnInfo.GetRuntimeName()).Rows.Count : 0;
    //    GetDataLoads();
    //    dt.Dispose();
    //}

    private void GetCountsByDate()
    {
        var column = _catalogue.CatalogueItems.Where(c => c.ColumnInfo.Data_type == "datetime2").FirstOrDefault();//temp
        var dateString = "yyyy-MM";
        DataTable dt = new();
        var discoveredColumn = column.ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
        var server = discoveredColumn.Table.Database.Server;
        using var con = server.GetConnection();
        con.Open();
        var repo = new MemoryCatalogueRepository();
        var qb = new QueryBuilder(null, null);
        qb.AddColumn(new ColumnInfoToIColumn(repo, column.ColumnInfo));
        qb.AddCustomLine($"{column.ColumnInfo.Name} IS NOT NULL", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
        var cmd = server.GetCommand(qb.SQL, con);
        using var da = server.GetDataAdapter(cmd);
        dt.BeginLoadData();
        da.Fill(dt);
        dt.EndLoadData();
        con.Dispose();
        Dictionary<string, int> counts = [];
        foreach (var key in dt.AsEnumerable().Select(row => DateTime.Parse(row[0].ToString()).ToString(dateString)))
        {
            if (counts.TryGetValue(key, out var count))
            {
                counts[key]++;
            }
            else
            {
                counts[key] = 1;
            }
        }

        //this is stupidly slow
        foreach (CatalogueOverviewDataPoint dp in _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CatalogueOverviewDataPoint>("CatalogueOverview_ID", _catalogueOverview.ID).ToList())
        {
            dp.DeleteInDatabase();
        }
        foreach (var item in counts)
        {
            var dp = new CatalogueOverviewDataPoint(_activator.RepositoryLocator.CatalogueRepository, _catalogueOverview.ID, DateTime.Parse(item.Key), item.Value);
            dp.SaveToDatabase();
        }
    }

    public int GetNumberOfRecords()
    {
        return _catalogueOverview.NumberOfRecords;
    }

    public int GetNumberOfPeople()
    {
        return _catalogueOverview.NumberOfPeople;
    }

    public string GetLatestExtraction()
    {
        return _catalogueOverview.LastExtractionTime != null ? _catalogueOverview.LastExtractionTime.ToString() : null;
    }

    public string GetLatestDataLoad()
    {
        return _catalogueOverview.LastDataLoad != null ? _catalogueOverview.LastDataLoad.ToString() : null;
    }

    public Tuple<DateTime?, DateTime?> GetStartEndDates()
    {
        return new Tuple<DateTime?, DateTime?>(_catalogueOverview.StartDate, _catalogueOverview.EndDate);
    }


    public static DataTable GetCountsByDatePeriod(ColumnInfo dateColumn, string datePeriod, string optionalWhere = "")
    {
        DataTable dt = new();
        //if (!(new[] { "Day", "Month", "Year" }).Contains(datePeriod))
        //{
        //    throw new Exception("Invalid Date period");
        //}
        //var discoveredColumn = dateColumn.Discover(DataAccessContext.InternalDataProcessing);
        //var server = discoveredColumn.Table.Database.Server;
        //using var con = server.GetConnection();
        //con.Open();
        //var dateString = "yyyy-MM";
        //switch (datePeriod)
        //{
        //    case "Day":
        //        dateString = "yyyy-MM-dd";
        //        break;
        //    case "Month":
        //        dateString = "yyyy-MM";
        //        break;
        //    case "Year":
        //        dateString = "yyyy";
        //        break;
        //}
        //if (server.DatabaseType == FAnsi.DatabaseType.MicrosoftSQLServer)
        //{
        //    var sql = @$"
        //SELECT format({dateColumn.GetRuntimeName()}, '{dateString}') as YearMonth, count(*) as '# Records'
        //FROM {discoveredColumn.Table.GetRuntimeName()}
        //WHERE {dateColumn.GetRuntimeName()} IS NOT NULL
        //{(optionalWhere != "" ? "AND" : "")} {optionalWhere.Replace('"', '\'')}
        //GROUP BY format({dateColumn.GetRuntimeName()}, '{dateString}')
        //ORDER BY 1
        //";

        //    using var cmd = server.GetCommand(sql, con);
        //    cmd.CommandTimeout = 30000;
        //    using var da = server.GetDataAdapter(cmd);
        //    dt.BeginLoadData();
        //    da.Fill(dt);
        //    dt.EndLoadData();
        //}
        //else
        //{
        //    var repo = new MemoryCatalogueRepository();
        //    var qb = new QueryBuilder(null, null);
        //    qb.AddColumn(new ColumnInfoToIColumn(repo, dateColumn));
        //    qb.AddCustomLine($"{dateColumn.Name} IS NOT NULL", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
        //    var cmd = server.GetCommand(qb.SQL, con);
        //    using var da = server.GetDataAdapter(cmd);
        //    dt.BeginLoadData();
        //    da.Fill(dt);
        //    Dictionary<string, int> counts = [];
        //    foreach (var key in dt.AsEnumerable().Select(row => DateTime.Parse(row.ItemArray[0].ToString()).ToString(dateString)))
        //    {
        //        counts[key]++;
        //    }
        //    dt = new DataTable();
        //    foreach (var item in counts)
        //    {
        //        DataRow dr = dt.NewRow();
        //        dr["YearMonth"] = item.Key;
        //        dr["# Records"] = item.Value;
        //        dt.Rows.Add(dr);
        //    }
        //    dt.EndLoadData();

        //}
        //con.Dispose();
        return dt;
    }

}
