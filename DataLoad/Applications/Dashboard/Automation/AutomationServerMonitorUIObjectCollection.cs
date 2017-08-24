using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Dashboarding;
using MapsDirectlyToDatabaseTable;

namespace Dashboard.Automation
{
    public class AutomationServerMonitorUIObjectCollection : IPersistableObjectCollection
    {
        
        public PersistStringHelper Helper { get; private set; }
        public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }

        public AutomationServiceException[] GlobalServiceLevelExceptions { get; private set; }
        public AutomationJob[] AutomationJobs { get; private set; }

        public AutomationServerMonitorUIObjectCollection()
        {
            DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
            GlobalServiceLevelExceptions = new AutomationServiceException[0];
            AutomationJobs = new AutomationJob[0];
        }

        public string SaveExtraText()
        {
            return "";
        }

        public void LoadExtraText(string s)
        {
            
        }

        public void SetServer(AutomationServiceSlot selected)
        {
            DatabaseObjects.Clear();
            
            if(selected != null)
                DatabaseObjects.Add(selected);

            GlobalServiceLevelExceptions = new AutomationServiceException[0];
            AutomationJobs = new AutomationJob[0];
        }

        public AutomationServiceSlot GetServerIfAny()
        {
            return DatabaseObjects.OfType<AutomationServiceSlot>().SingleOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if there were appreciable changes</returns>
        public bool UpdateServerStateIfAny()
        {
            var server = GetServerIfAny();
            if(server != null)
            {
                var beforeHeldBy = server.LockHeldBy;
                var beforeLocked = server.LockedBecauseRunning;
                
                var beforeExceptions = GlobalServiceLevelExceptions;
                var beforeJobs = AutomationJobs;

                server.RefreshLockPropertiesFromDatabase();
                server.RefreshLifelinePropertyFromDatabase();

                GlobalServiceLevelExceptions = server.Repository.GetAllObjects<AutomationServiceException>().Where(a=>string.IsNullOrWhiteSpace(a.Explanation)).OrderByDescending(e=>e.EventDate).ToArray();
                
                //do not take more than 100!
                AutomationJobs = server.GetAutomationJobs(100).ToArray();

                
                return
                    beforeHeldBy != server.LockHeldBy ||
                    beforeLocked != server.LockedBecauseRunning ||
                    beforeExceptions.Length != GlobalServiceLevelExceptions.Length ||
                    JobDifferences(beforeJobs,AutomationJobs);
            }

            return false;
        }

        private bool JobDifferences(AutomationJob[] beforeJobs, AutomationJob[] automationJobs)
        {
            //different length
            if (beforeJobs.Length != automationJobs.Length)
                return true;

            for (int i = 0; i < beforeJobs.Length; i++)
            {
                var before = beforeJobs[i];
                var after = automationJobs[i];

                //checks for equals via ID and type (are they the same record)
                if (!before.Equals(after))
                    return true;

                if (before.LastKnownStatus != after.LastKnownStatus)
                    return true;

                if (before.CancelRequested != after.CancelRequested)
                    return true;

                if (before.AutomationJobType != after.AutomationJobType)
                    return true;

                if (before.DataLoadRunID != after.DataLoadRunID)
                    return true;

                if (before.Description != after.Description)
                    return true;
            }

            return false;
        }
    }
}