// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
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
///     Documents an ongoing load that is executing in the Data Load Engine.  This includes the load configuration
///     (LoadMetadata), Logging object (DataLoadInfo),
///     file system (LoadDirectory) etc.
/// </summary>
public interface IDataLoadJob : IDataLoadEventListener, IDisposeAfterDataLoad
{
    string Description { get; }
    IDataLoadInfo DataLoadInfo { get; }
    ILoadDirectory LoadDirectory { get; set; }
    int JobID { get; set; }
    ILoadMetadata LoadMetadata { get; }
    string ArchiveFilepath { get; }

    /// <summary>
    ///     Optional externally provided object to drive the data load.  For example if you have an explicit list of objects in
    ///     memory to process and
    ///     a custom Attacher which expects to be magically provided with this list then communicate the list to the Attacher
    ///     via this property.
    /// </summary>
    object Payload { get; set; }

    /// <summary>
    ///     Collection of all calls to <see cref="CrashAtEnd" />.  If there are any
    ///     of these at the end of the load they will be notified and a crash exit code will be
    ///     returned (but otherwise the load will complete normally).
    /// </summary>
    IReadOnlyCollection<NotifyEventArgs> CrashAtEndMessages { get; }

    List<ITableInfo> RegularTablesToLoad { get; }
    List<ITableInfo> LookupTablesToLoad { get; }

    IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

    void StartLogging();
    void CloseLogging();

    HICDatabaseConfiguration Configuration { get; }

    /// <summary>
    ///     True to automatically skip creating/dropping the RAW database
    /// </summary>
    bool PersistentRaw { get; set; }

    /// <summary>
    ///     Orders the job to create the tables it requires in the given stage (e.g. RAW/STAGING), the job will also take
    ///     ownership of the cloner for the purposes
    ///     of disposal (DO NOT DISPOSE OF CLONER YOURSELF)
    /// </summary>
    /// <param name="cloner"></param>
    /// <param name="stage"></param>
    void CreateTablesInStage(DatabaseCloner cloner, LoadBubble stage);

    void PushForDisposal(IDisposeAfterDataLoad disposeable);

    /// <summary>
    ///     Returns all <see cref="ColumnInfo" /> in <see cref="RegularTablesToLoad " /> and <see cref="LookupTablesToLoad" />
    /// </summary>
    /// <returns></returns>
    ColumnInfo[] GetAllColumns();

    /// <summary>
    ///     Call you see that something has gone horribly wrong but want to keep going
    ///     with the load anyway.  Once the load has been completed these will crash
    ///     the process and result in a non 0 exit code.  Archiving and postload operations
    ///     will still occur.
    /// </summary>
    /// <param name="because"></param>
    void CrashAtEnd(NotifyEventArgs because);
}