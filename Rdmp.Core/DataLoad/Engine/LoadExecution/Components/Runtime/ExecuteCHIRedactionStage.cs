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
                        var rc = new RedactedCHI(_job.RepositoryLocator.CatalogueRepository, foundChi, ExecuteCommandIdentifyCHIInCatalogue.WrapCHIInContext(foundChi, row[col].ToString(), 20), $"{tbl.GetFullyQualifiedName().Replace(_loadStage.ToString(), "")}.[{col.ColumnName}]"); //todo make sure this matches
                        rc.SaveToDatabase();
                        row[col] = row[col].ToString().Replace(foundChi, $"REDACTED_CHI_{rc.ID}");
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
