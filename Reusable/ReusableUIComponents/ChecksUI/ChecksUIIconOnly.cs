using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReusableLibraryCode.Checks;

namespace ReusableUIComponents.ChecksUI
{
    /// <summary>
    /// A miniature version of ChecksUI, only shows the worst message type that occurred during checking.  Double click the icon to launch a full
    /// ChecksUI (See ChecksUI).
    /// </summary>
    public partial class ChecksUIIconOnly : UserControl
    {
        private ICheckable _checkable;
        ToolTip tt = new ToolTip();

        public ChecksUIIconOnly()
        {
            InitializeComponent();
            
        }

        public void Check(ICheckable checkable)
        {
            //record the checkable
            _checkable = checkable;
            
            ToMemoryCheckNotifier notifier = new ToMemoryCheckNotifier();
            try
            {
                _checkable.Check(notifier);
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Entire checking process failed", CheckResult.Fail, e));
            }

            UpdateUI(notifier);

        }

        private void UpdateUI(ToMemoryCheckNotifier results)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => UpdateUI(results)));
                return;
            }

            tt.SetToolTip(pictureBox1, "Double click to rerun checks in popup");

            switch (results.GetWorst())
            {
                case CheckResult.Success:
                    pictureBox1.Image = imageList1.Images["Pass"];
                    break;
                case CheckResult.Warning:
                    pictureBox1.Image = imageList1.Images["Warning"];
                    break;
                case CheckResult.Fail:
                    pictureBox1.Image = imageList1.Images["Fail"];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            if(_checkable == null)
                return;

            PopupChecksUI popup = new PopupChecksUI("Checking " + _checkable,false);
            popup.Check(_checkable);
        }

        public Task BeginCheck(ICheckable checkable)
        {
            //it is ongoing already
            var task = new Task(()=>Check(checkable));
            task.Start();
            return task;
        }
    }
}
