using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking
{
    public class ContentWindowTracker
    {
        List<RDMPSingleControlTab>  TrackedWindows = new List<RDMPSingleControlTab>();
        List<DockContent> TrackedAdhocWindows = new List<DockContent>();

        public ContentWindowTracker()
        {
            
        }

        public void AddWindow(RDMPSingleControlTab window)
        {
            var singleObjectUI = window as PersistableSingleDatabaseObjectDockContent;

            if(singleObjectUI != null)
                if(AlreadyActive(singleObjectUI.GetControl().GetType(),singleObjectUI.DatabaseObject))
                    throw new ArgumentOutOfRangeException("Cannot create another window for object " + singleObjectUI.DatabaseObject + " of type " + singleObjectUI.GetControl().GetType() + " because there is already a window active for that object/window type");
            
            TrackedWindows.Add(window);

            window.FormClosed += (s,e)=>Remove(window);
        }

        public void AddAdhocWindow(DockContent adhocWindow)
        {
            TrackedAdhocWindows.Add(adhocWindow);
            adhocWindow.FormClosed += (s, e) => TrackedAdhocWindows.Remove(adhocWindow);
        }

        private void Remove(RDMPSingleControlTab window)
        {
            TrackedWindows.Remove(window);
        }

        public PersistableSingleDatabaseObjectDockContent GetActiveWindowIfAnyFor(Type windowType, IMapsDirectlyToDatabaseTable databaseObject)
        {
            return TrackedWindows.OfType<PersistableSingleDatabaseObjectDockContent>().SingleOrDefault(t => t.GetControl().GetType() == windowType && t.DatabaseObject.Equals(databaseObject));
        }

        public bool AlreadyActive(Type windowType, IMapsDirectlyToDatabaseTable databaseObject)
        {
            return TrackedWindows.OfType<PersistableSingleDatabaseObjectDockContent>().Any(t => t.GetControl().GetType() == windowType && t.DatabaseObject.Equals(databaseObject));
        }

        public void CloseAllWindows()
        {
            foreach (var trackedWindow in TrackedWindows.ToArray())
                trackedWindow.Close();

            foreach (var adhoc in TrackedAdhocWindows.ToArray())
                adhoc.Close();
        }

        public void CloseAllButThis(DockContent content)
        {
            var trackedWindowsToClose = TrackedWindows.ToArray().Where(t => t != content);

            foreach (var trackedWindow in trackedWindowsToClose)
                trackedWindow.Close();
            
            foreach (var adhoc in TrackedAdhocWindows.ToArray().Where(t => t != content))
                adhoc.Close();
        }

    }
}
