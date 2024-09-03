// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

public abstract class MatchingTablesMutilatorWithDataLoadJob : IPluginMutilateDataTables
{
    private readonly LoadStage[] _allowedStages;

    [DemandsInitialization(
        "All tables matching this pattern which have a TableInfo defined in the load will be affected by this mutilation",
        DefaultValue = ".*")]
    public Regex TableRegexPattern { get; set; }

    [DemandsInitialization(
        "Overrides TableRegexPattern.  If this is set then the tables chosen will be mutilated instead")]
    public TableInfo[] OnlyTables { get; set; }

    [DemandsInitialization("How long to allow for each command to execute in seconds", DefaultValue = 600)]
    public int Timeout { get; set; }

    protected DiscoveredDatabase DbInfo;
    private LoadStage _loadStage;

    protected MatchingTablesMutilatorWithDataLoadJob(params LoadStage[] allowedStages)
    {
        _allowedStages = allowedStages;
    }

    public virtual void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        if (_allowedStages != null && !_allowedStages.Contains(loadStage))
            throw new NotSupportedException($"Mutilation {GetType()} is not allowed at stage {loadStage}");

        _loadStage = loadStage;
        DbInfo = dbInfo;
    }

    public ExitCodeType Mutilate(IDataLoadJob job)
    {
        if (TableRegexPattern != null)
            TableRegexPattern = new Regex(TableRegexPattern.ToString(), RegexOptions.IgnoreCase);

        foreach (var tableInfo in job.RegularTablesToLoad)
            if (OnlyTables != null && OnlyTables.Any())
            {
                if (OnlyTables.Contains(tableInfo))
                    FireMutilate(tableInfo, job);
            }
            else if (TableRegexPattern == null)
            {
                throw new Exception("You must specify either TableRegexPattern or OnlyTables");
            }
            else if (TableRegexPattern.IsMatch(tableInfo.GetRuntimeName()))
            {
                FireMutilate(tableInfo, job);
            }

        return ExitCodeType.Success;
    }

    private void FireMutilate(ITableInfo tableInfo, IDataLoadJob job)
    {
        var tbl = DbInfo.ExpectTable(tableInfo.GetRuntimeName(_loadStage, job.Configuration.DatabaseNamer));

        if (!tbl.Exists())
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"Expected table {tbl} did not exist in RAW"));
        }
        else
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"About to run {GetType()} mutilation on table {tbl}"));
            var sw = new Stopwatch();
            sw.Start();
            MutilateTable(job, tableInfo, tbl);
            sw.Stop();
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"{GetType()} mutilation on table {tbl} completed after {sw.ElapsedMilliseconds} ms"));
        }
    }

    protected abstract void MutilateTable(IDataLoadJob job, ITableInfo tableInfo, DiscoveredTable table);

    public virtual void Check(ICheckNotifier notifier)
    {
        if (TableRegexPattern == null && (OnlyTables == null || OnlyTables.Length == 0))
            notifier.OnCheckPerformed(new CheckEventArgs(
                "You must specify either a regex pattern (TableRegexPattern) or set OnlyTables for identifying tables which need to be processed",
                CheckResult.Fail));
    }
}