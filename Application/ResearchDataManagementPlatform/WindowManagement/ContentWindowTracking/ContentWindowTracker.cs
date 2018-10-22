using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking
{
    /// <summary>
    /// Tracks which tabs are currently open in RDMP main application (to prevent activating the same object editing tab window twice etc).
    /// </summary>
    public class ContentWindowTracker
    {
        readonly List<RDMPSingleControlTab>  _trackedWindows = new List<RDMPSingleControlTab>();
        readonly List<DockContent> _trackedAdhocWindows = new List<DockContent>();

        /// <summary>
        /// Records the fact that a new single object editing tab has been opened.  .
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if another instance of the Control Type is already active with the same DatabaseObject</exception>
        /// <param name="window"></param>
        public void AddWindow(RDMPSingleControlTab window)
        {
            var singleObjectUI = window as PersistableSingleDatabaseObjectDockContent;

            if(singleObjectUI != null)
                if(AlreadyActive(singleObjectUI.GetControl().GetType(),singleObjectUI.DatabaseObject))
                    throw new ArgumentOutOfRangeException("Cannot create another window for object " + singleObjectUI.DatabaseObject + " of type " + singleObjectUI.GetControl().GetType() + " because there is already a window active for that object/window type");
            
            _trackedWindows.Add(window);

            window.FormClosed += (s,e)=>Remove(window);
        }

        /// <summary>
        /// Records the fact that a new impromptu/adhoc tab has been shown.  These windows are not checked for duplication.
        /// </summary>
        /// <param name="adhocWindow"></param>
        public void AddAdhocWindow(DockContent adhocWindow)
        {
            _trackedAdhocWindows.Add(adhocWindow);
            adhocWindow.FormClosed += (s, e) => _trackedAdhocWindows.Remove(adhocWindow);
        }

        private void Remove(RDMPSingleControlTab window)
        {
            _trackedWindows.Remove(window);
        }

        public PersistableSingleDatabaseObjectDockContent GetActiveWindowIfAnyFor(Type windowType, IMapsDirectlyToDatabaseTable databaseObject)
        {
            return _trackedWindows.OfType<PersistableSingleDatabaseObjectDockContent>().SingleOrDefault(t => t.GetControl().GetType() == windowType && t.DatabaseObject.Equals(databaseObject));
        }

        /// <summary>
        /// Check whether a given RDMPSingleControlTab is already showing with the given DatabaseObject (e.g. is user currently editing Catalogue bob in CatalogueTab)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <param name="windowType">A Type derrived from RDMPSingleControlTab</param>
        /// <param name="databaseObject">An instance of an object which matches the windowType</param>
        /// <returns></returns>
        public bool AlreadyActive(Type windowType, IMapsDirectlyToDatabaseTable databaseObject)
        {
            if (!typeof(IRDMPSingleDatabaseObjectControl).IsAssignableFrom(windowType))
                throw new ArgumentException("windowType must be a Type derrived from RDMPSingleControlTab");


            return _trackedWindows.OfType<PersistableSingleDatabaseObjectDockContent>().Any(t => t.GetControl().GetType() == windowType && t.DatabaseObject.Equals(databaseObject));
        }

        /// <summary>
        /// Closes all Tracked windows 
        /// </summary>
        /// <param name="tab"></param>
        public void CloseAllWindows(RDMPSingleControlTab tab)
        {
            if(tab != null)
            {
                CloseAllButThis(tab);
                tab.Close();
            }
            else
            {
                foreach (var trackedWindow in _trackedWindows.ToArray())
                    trackedWindow.Close();

                foreach (var adhoc in _trackedAdhocWindows.ToArray())
                    adhoc.Close();
            }
        }
        
        /// <summary>
        /// Closes all Tracked windows except the specified tab
        /// </summary>
        public void CloseAllButThis(DockContent content)
        {
            var trackedWindowsToClose = _trackedWindows.ToArray().Where(t => t != content);

            foreach (var trackedWindow in trackedWindowsToClose)
                CloseWindowIfInSameScope(trackedWindow, content);

            foreach (var adhoc in _trackedAdhocWindows.ToArray().Where(t => t != content))
                CloseWindowIfInSameScope(adhoc, content);
        }

        private void CloseWindowIfInSameScope(DockContent toClose, DockContent tabInSameScopeOrNull)
        {
            var parent = tabInSameScopeOrNull == null ? null : tabInSameScopeOrNull.Parent;

            if (toClose != null && (parent == null || toClose.Parent == parent))
                toClose.Close();
        }
    }
}
