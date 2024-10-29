// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using MongoDB.Driver;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Logging;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using SynthEHR.Datasets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Overview;

/// <summary>
/// Used to populate information about a catalogue for use in the overview UI
/// </summary>
public class OverviewModel
{

    private ICatalogue _catalogue;
    private IBasicActivateItems _activator;

    private DataTable _dataLoads;

    public OverviewModel(IBasicActivateItems activator, ICatalogue catalogue)
    {
        _activator = activator;
        _catalogue = catalogue;
        GetNumberOfPeople();
    }

    public int GetNumberOfRecords()
    {
        DataTable dt = new();

        var discoveredColumn = _catalogue.CatalogueItems.First().ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
        var server = discoveredColumn.Table.Database.Server;
        using var con = server.GetConnection();
        con.Open();
        var sql = $"select count(*) FROM {discoveredColumn.Table.GetRuntimeName()}";
        using var cmd = server.GetCommand(sql, con);
        cmd.CommandTimeout = 30000;
        using var da = server.GetDataAdapter(cmd);
        dt.BeginLoadData();
        da.Fill(dt);
        dt.EndLoadData();
        con.Dispose();
        return int.Parse(dt.Rows[0].ItemArray[0].ToString());
    }

    public int GetNumberOfPeople()
    {
        //extractionInformation isExtractionIdentifier
        var discoveredColumns = _catalogue.CatalogueItems.Where(ci => ci.ExtractionInformation.IsExtractionIdentifier).Select(ci => ci.ColumnInfo.Discover(DataAccessContext.InternalDataProcessing));
        if(discoveredColumns.Count() > 1)
        {
            //more than 1 extraction identifier...
        }
        var server = discoveredColumns.First().Table.Database.Server;
        using var con = server.GetConnection();
        con.Open();
        var columnStrings = string.Join(" , ", discoveredColumns.Select(dc => dc.GetRuntimeName()));
        var sql = $"SELECT COUNT(*) FROM (SELECT DISTINCT({columnStrings}) FROM {discoveredColumns.First().Table.GetRuntimeName()}) as x";
        using var cmd = server.GetCommand(sql, con);
        cmd.CommandTimeout = 30000;
        var result = cmd.ExecuteScalar();
        return int.Parse((string)result);
    }

    public Tuple<DateTime, DateTime> GetStartEndDates(ColumnInfo dateColumn)
    {
        DataTable dt = new();

        var discoveredColumn = _catalogue.CatalogueItems.First().ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
        var server = discoveredColumn.Table.Database.Server;
        using var con = server.GetConnection();
        con.Open();
        var sql = $@"
        select min({dateColumn.GetRuntimeName()}) as min, max({dateColumn.GetRuntimeName()}) as max
        from
        (select {dateColumn.GetRuntimeName()},
        count(1) over (partition by year({dateColumn.GetRuntimeName()})) as occurs 
        from {discoveredColumn.Table.GetRuntimeName()}) as t
        where occurs >1
        ";

        using var cmd = server.GetCommand(sql, con);
        cmd.CommandTimeout = 30000;
        using var da = server.GetDataAdapter(cmd);
        dt.BeginLoadData();
        da.Fill(dt);
        dt.EndLoadData();
        con.Dispose();
        return new Tuple<DateTime, DateTime>(DateTime.Parse(dt.Rows[0].ItemArray[0].ToString()), DateTime.Parse(dt.Rows[0].ItemArray[1].ToString()));
    }


    public DataTable GetCountsByDatePeriod(ColumnInfo dateColumn, string datePeriod, string optionalWhere = "")
    {
        DataTable dt = new();
        if (!(new[] { "Day", "Month", "Year" }).Contains(datePeriod))
        {
            throw new Exception("Invalid Date period");
        }
        var discoveredColumn = dateColumn.Discover(DataAccessContext.InternalDataProcessing);
        var server = discoveredColumn.Table.Database.Server;
        using var con = server.GetConnection();
        con.Open();
        //TODO make this work on non-sql
        var dateString = "yyyy-MM";
        switch (datePeriod)
        {
            case "Day":
                dateString = "yyyy-MM-dd";
                break;
            case "Month":
                dateString = "yyyy-MM";
                break;
            case "Year":
                dateString = "yyyy";
                break;
        }
        var sql = @$"
        SELECT format({dateColumn.GetRuntimeName()}, '{dateString}') as YearMonth, count(*) as '# Records'
        FROM {discoveredColumn.Table.GetRuntimeName()}
        WHERE {dateColumn.GetRuntimeName()} IS NOT NULL
        {(optionalWhere != "" ? "AND" : "")} {optionalWhere.Replace('"', '\'')}
        GROUP BY format({dateColumn.GetRuntimeName()}, '{dateString}')
        ORDER BY 1
        ";

        using var cmd = server.GetCommand(sql, con);
        cmd.CommandTimeout = 30000;
        using var da = server.GetDataAdapter(cmd);
        dt.BeginLoadData();
        da.Fill(dt);
        dt.EndLoadData();
        con.Dispose();
        return dt;
    }

