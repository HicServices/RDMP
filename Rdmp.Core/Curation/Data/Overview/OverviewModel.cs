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

    public void Generate(int dateColumn_ID)
    {
        var recordCount = 1;// GetRecordCount();
        var peopleCount = 1;// GetPeopleCount();
        if (_catalogueOverview is null)
        {
            _catalogueOverview = new CatalogueOverview(_activator.RepositoryLocator.CatalogueRepository, _catalogue.ID, recordCount, peopleCount, dateColumn_ID);
        }
        //_catalogueOverview.LastDataLoad = GetDataLoadDate();
        //_catalogueOverview.LastExtractionTime = GetExtractionTime();
        //_catalogueOverview.DateColumn_ID = dateColumn_ID;
        //var dates = GetDates();
        //_catalogueOverview.StartDate = dates?.Item1;
        //_catalogueOverview.EndDate = dates?.Item2;
        _catalogueOverview.SaveToDatabase();

        GetCountsByDate();
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
        DateTime? min = null;
        DateTime? max = null;
        var column = _catalogue.CatalogueItems.Where(c => c.ID == _catalogueOverview.DateColumn_ID).FirstOrDefault();
        if (column == null) return new Tuple<DateTime?, DateTime?>(min, max); ;
        var discoveredColumn = column.ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
        var server = discoveredColumn.Table.Database.Server;
        if (server.DatabaseType == FAnsi.DatabaseType.MicrosoftSQLServer)
        {
            var qb = new QueryBuilder(null, null, null);
            var memRepo = new MemoryRepository();
            qb.AddColumn(new ColumnInfoToIColumn(memRepo, column.ColumnInfo));
            using var con = server.GetConnection();
            con.Open();
            using var cmd = server.GetCommand($"SELECT max({column.Name}) from {discoveredColumn.Table.GetRuntimeName()}", con);
            max = DateTime.Parse((cmd.ExecuteScalar().ToString()));
            using var cmd2 = server.GetCommand($"SELECT min({column.Name}) from {discoveredColumn.Table.GetRuntimeName()}", con);
            min = DateTime.Parse((cmd2.ExecuteScalar().ToString()));
        }
        else
        {
            var qb = new QueryBuilder("DISTINCT", null, null);
            var memRepo = new MemoryRepository();
            qb.AddColumn(new ColumnInfoToIColumn(memRepo, column.ColumnInfo));
            using var con = server.GetConnection();
            con.Open();
            using var cmd = server.GetCommand(qb.SQL, con);
            cmd.CommandTimeout = 300;
            using var da = server.GetDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            var rows = dt.AsEnumerable().Where(r => !string.IsNullOrEmpty(r[0].ToString()));
            if (!rows.Any()) return new Tuple<DateTime?, DateTime?>(min, max);
            var dateTimeRows = rows.Select(r => DateTime.Parse(r[0].ToString()));
            max = dateTimeRows.Max();
            min = dateTimeRows.Min();
        }
        return new Tuple<DateTime?, DateTime?>(min, max);
    }

    private void GetCountsByDate()
    {
        var column = _catalogue.CatalogueItems.Where(c => c.ID == _catalogueOverview.DateColumn_ID).FirstOrDefault();
        var dateString = "yyyy-MM";
        var discoveredColumn = column.ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
        var server = discoveredColumn.Table.Database.Server;
        using var con = server.GetConnection();
        con.Open();
        var counts = new DataTable();
        if (server.DatabaseType == FAnsi.DatabaseType.MicrosoftSQLServer)
        {

            var sql = @$"
                SELECT 

                FORMAT({column.Name},'{dateString}'), count(FORMAT({column.Name},'{dateString}'))
                FROM 
                {discoveredColumn.Table.GetRuntimeName()}
                WHERE
                {column.Name} IS NOT NULL
                Group by FORMAT({column.Name},'{dateString}')
                ORDER BY FORMAT({column.Name},'{dateString}')";
            var cmd = server.GetCommand(sql, con);
            using var da = server.GetDataAdapter(cmd);
            counts.BeginLoadData();
            da.Fill(counts);
            counts.EndLoadData();
            con.Dispose();
        }
        else
        {
            //nonsql v slow
            var repo = new MemoryCatalogueRepository();
            var qb = new QueryBuilder(null, null);
            qb.AddColumn(new ColumnInfoToIColumn(repo, column.ColumnInfo));
            qb.AddCustomLine($"{column.ColumnInfo.Name} IS NOT NULL", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
            var cmd = server.GetCommand(qb.SQL, con);
            using var da = server.GetDataAdapter(cmd);
            var dt = new DataTable();
            dt.BeginLoadData();
            da.Fill(dt);
            dt.EndLoadData();
            con.Dispose();
            Dictionary<string, int> _counts = [];
            foreach (var key in dt.AsEnumerable().Select(row => DateTime.Parse(row[0].ToString()).ToString(dateString)))
            {
                if (_counts.TryGetValue(key, out var count))
                {
                    _counts[key]++;
                }
                else
                {
                    _counts[key] = 1;
                }
            }
            foreach (var item in _counts)
            {
                DataRow dr = counts.NewRow();
                dr[0] = item.Key;
                dr[1] = item.Value;
            }
        }
        if (_activator.RepositoryLocator.CatalogueRepository.GetType() == typeof(CatalogueRepository))
        {
            //fast
            ((CatalogueRepository)_activator.RepositoryLocator.CatalogueRepository).Delete($"DELETE CatalogueOverviewDataPoint WHERE CatalogueOverview_ID = {_catalogueOverview.ID}", null, false);
        }
        else
        {
            //very slow
            foreach (CatalogueOverviewDataPoint dp in _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CatalogueOverviewDataPoint>("CatalogueOverview_ID", _catalogueOverview.ID).ToList())
            {
                dp.DeleteInDatabase();
            }
        }

        foreach (var item in counts.AsEnumerable())
        {
            var dp = new CatalogueOverviewDataPoint(_activator.RepositoryLocator.CatalogueRepository, _catalogueOverview.ID, DateTime.Parse(item[0].ToString()), (int)item[1]);
            dp.SaveToDatabase();
        }
    }

    public int GetNumberOfRecords()
    {
        return _catalogueOverview != null ? _catalogueOverview.NumberOfRecords : 0;
    }

    public int GetNumberOfPeople()
    {
        return _catalogueOverview != null ? _catalogueOverview.NumberOfPeople : 0;
    }

    public string GetLatestExtraction()
    {
        return _catalogueOverview?.LastExtractionTime != null ? ((DateTime)_catalogueOverview.LastExtractionTime).ToString("dd/MM/yyyy") : null;
    }

    public string GetLatestDataLoad()
    {
        return _catalogueOverview?.LastDataLoad != null ? ((DateTime)_catalogueOverview.LastDataLoad).ToString("dd/MM/yyyy") : null;
    }

    public Tuple<DateTime?, DateTime?> GetStartEndDates()
    {
        return new Tuple<DateTime?, DateTime?>(_catalogueOverview?.StartDate, _catalogueOverview?.EndDate);
    }

    public int? GetDateColumn()
    {
        return _catalogueOverview != null ? _catalogueOverview.DateColumn_ID : null;
    }
    public DataTable GetCountsByDatePeriod()
    {

        var dt = new DataTable();
        dt.Columns.Add("YearMonth");
        dt.Columns.Add("# Records");
        if (_catalogueOverview is null) return dt;
        var counts = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CatalogueOverviewDataPoint>("CatalogueOverview_ID", _catalogueOverview.ID).ToList().OrderBy(c => c.Date);
        foreach (var item in counts)
        {
            dt.Rows.Add([item.Date.ToString("yyyy-MM"), item.Count]);
        }

        return dt;
    }

}
