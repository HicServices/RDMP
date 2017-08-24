using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueManager.LogViewer
{
    public delegate void NavigatePaneToEntityHandler(object sender, NavigatePaneToEntityArgs args);

    /// <summary>
    /// Used to issue navigation orders to LogViewerNavigationPane e.g. expand tree down to select TableLoadRun with ID 543
    /// </summary>
    public class NavigatePaneToEntityArgs
    {
        public LogViewerNavigationTarget EntityTarget { get; set; }
        public int EntityID { get; set; }

        public NavigatePaneToEntityArgs(LogViewerNavigationTarget entityTarget, int entityID)
        {
            EntityTarget = entityTarget;
            EntityID = entityID;
        }
    }
}