    private void GetDataLoads()
    {
        _dataLoads = new();
        var repo = new MemoryCatalogueRepository();
        var qb = new QueryBuilder(null, null);
        var columnInfo = _catalogue.CatalogueItems.Where(c => c.Name == SpecialFieldNames.DataLoadRunID).Select(c => c.ColumnInfo).FirstOrDefault();
        if (columnInfo != null)
        {
            qb.AddColumn(new ColumnInfoToIColumn(repo, columnInfo));
            qb.AddCustomLine($"{columnInfo.Name} IS NOT NULL", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
            var sql = qb.SQL;
            var server = columnInfo.Discover(DataAccessContext.InternalDataProcessing).Table.Database.Server;
            using var con = server.GetConnection();
            con.Open();

            using var cmd = server.GetCommand(sql, con);
            cmd.CommandTimeout = 30000;
            using var da = server.GetDataAdapter(cmd);
            _dataLoads.BeginLoadData();
            da.Fill(_dataLoads);
            _dataLoads.EndLoadData();
        }

    }

    public DataTable GetDataLoadDetails()
    {
        DataTable dt = new();
        dt.Columns.Add("Data Load");
        dt.Columns.Add("");
        dt.Rows.Add("# Records inserted or updated in most recent data load", 1);
        dt.Rows.Add("Average # Records inserted or updated in data loads", 1);
        dt.Rows.Add("Data Load Frequency", "Weekly");

        return dt;
    }

    public DataTable GetMostRecentDataLoad()
    {
        if (_dataLoads == null) GetDataLoads();
        if (_dataLoads.Rows.Count == 0) return null;
        var maxDataLoadId = _dataLoads.AsEnumerable().Select(r => int.Parse(r[0].ToString())).Distinct().Max();
        //find the most recent dataload id in the catalogue then go grab the dataLoadrun from the logging db
        var loggingServers = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<ExternalDatabaseServer>("CreatedByAssembly", "Rdmp.Core/Databases.LoggingDatabase");
        var columnInfo = _catalogue.CatalogueItems.Where(c => c.Name == SpecialFieldNames.DataLoadRunID).Select(c => c.ColumnInfo).First();
        var server = columnInfo.Discover(DataAccessContext.InternalDataProcessing).Table.Database.Server;

        DataTable dt = new();
        foreach (var loggingServer in loggingServers)
        {
            var logCollection = new ViewLogsCollection(loggingServer, new LogViewerFilter(LoggingTables.DataLoadRun));
            var dataLoadRunSql = $"{logCollection.GetSql()} WHERE ID={maxDataLoadId}";
            var logServer = loggingServer.Discover(DataAccessContext.InternalDataProcessing).Server;
            using var loggingCon = logServer.GetConnection();
            loggingCon.Open();
            using var loggingCmd = logServer.GetCommand(dataLoadRunSql, loggingCon);
            loggingCmd.CommandTimeout = 30000;
            using var loggingDa = server.GetDataAdapter(loggingCmd);
            dt.BeginLoadData();
            loggingDa.Fill(dt);
            dt.EndLoadData();
            loggingCon.Dispose();
            if (dt.Rows.Count > 0)
            {
                //if we've found it, then stop
                break;
            }
        }
        return dt;
    }

    public List<CumulativeExtractionResults> GetExtractions()
    {
        var datasets = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Select(d => d.ID);
        var results = _activator.RepositoryLocator.DataExportRepository.GetAllObjects<CumulativeExtractionResults>().Where(result => datasets.Contains(result.ExtractableDataSet_ID)).ToList();
        return results;

    }

}
