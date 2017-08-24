using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Reflection;
using ReusableUIComponents;

namespace CatalogueManager.SimpleDialogs.Automation
{
    /// <summary>
    /// Allows you to view all the currently locked entities in the Catalogue database.  This includes loads that are underway, caching permission windows that are executing, locked
    /// automation server slots etc (See AutomationServiceSlotManagement).  You shouldn't release these locks unless you are sure the locking process has terminated abnormally or
    /// otherwise failed to cleanup after itself properly and you want to force an unlock.
    /// 
    /// Clicking Unlock will force the lock to be released in the database although depending on your automation server settings it is possible that a given object will rapidly be
    /// relocked again e.g. if a scheduled load is due but a lingering lock exists on the LoadProgress.
    /// </summary>
    public partial class LockableOverviewUI : RDMPUserControl
    {
        

        public LockableOverviewUI()
        {
            InitializeComponent();

            olvUnlock.AspectGetter = (s) => "Unlock";
            olvUnlock.ButtonSizing = OLVColumn.ButtonSizingMode.CellBounds;
            olvLockables.ButtonClick += OlvLockablesOnButtonClick;
            olvName.ImageGetter += ImageGetter;
            olvType.AspectGetter += AspectGetterType;

            olvLockables.AlwaysGroupByColumn = olvType;
        }

        private object AspectGetterType(object rowObject)
        {
            return rowObject.GetType().Name;
        }

        private object ImageGetter(object rowObject)
        {
            return "padlockSquare.png";
        }

        protected override void OnLoad(System.EventArgs e)
        {
            if(VisualStudioDesignMode)
                return;

            base.OnLoad(e);

            RefreshUIFromDatabase();
        }

        private void OlvLockablesOnButtonClick(object sender, CellClickEventArgs cellClickEventArgs)
        {
            var lockable = cellClickEventArgs.Model as ILockable;
            
            if(lockable == null)
                return;

            lockable.Unlock();
            RefreshUIFromDatabase();
        }
        
        private Task _task;

        private void RefreshUIFromDatabase()
        {
            if(VisualStudioDesignMode || RepositoryLocator == null)
                return;

            //if there is already an ongoing Refresh don't execute another one
            if(_task != null && !_task.IsCompleted)
                return;

            _task = checksUIIconOnly1.BeginCheck(new DoesNotThrowCheckable(RefreshUIFromDatabaseRun));
            
        }

        private void RefreshUIFromDatabaseRun()
        {
            var newObjects = GetCurrentlyLockedObjects();
            
            //no current objects
            if (olvLockables.Objects == null)
                if (newObjects.Any())
                {
                    olvLockables.AddObjects(newObjects); //no current objects but some new objects
                    return;
                }
                else
                    return;//no new objects and no current objects

            //some current objects
            var currentObjects = olvLockables.Objects.Cast<ILockable>().ToArray();

            //if there is a change in lockableness objects (new ones or ones disapearing)
            if (currentObjects.Except(newObjects).Any() || newObjects.Except(currentObjects).Any())
            {
                //refresh it
                olvLockables.ClearObjects();
                olvLockables.AddObjects(newObjects);
            }
        }

        private ILockable[] GetCurrentlyLockedObjects()
        {
            var repo = RepositoryLocator.CatalogueRepository;

            List<ILockable> lockables = new List<ILockable>();

            lockables.AddRange(repo.GetAllObjects<PermissionWindow>("WHERE LockedBecauseRunning=1"));
            lockables.AddRange(repo.GetAllObjects<LoadProgress>("WHERE LockedBecauseRunning=1"));
            lockables.AddRange(repo.GetAllObjects<AutomationServiceSlot>("WHERE LockedBecauseRunning=1"));

            return lockables.ToArray();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshUIFromDatabase();
        }
    }

    
}
