// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.DatabaseManagement.Operations;
using DataLoadEngine.LoadExecution.Components;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job
{
    /// <summary>
    /// See DataLoadJob
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
        /// Optional externally provided object to drive the data load.  For example if you have an explicit list of objects in memory to process and
        /// a custom Attacher which expects to be magically provided with this list then communicate the list to the Attacher via this property.
        /// </summary>
        object Payload { get; set; }

        List<ITableInfo> RegularTablesToLoad { get; }
        List<ITableInfo> LookupTablesToLoad { get; }
        
        IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

        void StartLogging();
        void CloseLogging();

        HICDatabaseConfiguration Configuration { get; }
        
        /// <summary>
        /// Orders the job to create the tables it requires in the given stage (e.g. RAW/STAGING), the job will also take ownership of the cloner for the purposes
        /// of disposal (DO NOT DISPOSE OF CLONER YOURSELF)
        /// </summary>
        /// <param name="cloner"></param>
        /// <param name="stage"></param>
        void CreateTablesInStage(DatabaseCloner cloner,LoadBubble stage);

        void PushForDisposal(IDisposeAfterDataLoad disposeable);
    }
}