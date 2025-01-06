// Copyright (c) The University of Dundee 2024-2025
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MongoDB.Driver;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Data;
using System.Linq;

namespace Rdmp.Core.Curation.Data.Overview;

/// <summary>
/// Used to populate information about a catalogue for use in the overview UI
/// </summary>
public class OverviewModel
{

    private readonly ICatalogue _catalogue;
    private readonly IBasicActivateItems _activator;
    private Evaluation _evaluation;
    private readonly DataTable _dqeTable = new();
    private readonly DateTime? _extractionDate;
    private readonly DateTime? _dataLoadDate;

    public OverviewModel(IBasicActivateItems activator, ICatalogue catalogue)
    {
        _activator = activator;
        _catalogue = catalogue;
        _dqeTable = GetCountsByDatePeriod();
        _dataLoadDate = GetDataLoadDate();
        _extractionDate = GetExtractionDate();
    }

    public bool HasDQEEvaluation()
    {
        return _evaluation is not null;
    }

    public DataTable GetTableData()
    {
        return _dqeTable;
    }

    private DateTime? GetExtractionDate()
    {
        var extractableDataSets = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID);
        DateTime? maxDateOfExtraction = null;
        foreach(var eds in extractableDataSets)
        {
            var results = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<CumulativeExtractionResults>("ExtractableDataSet_ID", eds.ID);
            if (results.Length != 0)
            {
                var max = results.Select(cer => cer.DateOfExtraction).Max();
                if (maxDateOfExtraction == null || max > maxDateOfExtraction)
                {
                    maxDateOfExtraction = max;
                }
            }
        }
        return maxDateOfExtraction;
    }

    private DateTime? GetDataLoadDate()
    {
        if (_evaluation is null) return null;
        int dataLoadID = _evaluation.RowStates.Max(rs => rs.DataLoadRunID);
        if (dataLoadID > 0) {
            var loggingDB = _catalogue.CatalogueRepository.GetAllObjectsWhere<ExternalDatabaseServer>("CreatedByAssembly", "Rdmp.Core/Databases.LoggingDatabase").FirstOrDefault();
            if (loggingDB != null) {
                var discoveredDB = loggingDB.Discover(DataAccessContext.InternalDataProcessing);
                var discoveredTable = discoveredDB.ExpectTable("TableLoadRun");
                if (discoveredTable != null) {
                    var conn = discoveredDB.Server.GetConnection();
                    conn.Open();
                    var cmd = discoveredTable.GetCommand($"SELECT startTime FROM {discoveredTable.GetFullyQualifiedName()} WHERE dataLoadRunID = {dataLoadID}", conn);
                    var result = cmd.ExecuteScalar();
                    conn.Close();
                    if(result != null)
                    {
                        return DateTime.Parse(result.ToString());
                    }
                }
            }
        }

        return null;
    }

    public int GetNumberOfRecords()
    {
        return _dqeTable.AsEnumerable()
    .Sum(x => int.Parse(x["# Records"].ToString()));
    }

    public string GetLatestExtraction()
    {
        return _extractionDate != null ? ((DateTime)_extractionDate).ToString("dd/MM/yyyy") : null;
    }

    public string GetLatestDataLoad()
    {
        return _dataLoadDate!= null ? ((DateTime)_dataLoadDate).ToString("dd/MM/yyyy") : null;
    }

    public Tuple<DateTime?, DateTime?> GetStartEndDates()
    {
        if(_dqeTable.Rows.Count ==0) return new Tuple<DateTime?, DateTime?>(null,null);
        var start = DateTime.Parse(_dqeTable.AsEnumerable().First()["YearMonth"].ToString());
        var end= DateTime.Parse(_dqeTable.AsEnumerable().Last()["YearMonth"].ToString());
        return new Tuple<DateTime?, DateTime?>(start,end);
    }

    private DataTable GetCountsByDatePeriod()
    {

        var dt = new DataTable();
        try
        {
            var repo = new DQERepository(_catalogue.CatalogueRepository);
            _evaluation = repo.GetAllObjectsWhere<Evaluation>("CatalogueID", _catalogue.ID).LastOrDefault();
        }catch(Exception) {
            return dt;
        }
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
