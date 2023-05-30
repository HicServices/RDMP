// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class ToMemoryDataLoadJob : ToMemoryDataLoadEventListener, IDataLoadJob
{
    private List<NotifyEventArgs> _crashAtEnd = new ();
    /// <inheritdoc/>
    public IReadOnlyCollection<NotifyEventArgs> CrashAtEndMessages => _crashAtEnd.AsReadOnly();

    public ToMemoryDataLoadJob(bool throwOnErrorEvents = true): base(throwOnErrorEvents)
    {
    }

    public string Description { get; private set; }
    public IDataLoadInfo DataLoadInfo { get; private set; }
    public ILoadDirectory LoadDirectory { get; set; }
    public int JobID { get; set; }
    public ILoadMetadata LoadMetadata { get; private set; }
    public bool DisposeImmediately { get; private set; }
    public string ArchiveFilepath { get; private set; }
    public List<ITableInfo> RegularTablesToLoad { get; private set; } = new List<ITableInfo>();
    public List<ITableInfo> LookupTablesToLoad { get; private set; } = new List<ITableInfo>();
    public IRDMPPlatformRepositoryServiceLocator RepositoryLocator => null;

    public void StartLogging()
    {
    }

    public void CloseLogging()
    {
    }

    public HICDatabaseConfiguration Configuration { get; private set; }

    public object Payload { get; set; }
    public bool PersistentRaw { get; set; }

    public void CreateTablesInStage(DatabaseCloner cloner, LoadBubble stage)
    {
    }

    public void PushForDisposal(IDisposeAfterDataLoad disposeable)
    {
    }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }
    public ColumnInfo[] GetAllColumns()
    {
        return RegularTablesToLoad.SelectMany(t=>t.ColumnInfos).Union(LookupTablesToLoad.SelectMany(t=>t.ColumnInfos)).Distinct().ToArray();
    }
    /// <inheritdoc/>
    public void CrashAtEnd(NotifyEventArgs because)
    {
        _crashAtEnd.Add(because);
    }
}