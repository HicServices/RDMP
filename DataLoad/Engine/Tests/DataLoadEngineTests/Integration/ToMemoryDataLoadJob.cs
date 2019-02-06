// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.DatabaseManagement.Operations;
using DataLoadEngine.Job;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngineTests.Integration
{
    public class ToMemoryDataLoadJob : ToMemoryDataLoadEventListener, IDataLoadJob
    {
        public ToMemoryDataLoadJob(bool throwOnErrorEvents = true): base(throwOnErrorEvents)
        {
        }

        public string Description { get; private set; }
        public IDataLoadInfo DataLoadInfo { get; private set; }
        public IHICProjectDirectory HICProjectDirectory { get; set; }
        public int JobID { get; set; }
        public ILoadMetadata LoadMetadata { get; private set; }
        public bool DisposeImmediately { get; private set; }
        public string ArchiveFilepath { get; private set; }
        public List<ITableInfo> RegularTablesToLoad { get; private set; }
        public List<ITableInfo> LookupTablesToLoad { get; private set; }
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get { return null; }}

        public void StartLogging()
        {
        }

        public void CloseLogging()
        {
        }

        public HICDatabaseConfiguration Configuration { get; private set; }

        public object Payload { get; set; }

        public void CreateTablesInStage(DatabaseCloner cloner, LoadBubble stage)
        {
        }

        public void PushForDisposal(IDisposeAfterDataLoad disposeable)
        {
        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
        }
    }
}