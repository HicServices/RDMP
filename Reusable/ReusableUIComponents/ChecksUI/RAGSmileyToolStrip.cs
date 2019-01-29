using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.Dialogs;

namespace ReusableUIComponents.ChecksUI
{
    /// <summary>
    /// Reusable component that indicates the success / warning / failure of a task in a nice user friendly way.  Green indicates success, yellow indicates a warning and red indicates 
    /// failure.  If there is an exception associated with a failure then clicking on the red face will show the Exception.
    /// </summary>
    public partial class RAGSmileyToolStrip : ToolStripButton,  IRAGSmiley
    {
        private readonly Control _host;
        private CheckResult _worst;

        public RAGSmileyToolStrip(Control host)
        {
            _host = host;
            _worst = CheckResult.Success;

            //until first check is run
            Enabled = false;
            Text = "Checks";
        }

        public bool IsGreen()
        {
            return _worst == CheckResult.Success;
        }

        public bool IsWarning()
        {
            return _worst == CheckResult.Warning;
        }

        public bool IsFatal()
        {
            return _worst == CheckResult.Fail;
        }

        private Bitmap _green = Images.TinyGreen;
        private Bitmap _yellow = Images.TinyYellow;
        private Bitmap _red = Images.TinyRed;

        private ToMemoryCheckNotifier memoryCheckNotifier = new ToMemoryCheckNotifier();
        private Task _checkTask;
        private object oTaskLock = new object();

        public RAGSmileyToolStrip()
        {
            BackColor = Color.Transparent;
            Image = _green;
        }
        
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            Exception tag = Tag as Exception;

            if (PopupMessagesIfAny(tag))
                return;

            if (tag != null)
                ExceptionViewer.Show(tag);
        }

        public void Warning(Exception ex)
        {
            if (_host.InvokeRequired)
            {
                _host.Invoke(new MethodInvoker(() => Warning(ex)));
                return;
            }
            
            if (IsFatal())
                return;

            _worst = CheckResult.Warning;
            Image = _yellow;

            Tag = ex;
            Enabled = true;
        }


        public void Fatal(Exception ex)
        {
            if (_host.InvokeRequired)
            {
                _host.Invoke(new MethodInvoker(() => Fatal(ex)));
                return;
            }

            _worst = CheckResult.Fail;
            Image = _red;
            Tag = ex;
            Enabled = true;
        }



        public void Reset()
        {
            if (_host.InvokeRequired)
            {
                _host.Invoke(new MethodInvoker(Reset));
                return;
            }
            
            //reset the checks too so as not to leave old check results kicking about
            memoryCheckNotifier = new ToMemoryCheckNotifier();

            _worst = CheckResult.Success;
            Image = _green;
            Enabled = false;
        }



        public bool OnCheckPerformed(CheckEventArgs args)
        {
            if (_host.InvokeRequired)
            {
                _host.Invoke(new MethodInvoker(() => OnCheckPerformed(args)));
                return false;
            }
            
            //record in memory
            memoryCheckNotifier.OnCheckPerformed(args);
            
            Enabled = true;

            ElevateState(args.Result);

            if (args.Ex != null)
                Tag = args.Ex;

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

                if (tag != null)
                    popup.OnCheckPerformed(new CheckEventArgs(tag.Message, CheckResult.Fail, tag));

                return true;
            }

            return false;
        }


        public void SetVisible(bool visible)
        {
            if (_host.InvokeRequired)
            {
                _host.Invoke(new MethodInvoker(() => SetVisible(visible)));
                return;
            }
            
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
                        Fatal(new Exception("Entire Checking Process Failed", ex));
                    }
                }
                    );
                _checkTask.Start();
            }
        }
    }
}