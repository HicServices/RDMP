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