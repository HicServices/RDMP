// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.using Amazon.Auth.AccessControlPolicy;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

internal class ExecuteCHIRedactionStage
{
    private readonly IDataLoadJob _job;
    private readonly DiscoveredDatabase _db;
    private readonly LoadStage _loadStage;
    private Dictionary<string, List<string>> _allowList;
    private bool _redact;

    public ExecuteCHIRedactionStage(IDataLoadJob job, DiscoveredDatabase db, LoadStage loadStage)
    {
        _job = job;
        _db = db;
        _loadStage = loadStage;
    }

    public ExitCodeType Execute(bool redact, Dictionary<string, List<string>> allowLists = null)
    {
        if (_loadStage != LoadStage.AdjustRaw && _loadStage != LoadStage.AdjustStaging)
            throw new NotSupportedException("This mutilator can only run in AdjustRaw or AdjustStaging");

        _allowList = allowLists;
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
        foreach (DataColumn col in dt.Columns)
        {
            if (_allowList.ContainsKey(col.ColumnName)) continue;
            foreach (DataRow row in dt.Rows)
            {
                var foundChi = CHIColumnFinder.GetPotentialCHI(row[col].ToString());
                if (!string.IsNullOrWhiteSpace(foundChi))
                {
                    if (_redact)
                    {
                        var replacementIdex = row[col].ToString().IndexOf(foundChi);
                        var foundTable = tbl.GetFullyQualifiedName().Replace(_loadStage.ToString(), "").Split("..")[1].Replace("[", "").Replace("]", "");
                        var catalogue = _job.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().Where(catalogue => catalogue.Name == foundTable).First();
                        var pkValue = "fake";
                        var pkColumnName = "Unknown";
                        if (catalogue != null)
                        {
                            //this can probably be tidied up
                            var pkColumnInfo = catalogue.CatalogueItems.Select(x => x.ColumnInfo).Where(x => x.IsPrimaryKey).First();
                            pkColumnName = pkColumnInfo.Name;
                            if (pkColumnInfo != null)
                            {
                                var pkName = pkColumnInfo.Name.Split(".").Last().Replace("[", "").Replace("]", "");
                                var arrayNames = (from DataColumn x
                                                  in dt.Columns.Cast<DataColumn>()
                                                  select x.ColumnName).ToList();
                                var index = arrayNames.IndexOf(pkName);
                                pkValue = row[index].ToString();
                            }
                        }
                        var ft = tbl.GetFullyQualifiedName().Replace(_loadStage.ToString(), "");
                        ft = ft.Replace("..", ".[dbo].");
                        var rc = new RedactedCHI(_job.RepositoryLocator.CatalogueRepository, foundChi, replacementIdex,ft, pkValue, pkColumnName.Split(".").Last(),$"[{col.ColumnName}]");
                        rc.SaveToDatabase();
                        var redactionString = "##########";
                        row[col] = row[col].ToString().Replace(foundChi, redactionString.Substring(0, Math.Min(foundChi.Length, redactionString.Length)));
                    }
                    else
                    {
                        _job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Found the CHI {foundChi} during the dataload"));
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
