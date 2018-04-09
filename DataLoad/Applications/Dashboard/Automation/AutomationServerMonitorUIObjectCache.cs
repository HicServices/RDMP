using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Dashboarding;
using MapsDirectlyToDatabaseTable;

namespace Dashboard.Automation
{
    /// <summary>
    /// Records the last known state of an AutomationServiceSlot (including all the jobs / exceptions).  Allows you to keep an up-to-date knowledge of the state
    /// of Automation database objects even though they are remotely updated by the automation service (which could be running on another computer).
    /// </summary>
    public class AutomationServerMonitorUIObjectCache
    {
        private readonly AutomationServiceSlot _slot;
        public AutomationServiceException[] GlobalServiceLevelExceptions { get; private set; }
        public AutomationJob[] AutomationJobs { get; private set; }

        public AutomationServerMonitorUIObjectCache(AutomationServiceSlot slot)
        {
            _slot = slot;
            GlobalServiceLevelExceptions = new AutomationServiceException[0];
            AutomationJobs = new AutomationJob[0];
        }

        public AutomationServiceSlot GetSlot()
        {
            return _slot;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if there were appreciable changes</returns>
        public bool UpdateServerStateIfAny()
        {
           var beforeHeldBy = _slot.LockHeldBy;
            var beforeLocked = _slot.LockedBecauseRunning;
                
            var beforeExceptions = GlobalServiceLevelExceptions;
            var beforeJobs = AutomationJobs;

            _slot.RefreshLockPropertiesFromDatabase();
            _slot.RefreshLifelinePropertyFromDatabase();

            GlobalServiceLevelExceptions = _slot.Repository.GetAllObjects<AutomationServiceException>().Where(a => string.IsNullOrWhiteSpace(a.Explanation)).OrderByDescending(e => e.EventDate).ToArray();
                
            //do not take more than 100!
            AutomationJobs = _slot.GetAutomationJobs(100).ToArray();

                
            return
                beforeHeldBy != _slot.LockHeldBy ||
                beforeLocked != _slot.LockedBecauseRunning ||
                beforeExceptions.Length != GlobalServiceLevelExceptions.Length ||
                JobDifferences(beforeJobs,AutomationJobs);
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
