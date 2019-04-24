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
using FAnsi.Discovery;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job
{
    /// <summary>
    /// Empty implementation of IDataLoadJob that can be used during Checking / Tests etc 
    /// </summary>
    public class ThrowImmediatelyDataLoadJob: IDataLoadJob
    {
        private readonly IDataLoadEventListener _listener;

        public ThrowImmediatelyDataLoadJob()
        {
            _listener = new ThrowImmediatelyDataLoadEventListener();
        }

        public ThrowImmediatelyDataLoadJob(DiscoveredServer liveServer)
        {
            _listener = new ThrowImmediatelyDataLoadEventListener();
            Configuration = new HICDatabaseConfiguration(liveServer);
        }

        public ThrowImmediatelyDataLoadJob(IDataLoadEventListener listener)
        {
            _listener = listener;
        }
        public ThrowImmediatelyDataLoadJob(HICDatabaseConfiguration configuration, params TableInfo[] regularTablesToLoad)
        {
            _listener = new ThrowImmediatelyDataLoadEventListener();
            RegularTablesToLoad = new List<ITableInfo>(regularTablesToLoad);
            Configuration = configuration;
        }

        public string Description { get; private set; }
        public IDataLoadInfo DataLoadInfo { get; set; }
        public ILoadDirectory LoadDirectory { get; set; }
        public int JobID { get; set; }
        public ILoadMetadata LoadMetadata { get; private set; }
        public bool DisposeImmediately { get; private set; }
        public string ArchiveFilepath { get; private set; }
        public List<ITableInfo> RegularTablesToLoad { get; set; }
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

        public void AddForDisposalAfterCompletion(IDisposeAfterDataLoad disposable)
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
            _listener.OnNotify(sender,e);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            _listener.OnProgress(sender,e);
        }
    }
}