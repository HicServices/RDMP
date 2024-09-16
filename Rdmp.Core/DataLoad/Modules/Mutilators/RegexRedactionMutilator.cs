// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;
/// <summary>
/// Redacts an incoming data table 
/// </summary>
public class RegexRedactionMutilator : MatchingTablesMutilatorWithDataLoadJob
{
    [DemandsInitialization("the regex redaction configuration to use")]
    public RegexRedactionConfiguration redactionConfiguration { get; set; }

    [DemandsInitialization(
       "All Columns matching this pattern which have a ColumnInfo defined in the load will be affected by this mutilation",
       DefaultValue = ".*")]
    public Regex ColumnRegexPattern { get; set; }

    [DemandsInitialization(
        "Overrides ColumnRegexPattern.  If this is set then the columns chosen will be mutilated instead")]
    public ColumnInfo[] OnlyColumns { get; set; }

    private DiscoveredColumn[] _discoveredPKColumns;

    public RegexRedactionMutilator() : base([LoadStage.AdjustRaw, LoadStage.AdjustStaging]) { }

    private bool ColumnMatches(DiscoveredColumn column)
    {
        if (OnlyColumns is not null && OnlyColumns.Length > 0)
        {
            return OnlyColumns.Select(c => c.GetRuntimeName()).Contains(column.GetRuntimeName());
        }
        if (ColumnRegexPattern != null)
        {
            ColumnRegexPattern = new Regex(ColumnRegexPattern.ToString(), RegexOptions.IgnoreCase);
            return ColumnRegexPattern.IsMatch(column.GetRuntimeName());
        }
        return false;
    }

    protected override void MutilateTable(IDataLoadJob job, ITableInfo tableInfo, DiscoveredTable table)
    {
        DataTable redactionsToSaveTable = RegexRedactionHelper.GenerateRedactionsDataTable();
        DataTable pksToSave = RegexRedactionHelper.GeneratePKDataTable();

        var columns = table.DiscoverColumns();

        var relatedCatalogues = tableInfo.GetAllRelatedCatalogues();
        var cataloguePks = relatedCatalogues.SelectMany(c => c.CatalogueItems).Where(ci => ci.ColumnInfo.IsPrimaryKey).ToList();
        _discoveredPKColumns = columns.Where(c => cataloguePks.Select(cpk => cpk.Name).Contains(c.GetRuntimeName())).ToArray();
        if (_discoveredPKColumns.Length == 0)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "No Primary Keys found. Redaction cannot be perfomed without a primary key."));
            //Don't want to fail the data load, but just let the user know
            return;
        }
        var pkColumnInfos = cataloguePks.Select(c => c.ColumnInfo);
        foreach (var column in columns.Where(c => !pkColumnInfos.Select(c => c.GetRuntimeName()).Contains(c.GetRuntimeName())))
        {
            if (ColumnMatches(column))
            {
                var pkSeparator = pkColumnInfos.Count() > 0 ? "," : "";
                var sql = @$"
                    SELECT {column.GetRuntimeName()} {pkSeparator} {string.Join(", ", pkColumnInfos.Select(c => c.GetRuntimeName()))}
                    FROM {table.GetRuntimeName()}
                    WHERE {column.GetRuntimeName()} LIKE '%{redactionConfiguration.RegexPattern}%'
                    ";
                var dt = new DataTable();
                dt.BeginLoadData();
                var conn = table.Database.Server.GetConnection();
                conn.Open();
                using (var cmd = table.Database.Server.GetCommand(sql, conn))
                {
                    cmd.CommandTimeout = Timeout * 1000;
                    using var da = table.Database.Server.GetDataAdapter(cmd);
                    da.Fill(dt);
                }
                dt.EndLoadData();
                var redactionUpates = dt.Clone();
                var columnInfo = relatedCatalogues.SelectMany(c => c.CatalogueItems).ToArray().Select(ci => ci.ColumnInfo).Where(ci => ci.GetRuntimeName() == column.GetRuntimeName()).FirstOrDefault();
                if (columnInfo is null)
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Unable to find the related column info"));
                    return;
                }
                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        RegexRedactionHelper.Redact(columnInfo, row, cataloguePks, redactionConfiguration, redactionsToSaveTable, pksToSave, redactionUpates);
                    }
                    catch (Exception e)
                    {
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, $"{e.Message}"));

                    }
                }
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Regex Redaction mutilator found {dt.Rows.Count} redactions."));
                if (redactionsToSaveTable.Rows.Count == 0) return;
                for (int i = 0; i < pksToSave.Rows.Count; i++)
                {
                    pksToSave.Rows[i]["ID"] = i + 1;
                }
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Creating Temporary tables"));
                var t1 = table.Database.CreateTable(nameof(RegexRedactionHelper.Constants.pksToSave_Temp), pksToSave);
                var t2 = table.Database.CreateTable(nameof(RegexRedactionHelper.Constants.redactionsToSaveTable_Temp), redactionsToSaveTable);
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Saving Redactions"));
                var _server = relatedCatalogues.First().GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false);
                RegexRedactionHelper.SaveRedactions(job.RepositoryLocator.CatalogueRepository, t1, t2, _server, Timeout * 1000);
                t1.Drop();
                t2.Drop();
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Performing join update"));
                RegexRedactionHelper.DoJoinUpdate(columnInfo, table, table.Database.Server, redactionUpates, _discoveredPKColumns, Timeout * 1000);
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Regex Redactions tool found {dt.Rows.Count} redactions."));
            }
        }
    }
}
