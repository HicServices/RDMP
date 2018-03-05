using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace ReusableUIComponents
{
    /// <summary>
    /// Allows you to view the status of an ILockable object in the Catalogue Database.  For example an AutomationServiceSlot gets locked by the Automation server so that no other automation
    /// executables can be started up at the same time.  The control will show when the lock was established.
    /// 
    /// Sometimes an ILockable will hard crash (e.g. the someone trips over your automation servers power cable) in which case the lifeline graph will flat line (after 30 seconds or so).  Your
    /// slot will still be locked.  Pressing the 'Force Lock Reset' will unlock the slot and make it available for use again, only do this if you are sure the ILockable (e.g. the automation 
    /// executable) is no longer running.
    /// </summary>
    public partial class LockableUI : UserControl
    {
        private ILockable _lockable;

        public ILockable Lockable
        {
            get { return _lockable; }
            set
            {
                _lockable = value;
                UpdateLockPanel(value);
            }
        }

        public bool Poll { get; set; }

        public LockableUI()
        {
            InitializeComponent();
            timer1.Tick += timer1_Tick;
            timer1.Start();
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            if (Poll && _lockable != null)
            {
                try
                {
                    _lockable.RefreshLockPropertiesFromDatabase();
                    UpdateLockPanel(_lockable);
                }
                catch (ObjectDeletedException exception)
                {
                    _lockable = null;
                }
            }
        }

        private void btnUnlock_Click(object sender, EventArgs e)
        {
            _lockable.Unlock();
            UpdateLockPanel(_lockable);
        }

        public bool IsLocked()
        {
            if(_lockable == null)
                throw new NotSupportedException("ILockable has not been set yet");

            return !string.IsNullOrWhiteSpace(_lockable.LockHeldBy);
        }

        private void UpdateLockPanel(ILockable value)
        {
            groupBox2.Enabled = value != null;

            if(value != null)
                if (value.LockedBecauseRunning)
                    lblLockStatus.Text = "Lock in place, held by " + value.LockHeldBy;
                else
                    lblLockStatus.Text = "Not currently locked";
        }
    }
}
