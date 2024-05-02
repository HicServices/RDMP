// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Job;

/// <summary>
///     Empty implementation of IDataLoadJob that can be used during Checking / Tests etc
/// </summary>
public class ThrowImmediatelyDataLoadJob : IDataLoadJob
{
    private readonly IDataLoadEventListener _listener;

    public ThrowImmediatelyDataLoadJob()
    {
        _listener = ThrowImmediatelyDataLoadEventListener.Quiet;
    }

    public ThrowImmediatelyDataLoadJob(DiscoveredServer liveServer)
    {
        _listener = ThrowImmediatelyDataLoadEventListener.Quiet;
        Configuration = new HICDatabaseConfiguration(liveServer);
    }

    public ThrowImmediatelyDataLoadJob(IDataLoadEventListener listener)
    {
        _listener = listener;
    }

    public ThrowImmediatelyDataLoadJob(HICDatabaseConfiguration configuration, params ITableInfo[] regularTablesToLoad)
    {
        _listener = ThrowImmediatelyDataLoadEventListener.Quiet;
        RegularTablesToLoad = new List<ITableInfo>(regularTablesToLoad);
        Configuration = configuration;
    }

    public string Description { get; }
    public IDataLoadInfo DataLoadInfo { get; set; }
    public ILoadDirectory LoadDirectory { get; set; }
    public int JobID { get; set; }
    public ILoadMetadata LoadMetadata { get; set; }
    public bool DisposeImmediately { get; }
    public string ArchiveFilepath { get; }
    public List<ITableInfo> RegularTablesToLoad { get; set; } = new();
    public List<ITableInfo> LookupTablesToLoad { get; set; } = new();
    public IRDMPPlatformRepositoryServiceLocator RepositoryLocator => null;

    public void StartLogging()
    {
    }

    public void CloseLogging()
    {
    }

    public HICDatabaseConfiguration Configuration { get; set; }

    public object Payload { get; set; }
    public bool PersistentRaw { get; set; }

    private readonly List<NotifyEventArgs> _crashAtEnd = new();

    /// <inheritdoc />
    public IReadOnlyCollection<NotifyEventArgs> CrashAtEndMessages => _crashAtEnd.AsReadOnly();


    public static void AddForDisposalAfterCompletion(IDisposeAfterDataLoad disposable)
    {
    }

    public void CreateTablesInStage(DatabaseCloner cloner, LoadBubble stage)
    {
    }

    public void PushForDisposal(IDisposeAfterDataLoad disposeable)
    {
    }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    public void OnNotify(object sender, NotifyEventArgs e)
    {
        _listener.OnNotify(sender, e);
    }

    public void OnProgress(object sender, ProgressEventArgs e)
    {
        _listener.OnProgress(sender, e);
    }

    public ColumnInfo[] GetAllColumns()
    {
        return RegularTablesToLoad.SelectMany(t => t.ColumnInfos)
            .Union(LookupTablesToLoad.SelectMany(t => t.ColumnInfos)).Distinct().ToArray();
    }

    /// <inheritdoc />
    public void CrashAtEnd(NotifyEventArgs because)
    {
        _crashAtEnd.Add(because);
    }
}