// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateHoldoutLookup : BasicCommandExecution
{
    private readonly CohortIdentificationConfiguration _cic;
    readonly IBasicActivateItems _activator;
    private DiscoveredServer _server;
    private DataTable _dataTable;


    public ExecuteCommandCreateHoldoutLookup(IBasicActivateItems activator,
        CohortIdentificationConfiguration cic) : base(activator)
    {
        _cic = cic;
        _activator = activator;
    }

    public override string GetCommandName() => "Create Holdout";

    /// <summary>
    /// Describes in a user friendly way the activity of picking an <see cref="ExternalCohortTable"/>
    /// </summary>
    /// <returns></returns>
    private static DialogArgs GetChooseCohortDialogArgs() =>
        new()
        {
            WindowTitle = "Choose where to save cohort",
            TaskDescription =
                "Select the Cohort Database in which to store the identifiers.  If you have multiple methods of anonymising cohorts or manage different types of identifiers (e.g. CHI lists, ECHI lists and/or BarcodeIDs) then you must pick the Cohort Database that matches your cohort identifier type/anonymisation protocol.",
            EntryLabel = "Select Cohort Database",
            AllowAutoSelect = true
        };

    private DataTable LoadDataTable(DiscoveredServer server, string sql)
    {

        var dt = new DataTable();

        try
        {
            using var con = server.GetConnection();
            con.Open();
            using var cmd = server.GetCommand(sql, con);
            cmd.CommandTimeout = 10000;
            var adapter = server.GetDataAdapter(cmd);
            dt.BeginLoadData();
            adapter.Fill(dt);
            dt.EndLoadData();
            con.Close();
        }
        catch (Exception e)
        {
            GlobalError("Unable to access datatable",e);
        }
        return dt;

    }

    private const string HoldoutShuffle = "_HoldoutShuffle";

    public override void Execute()
    {
        base.Execute();

        SelectOne(GetChooseCohortDialogArgs(),
                    BasicActivator.RepositoryLocator.DataExportRepository,
                    out ExternalCohortTable ect);
        if (ect is null)
            return;

        var holdoutRequest = BasicActivator.GetCohortHoldoutLookupRequest(ect, null, _cic);
        if(holdoutRequest is null)
            return;

        var cohortConfiguration = new ViewCohortIdentificationConfigurationSqlCollection(_cic);
        var sql = cohortConfiguration.GetSql();
        _server = DataAccessPortal
                .ExpectServer(cohortConfiguration.GetDataAccessPoint(), DataAccessContext.InternalDataProcessing, false);
        _server.TestConnection();
        _dataTable = LoadDataTable(_server, sql);
        if(_dataTable.Rows.Count == 0)
        {
            Show("Unable to Access Cohort");
            return;
        }
        StringBuilder sb = new();

        var columnNames = _dataTable.Columns.Cast<DataColumn>().Select(static column => column.ColumnName).ToArray();
        sb.AppendLine(string.Join(",", columnNames));
        _dataTable.Columns.Add(HoldoutShuffle);
        Random rnd = new();
        foreach (DataRow row in _dataTable.Rows)
        {
            row[HoldoutShuffle] = rnd.Next();
        }
        var beforeDate = holdoutRequest.MaxDate;
        var afterDate = holdoutRequest.MinDate;
        var dateColumn = holdoutRequest.DateColumnName;
        var hasMinDate = false;
        var hasMaxDate = false;


        if (columnNames.Contains(dateColumn))
        {
            if (beforeDate.Date != DateTime.MinValue)
            {
                //has max date
                hasMaxDate = true;
            }
            if (afterDate.Date != DateTime.MinValue)
            {
                //has min date
                hasMinDate = true;
            }
        }

        if (hasMinDate || hasMaxDate)
        {
            foreach(DataRow row in _dataTable.Rows)
            {
                if (hasMaxDate && DateTime.Parse(row[dateColumn].ToString()) > beforeDate) {
                    row.Delete();
                }
                else if (hasMinDate && DateTime.Parse(row[dateColumn].ToString()) < afterDate)
                {
                    row.Delete();
                }
            }
        }
        _dataTable.DefaultView.Sort = HoldoutShuffle;
        _dataTable = _dataTable.DefaultView.ToTable();
        _dataTable.Columns.Remove(HoldoutShuffle);
        var rowCount = holdoutRequest.Count;
        var rows = _dataTable.Rows.Cast<DataRow>().Take(rowCount);
        if (holdoutRequest.IsPercent)
        {
            if (rowCount > 100)
            {
                rowCount = 100;
            }
            rowCount = (int)Math.Ceiling((float)_dataTable.Rows.Count / 100 * rowCount);
            rows = _dataTable.Rows.Cast<DataRow>().Take(rowCount);
        }

        var dataRows = rows as DataRow[] ?? rows.ToArray();
        if (dataRows.Length == 0)
        {
            Show("Holdout would be empty with current configuration. Will not create holdout.");
            return;
        }

        foreach (var row in dataRows)
        {
            sb.AppendLine(string.Join(",", row.ItemArray.Select(static field => field?.ToString())));
        }

        File.WriteAllText($"{holdoutRequest.Name}.csv", sb.ToString());
        var fi = new FileInfo($"{holdoutRequest.Name}.csv");

        var columns = _dataTable.Columns.Cast<DataColumn>().Select(c=>c.ColumnName).ToList();

        BasicActivator.SelectObject("Select an Extraction Identifier", columns.ToArray(), out var extractionIdentifier);
        if (extractionIdentifier == null)
            return;

        var db = SelectDatabase(true, "Select a Database to store the new Holdout.");
        if(db == null) return;

        var pipe = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(static p => p.ID)
        .FirstOrDefault(static p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));

        var importCommand = new ExecuteCommandCreateNewCatalogueByImportingFile(_activator, fi, extractionIdentifier, db, pipe, null,holdoutRequest.Description);
        importCommand.Execute();

    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => iconProvider.GetImage(RDMPConcept.CohortAggregate,OverlayKind.Link);
}
