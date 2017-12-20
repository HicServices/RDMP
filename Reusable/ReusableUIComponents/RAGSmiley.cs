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
using ReusableUIComponents.ChecksUI;

namespace ReusableUIComponents
{
    /// <summary>
    /// Reusable component that indicates the success / warning / failure of a task in a nice user friendly way.  Green indicates success, yellow indicates a warning and red indicates 
    /// failure.  If there is an exception associated with a failure then clicking on the red face will show the Exception.
    /// </summary>
    public partial class RAGSmiley : UserControl,ICheckNotifier
    {
        private bool _alwaysShowHandCursor;
        
        public bool AlwaysShowHandCursor
        {
            get { return _alwaysShowHandCursor; }
            set
            {
                _alwaysShowHandCursor = value;
                SetCorrectCursor();
            }
        }

        public bool IsGreen()
        {
            return pbGreen.Visible;
        }

        public bool IsFatal()
        {
            return pbRed.Visible;
        }

        private void SetCorrectCursor()
        {
            if (AlwaysShowHandCursor || memoryCheckNotifier.Messages.Any())
                this.Cursor = Cursors.Hand;
            else
            if (pbYellow.Tag != null || pbRed.Tag != null)
                this.Cursor = Cursors.Hand;
            else
                this.Cursor = Cursors.Arrow;
        }

        public RAGSmiley()
        {
            InitializeComponent();

            BackColor = Color.Transparent;
            
            pbGreen.Visible = true;
            pbYellow.Visible = false;
            pbRed.Visible = false;
        }

        
        private void pb_Click(object sender, EventArgs e)
        {
            Exception tag = ((Control)sender).Tag as Exception;
            
            if (PopupMessagesIfAny(tag))
                return;

            if(tag != null)
                ExceptionViewer.Show(tag);
        }

        public void Warning(Exception ex)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(()=>Warning(ex)));
                return;
            }

            if(pbRed.Visible)
                return;

            //only change for novel values to prevent flickering
            if(!pbYellow.Visible)
            {
                pbGreen.Visible = false;
                pbYellow.Visible = true;
            }

            pbYellow.Tag = ex;
            SetCorrectCursor();
        }
        public void Fatal(Exception ex)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => Fatal(ex)));
                return;
            }
            pbGreen.Visible = false;
            pbYellow.Visible = false;
            pbRed.Visible = true;
            pbRed.Tag = ex;
            SetCorrectCursor();
        }

        

        public void Reset()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(Reset));
                return;
            }

            //reset the checks too so as not to leave old check results kicking about
            memoryCheckNotifier = new ToMemoryCheckNotifier();

            pbGreen.Visible = true;
            pbYellow.Visible = false;
            pbYellow.Tag = null;
            pbRed.Visible = false;
            pbRed.Tag = null;
            SetCorrectCursor();

        }

        private ToMemoryCheckNotifier memoryCheckNotifier = new ToMemoryCheckNotifier();
        private Task _checkTask;
        private object oTaskLock = new object();

        public bool OnCheckPerformed(CheckEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(()=>OnCheckPerformed(args)));
                return false;
            }

            
            //record in memory
            memoryCheckNotifier.OnCheckPerformed(args);

            ElevateState(args.Result);

            if(args.Ex!= null)
            {

                if (args.Result == CheckResult.Fail)
                    pbRed.Tag = args.Ex;
                else
                    pbYellow.Tag = args.Ex;
            }

            SetCorrectCursor();
            return false;
        }

        public void ElevateState(CheckResult result)
        {
            switch (result)
            {
                case CheckResult.Success:
                    break;
                case CheckResult.Warning:
                    Warning(null);
                    break;
                case CheckResult.Fail:
                    Fatal(null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool PopupMessagesIfAny(Exception tag)
        {
            if (memoryCheckNotifier.Messages.Any())
            {
                var popup = new PopupChecksUI("Record of events", false);
                new ReplayCheckable(memoryCheckNotifier).Check(popup);

                if(tag != null)
                    popup.OnCheckPerformed(new CheckEventArgs(tag.Message, CheckResult.Fail, tag));

                return true;
            }

            return false;
        }


        public void SetVisible(bool visible)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(()=> SetVisible(visible)));
                return;
            }
            BringToFront();
            Visible = visible;
        }

        public void StartChecking(ICheckable checkable)
        {
            lock (oTaskLock)
            {

                //if there is already a Task and it has not completed
                if (_checkTask != null && !_checkTask.IsCompleted)
                    return;

                //else start a new Task
                Reset();
                _checkTask = new Task(() =>
                {
                    try
                    {
                        checkable.Check(this);
                    }
                    catch (Exception ex)
                    {
                        Fatal(new Exception("Entire Checking Process Failed",ex));
                    }
                }
                    );
            _checkTask.Start();
            }
        }
    }
}
