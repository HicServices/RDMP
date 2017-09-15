using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Spontaneous;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Nodes.LoadMetadataNodes
{
    public class PermissionWindowUsedByCacheProgress: IDeleteable,ILockable
    {
        public CacheProgress CacheProgress { get; set; }
        public PermissionWindow PermissionWindow { get; private set; }

        public PermissionWindowUsedByCacheProgress(CacheProgress cacheProgress,PermissionWindow permissionWindow)
        {
            CacheProgress = cacheProgress;
            PermissionWindow = permissionWindow;
        }

        public override string ToString()
        {
            return PermissionWindow.Name;
        }

        public void DeleteInDatabase()
        {
            CacheProgress.PermissionWindow_ID = null;
            CacheProgress.SaveToDatabase();
        }

        public bool LockedBecauseRunning
        {
            get { return PermissionWindow.LockedBecauseRunning; }
            set { PermissionWindow.LockedBecauseRunning = value; }
        }

        public string LockHeldBy
        {
            get { return PermissionWindow.LockHeldBy; }
            set { PermissionWindow.LockHeldBy = value; }
        }
        public void Lock()
        {
            PermissionWindow.Lock();
        }

        public void Unlock()
        {
            PermissionWindow.Unlock();
        }

        public void RefreshLockPropertiesFromDatabase()
        {
            PermissionWindow.RefreshLockPropertiesFromDatabase();
        }
    }
}
