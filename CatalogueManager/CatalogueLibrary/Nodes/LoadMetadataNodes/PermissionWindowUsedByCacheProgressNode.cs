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
    public class PermissionWindowUsedByCacheProgressNode: IDeletableWithCustomMessage,ILockable
    {
        public CacheProgress CacheProgress { get; set; }
        public PermissionWindow PermissionWindow { get; private set; }
        public bool DirectionIsCacheToPermissionWindow { get; set; }

        public PermissionWindowUsedByCacheProgressNode(CacheProgress cacheProgress, PermissionWindow permissionWindow, bool directionIsCacheToPermissionWindow)
        {
            CacheProgress = cacheProgress;
            PermissionWindow = permissionWindow;
            DirectionIsCacheToPermissionWindow = directionIsCacheToPermissionWindow;
        }

        public override string ToString()
        {
            return DirectionIsCacheToPermissionWindow ? PermissionWindow.Name : CacheProgress.ToString();
        }

        public void DeleteInDatabase()
        {
            CacheProgress.PermissionWindow_ID = null;
            CacheProgress.SaveToDatabase();
        }

        public string GetDeleteMessage()
        {
            return "stop using a PermissionWindow with this CacheProgress";
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

        #region Equality Members
        protected bool Equals(PermissionWindowUsedByCacheProgressNode other)
        {
            return CacheProgress.Equals(other.CacheProgress) && PermissionWindow.Equals(other.PermissionWindow) && DirectionIsCacheToPermissionWindow.Equals(other.DirectionIsCacheToPermissionWindow);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PermissionWindowUsedByCacheProgressNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = CacheProgress.GetHashCode();
                hashCode = (hashCode*397) ^ PermissionWindow.GetHashCode();
                hashCode = (hashCode*397) ^ DirectionIsCacheToPermissionWindow.GetHashCode();
                return hashCode;
            }
        }
        #endregion

        public object GetImageObject()
        {
            return DirectionIsCacheToPermissionWindow ? PermissionWindow : (object)CacheProgress;
        }
    }
}
