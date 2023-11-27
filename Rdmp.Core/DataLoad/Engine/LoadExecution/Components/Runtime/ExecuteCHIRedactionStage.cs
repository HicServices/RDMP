using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

internal class ExecuteCHIRedactionStage
{
    private readonly IDataLoadJob _job;
    private readonly DiscoveredDatabase _db;
    private readonly LoadStage _loadStage;

    public ExecuteCHIRedactionStage(IDataLoadJob job, DiscoveredDatabase db, LoadStage loadStage)
    {
        _job = job;
        _db = db;
        _loadStage = loadStage;
    }

    public ExitCodeType Execute(bool redact, Dictionary<string, List<string>> _allowLists=null)
    {
        if (_loadStage != LoadStage.AdjustRaw && _loadStage != LoadStage.AdjustStaging)
            throw new NotSupportedException("This mutilator can only run in AdjustRaw or AdjustStaging");


        foreach(var tableInfo in _job.RegularTablesToLoad)
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
        //todo I have no if below does what I think ti does
        //attempting to copy from tbl to a dt then back to itself so that we can medify it
        var tempTbl = tbl.Database.ExpectTable(tbl.GetFullyQualifiedName());
        using var insert = tempTbl.BeginBulkInsert();
        insert.Upload(dt);
        tbl.BeginBulkInsert();
        tbl = tempTbl;
    }
}
