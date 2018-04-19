using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;

namespace CatalogueLibrary.Spontaneous
{
    /// <summary>
    /// Spontaneous (non database persisted) version of PermissionWindow.  Use this class when you want to define a runtime only (in memory) window of execution for
    /// caching / loading etc.  SpontaneouslyInventedPermissionWindow are never locked.
    /// </summary>
    public class SpontaneouslyInventedPermissionWindow:SpontaneousObject,IPermissionWindow
    {
        private readonly ICacheProgress _cp;

        public SpontaneouslyInventedPermissionWindow(ICacheProgress cp):this()
        {
            _cp = cp;
            PermissionWindowPeriods = new List<PermissionWindowPeriod>();
        }

        public SpontaneouslyInventedPermissionWindow(ICacheProgress cp, List<PermissionWindowPeriod> windows):this()
        {
            _cp = cp;
            PermissionWindowPeriods = windows;
        }

        private SpontaneouslyInventedPermissionWindow()
        {
            RequiresSynchronousAccess = true;
            Name = "Spontaneous Permission Window";
        }

        public bool LockedBecauseRunning { get; set; }
        public string LockHeldBy { get; set; }
        public void Lock()
        {
            
        }

        public void Unlock()
        {
            
        }

        public void RefreshLockPropertiesFromDatabase()
        {
            
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool RequiresSynchronousAccess { get; set; }
        
        public List<PermissionWindowPeriod> PermissionWindowPeriods { get; private set; }
        public bool WithinPermissionWindow()
        {
            //if no periods then yeah its in the window yo
            if (PermissionWindowPeriods == null || !PermissionWindowPeriods.Any())
                return true;

            return PermissionWindowPeriods.Any(w => w.Contains(DateTime.Now, true));
        }

        public bool WithinPermissionWindow(DateTime dateTimeUTC)
        {
            return WithinPermissionWindow(DateTime.UtcNow);
        }

        public IEnumerable<ICacheProgress> CacheProgresses{get { return new[] {_cp}; }}

        public void SetPermissionWindowPeriods(List<PermissionWindowPeriod> windowPeriods)
        {
            PermissionWindowPeriods = windowPeriods;
        }
    }
}
