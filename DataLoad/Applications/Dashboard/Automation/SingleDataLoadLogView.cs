// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using HIC.Logging;
using HIC.Logging.PastEvents;

namespace Dashboard.Automation
{
    /// <summary>
    /// Lists all the logging events in the Logging Database associated with a single DataLoadInfo run (e.g. a single data load from the DLE or a single DQE run etc).
    /// </summary>
    public partial class SingleDataLoadLogView : SingleDataLoadLogView_Design
    {
        private LogManager _logManager;
        private int _dataLoadInfoID;

        public SingleDataLoadLogView()
        {
            InitializeComponent();
        }

        private void RefreshLog()
        {
            loadEventsTreeView1.ClearObjects();

            if(_dataLoadInfoID == 0)
                return;

            if(_logManager == null)
                return;

            ArchivalDataLoadInfo archival = _logManager.GetLoadStatusOf(_dataLoadInfoID);

            if(archival == null) 
                throw new KeyNotFoundException("Could not find DataLoadInfo with ID " + _dataLoadInfoID + " in Logging database.");

            loadEventsTreeView1.AddObjects(new[] { archival });

            loadEventsTreeView1.ExpandAll();
        }

        public override void SetDatabaseObject(IActivateItems activator, ExternalDatabaseServer databaseObject)
        {
            _logManager = new LogManager(databaseObject);
            RefreshLog();
        }

        public void ShowDataLoadRunID(int dataLoadRunID)
        {
            _dataLoadInfoID = dataLoadRunID;
            RefreshLog();
        }
    }
}