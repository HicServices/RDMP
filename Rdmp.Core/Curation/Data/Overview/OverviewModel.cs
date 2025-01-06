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
using Rdmp.Core.DataQualityEngine.Data;
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
    //private CatalogueOverview _catalogueOverview;
    private Evaluation _evaluation;
    private DataTable _dqeTable = new();
    //private DateTime? _extractionDate;
    private DateTime? _dataLoadDate;

    public OverviewModel(IBasicActivateItems activator, ICatalogue catalogue)
    {
        _activator = activator;
        _catalogue = catalogue;
        _dqeTable = GetCountsByDatePeriod();
        _dataLoadDate = GetDataLoadDate();
    }

    public DataTable GetTableData()
    {
        return _dqeTable;
    }

    private DateTime? GetDataLoadDate()
    {
        int dataLoadID = _evaluation.RowStates.Max(rs => rs.DataLoadRunID);
        if (dataLoadID > 0) {
            //can use the hic_validFrom in the underlying catalogue table to get the data load date
            var pk = _catalogue.CatalogueItems.Select(c => c.ColumnInfo).Where(c => c.IsPrimaryKey).FirstOrDefault();
            if (pk is not null)
            {
                var table = pk.TableInfo.Discover(DataAccessContext.InternalDataProcessing);
                table.
            }
        }

        return null;
    }

    //private int GetRecordCount()
    //{
    //    var column = _catalogue.CatalogueItems.Where(ci => ci.ExtractionInformation.IsExtractionIdentifier).FirstOrDefault();
    //    if (column is null)
    //    {
    //        column = _catalogue.CatalogueItems.FirstOrDefault();
    //    }
    //    if (column is null) return 0;
    //    var discoveredColumn = column.ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
    //    return discoveredColumn.Table.GetRowCount();
    //}

    //private int GetPeopleCount()
    //{
    //    var column = _catalogue.CatalogueItems.Where(ci => ci.ExtractionInformation.IsExtractionIdentifier).FirstOrDefault();
    //    if (column is null) return 0;
    //    var discoveredColumn = column.ColumnInfo.Discover(DataAccessContext.InternalDataProcessing);
    //    var qb = new QueryBuilder("DISTINCT", null, null);
    //    var memRepo = new MemoryRepository();
    //    qb.AddColumn(new ColumnInfoToIColumn(memRepo, column.ColumnInfo));
    //    var server = discoveredColumn.Table.Database.Server;
    //    using var con = server.GetConnection();
    //    con.Open();
    //    using var cmd = server.GetCommand($"SELECT count(*) from ({qb.SQL})as dt", con);
    //    return int.Parse(cmd.ExecuteScalar().ToString());
    //}

    public int GetNumberOfRecords()
    {
        return _dqeTable.AsEnumerable()
    .Sum(x => int.Parse(x["# Records"].ToString()));
    }

    public int GetNumberOfPeople()
    {
        return 0;// _catalogueOverview != null ? _catalogueOverview.NumberOfPeople : 0;
    }

    public string GetLatestExtraction()
    {
        return null;// _extractionDate != null ? ((DateTime)_extractionDate).ToString("MM-yyyy") : null;
    }

    public string GetLatestDataLoad()
    {
        return _dataLoadDate!= null ? ((DateTime)_dataLoadDate).ToString("MM-yyyy") : null;
    }

    public Tuple<DateTime?, DateTime?> GetStartEndDates()
    {
        var start = DateTime.Parse(_dqeTable.AsEnumerable().First()["YearMonth"].ToString());
        var end= DateTime.Parse(_dqeTable.AsEnumerable().Last()["YearMonth"].ToString());
        return new Tuple<DateTime?, DateTime?>(start,end);
    }

    private DataTable GetCountsByDatePeriod()
    {

        var dt = new DataTable();
        var repo = new DQERepository(_catalogue.CatalogueRepository);
        _evaluation = repo.GetAllObjectsWhere<Evaluation>("CatalogueID", _catalogue.ID).LastOrDefault();
        if (_evaluation != null)
        {
            dt = PeriodicityState.GetPeriodicityForDataTableForEvaluation(_evaluation, "ALL", true);
            dt.Columns.Add("# Records");
            foreach (DataRow row in dt.Rows)
            {
                row["# Records"] = int.Parse(row["Correct"].ToString()) + int.Parse(row["Wrong"].ToString()) + int.Parse(row["Missing"].ToString()) + int.Parse(row["InvalidatesRow"].ToString());
            }
            dt.Columns.Remove("Year");
            dt.Columns.Remove("Month");
            dt.Columns.Remove("Correct");
            dt.Columns.Remove("Wrong");
            dt.Columns.Remove("Missing");
            dt.Columns.Remove("InvalidatesRow");
        }
        return dt;
    }

}
