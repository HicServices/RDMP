using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HIC.Logging.PastEvents;

namespace CatalogueManager.LogViewer
{
    public class LogViewerNavigateToCollection
    {
        public LogViewerNavigationTarget Target { get; set; }
        public int ParentID { get; set; }
        public IArchivalLoggingRecordOfPastEvent[] NavigationPaneChildren { get; set; }
        public int[] CollectionIDs;

        private readonly string _navigationPointName;

        public LogViewerNavigateToCollection(string navigationPointName, LogViewerNavigationTarget target, int parentID, int[] collectionIDs, IArchivalLoggingRecordOfPastEvent[] navigationPaneChildren = null)
        {
            
            Target = target;
            ParentID = parentID;
            CollectionIDs = collectionIDs;
            NavigationPaneChildren = navigationPaneChildren ?? new IArchivalLoggingRecordOfPastEvent[0];

            Array.Sort(NavigationPaneChildren);

            _navigationPointName = navigationPointName;
        }

        public override string ToString()
        {
            return _navigationPointName;
        }
    }

    public enum LogViewerNavigationTarget
    {
        DataLoadTasks,
        DataLoadRuns,
        ProgressMessages,
        FatalErrors,
        TableLoadRuns,
        DataSources

    }
}
