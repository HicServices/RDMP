// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

internal class ExecuteCHIRedactionStage
{
    private readonly IDataLoadJob _job;
    private readonly DiscoveredDatabase _db;
    private readonly LoadStage _loadStage;
    private Dictionary<string, List<String>> _allowLists = new();
    private bool _redact;
    private CHIRedactionHelpers _chiRedactionHelper = new CHIRedactionHelpers(null,null);

    public ExecuteCHIRedactionStage(IDataLoadJob job, DiscoveredDatabase db, LoadStage loadStage)
    {
        _job = job;
        _db = db;
        _loadStage = loadStage;
    }

    public ExitCodeType Execute(bool redact, Dictionary<string,List<String>> allowLists = null)
    {
        if (_loadStage != LoadStage.AdjustRaw && _loadStage != LoadStage.AdjustStaging)
            throw new NotSupportedException("This mutilator can only run in AdjustRaw or AdjustStaging");

        _allowLists = allowLists;
        _redact = redact;
        foreach (var tableInfo in _job.RegularTablesToLoad)
        {
            RedactCHIs(tableInfo);
        }
        return ExitCodeType.Success;
    }

    private void RedactCHIs(ITableInfo tableInfo)
    {
        var tbl = _db.ExpectTable(tableInfo.GetRuntimeName(_loadStage, _job.Configuration.DatabaseNamer));
        if (!tbl.Exists())
        {
            _job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"Expected table {tbl} did not exist in RAW"));
            return;
        }
        _job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
             $"About to run {GetType()} mutilation on table {tbl}"));
        var dt = tbl.GetDataTable();
        List<String> _allowList = new();
        if (_allowLists is not null && _allowLists.TryGetValue("RDMP_ALL", out var _extractionSpecificAllowances))
            _allowList.AddRange(_extractionSpecificAllowances);
        if (_allowLists is not null && _allowLists.TryGetValue(tbl.ToString(), out var _catalogueSpecificAllowances))
            _allowList.AddRange(_catalogueSpecificAllowances.ToList());
        foreach (DataColumn col in dt.Columns)
        {
            if (_allowList.Contains(col.ColumnName)) continue;
            foreach (DataRow row in dt.Rows)
            {
                var foundChi = CHIColumnFinder.GetPotentialCHI(row[col].ToString());
                if (!string.IsNullOrWhiteSpace(foundChi))
                {
                    if (_redact)
                    {
                        var replacementIdex = row[col].ToString().IndexOf(foundChi);
                        var foundTable = _chiRedactionHelper.StripEnclosingBrackets(tbl.GetFullyQualifiedName().Replace(_loadStage.ToString(), "").Split("..")[1]);
                        var catalogue = _job.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().Where(catalogue => catalogue.Name == foundTable).First();
                        var pkValue = "Unknown";
                        var pkColumnName = "Unknown";
                        if (catalogue != null)
                        {
                            var pkColumnInfo = catalogue.CatalogueItems.Select(x => x.ColumnInfo).Where(x => x.IsPrimaryKey && x.Name.Contains(foundTable)).First(); //there may be more, but we just need one
                            if (pkColumnInfo != null)
                            {
                                pkColumnName = pkColumnInfo.Name;
                                var pkName = _chiRedactionHelper.GetColumnNameFromColumnInfoName(pkColumnInfo.Name);
                                var arrayNames = (from DataColumn x
                                                  in dt.Columns.Cast<DataColumn>()
                                                  select x.ColumnName).ToList();
                                var index = arrayNames.IndexOf(pkName);
                                pkValue = row[index].ToString();
                            }
                        }
                        var ft = tbl.GetFullyQualifiedName();
                        if(_loadStage == LoadStage.AdjustRaw)
                        {
                            ft = ft.Replace("_RAW", "");
                        }
                        if (_loadStage == LoadStage.AdjustStaging)
                        {
                            ft = ft.Replace("_STAGING", "");
                        }
                        ft = ft.Replace("..", ".[dbo].");
                        if (_chiRedactionHelper.GetColumnNameFromColumnInfoName(pkColumnName) != $"{col.ColumnName}")
                        {
                            var rc = new RedactedCHI(_job.RepositoryLocator.CatalogueRepository, foundChi, replacementIdex, ft, pkValue, pkColumnName.Split(".").Last(), $"[{col.ColumnName}]");
                            rc.SaveToDatabase();
                            var redactionString = "##########";
                            row[col] = row[col].ToString().Replace(foundChi, redactionString[..Math.Min(foundChi.Length, redactionString.Length)]);
                        }
                    }
                    else
                    {
                        _job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, $"Found the CHI {foundChi} during the dataload"));
                    }
                }
            }
        }

        var conn = _db.Server.GetConnection();
        conn.Open();
        tbl.GetCommand($"DELETE FROM {tbl.GetRuntimeName()}", conn).ExecuteNonQuery();
        conn.Close();
        var insert = tbl.BeginBulkInsert();
        insert.Upload(dt);
    }
}
