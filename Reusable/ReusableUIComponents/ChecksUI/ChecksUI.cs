using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using QuickGraph;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents.Icons;

namespace ReusableUIComponents.ChecksUI
{
    /// <summary>
    /// There are two main event systems in play in the RDMP.  There is Checking and Progress.  Checking activities are tasks that should be supervised and can block asking the user
    /// whether or not a proposed fix to a problem should be applied (See ChecksUI).  Progress activities are messages only and can also include numerical update messages indicating 
    /// that progress is made towards a fixed number e.g. you could get 1000 messages over the course of an hour reporting how close towards a goal of 1,000,000 records a given task is.
    /// 
    /// <para>This control covers the checking event system. For information about the progress system see ProgressUI.</para>
    /// 
    /// <para>Used throughout the RDMP software to inform the user about the progress or checking of an activity.  Messages will appear along with a result (Success,Fail,Warning) and optionally
    /// an Exception if one was generated.  Double clicking a message lets you view a StackTrace and even view the source code (See ViewSourceCodeDialog) where the message was generated 
    /// (even if it wasn't an Exception).</para>
    /// 
    /// <para>You can copy and paste values out of the listbox using Ctrl+C and Ctrl+V to paste.</para>
    /// 
    /// <para>Typing into the Filter lets you filter by message text.</para>
    /// </summary>
    public partial  class ChecksUI : UserControl, ICheckNotifier
    {
        private Bitmap _tick;
        private Bitmap _warning;
        private Bitmap _warningEx;
        private Bitmap _fail;
        private Bitmap _failEx;

        public ChecksUI()
        {
            InitializeComponent();
            olvChecks.ItemActivate += olvChecks_ItemActivate;
            olvResult.ImageGetter += ImageGetter;
            olvChecks.RowHeight = 19;

            _tick = ChecksAndProgressIcons.Tick;
            _warning = ChecksAndProgressIcons.Warning;
            _warningEx = ChecksAndProgressIcons.WarningEx;
            _fail = ChecksAndProgressIcons.Fail;
            _failEx = ChecksAndProgressIcons.FailEx;

            olvChecks.PrimarySortOrder = SortOrder.Descending;

            olvChecks.UseFiltering = true;
        }

        private object ImageGetter(object rowObject)
        {
            var e = (CheckEventArgs) rowObject;

            if(e != null)
                switch (e.Result)
                {
                    case CheckResult.Success:
                        return _tick;
                    case CheckResult.Warning:
                        return e.Ex == null ? _warning : _warningEx;
                    case CheckResult.Fail:
                        return e.Ex == null ? _fail : _failEx;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return null;
        }

        public bool CheckingInProgress { get; private set; }

        public event AllChecksCompleteHandler AllChecksComplete;
        
        Thread _checkingThread;
        private YesNoYesToAllDialog yesNoYesToAllDialog = new YesNoYesToAllDialog();
        

        public void StartChecking(ICheckable rootCheckable, bool bClearUI =true)
        {
            if(bClearUI)
            {
                yesNoYesToAllDialog = new YesNoYesToAllDialog();
            }

            if (CheckingInProgress)
            {
                MessageBox.Show("Checking already in progress, please wait for current checks to complete before requesting more");
                return;
            }

            if (bClearUI)
                olvChecks.ClearObjects();

            CheckingInProgress = true;
            btnAbortChecking.Enabled = true;
            var listener = new ToMemoryCheckNotifier(this);
            
            _checkingThread = new Thread(() =>
            {
                try
                {
                    rootCheckable.Check(listener);
                    checker_AllChecksFinished(listener);
                }
                catch (Exception e)
                {
                    
                    OnCheckPerformed(new CheckEventArgs("Entire checking process crashed",CheckResult.Fail, e));
                    CheckingInProgress = false;

                    if (AllChecksComplete != null)
                        AllChecksComplete(this, new AllChecksCompleteHandlerArgs(listener));
                }
            });
            _checkingThread.Start();
        }
        
        void checker_AllChecksFinished(ToMemoryCheckNotifier listener)
        {

            if (InvokeRequired && !IsDisposed)
            {
                Invoke(new MethodInvoker(() => checker_AllChecksFinished(listener)));
                return;
            }
            
            olvChecks.AddObject(new CheckEventArgs("All Checks Complete",CheckResult.Success));
            
            CheckingInProgress = false;

            if(AllChecksComplete!= null)
                AllChecksComplete(this,new AllChecksCompleteHandlerArgs(listener));
        }


        public bool OnCheckPerformed(CheckEventArgs args)
        {
            bool shouldApplyFix = DoesUserWantToApplyFix(args);

            AddToListbox(shouldApplyFix
                ? new CheckEventArgs("Fix will be applied for message:" + args.Message, CheckResult.Warning, args.Ex)
                : args);

            return shouldApplyFix;
        }

        private bool DoesUserWantToApplyFix(CheckEventArgs args)
        {
            //if there is a fix and a request handler for whether or not to apply the fix
            if (args.ProposedFix != null)
            {
                if (args.Result == CheckResult.Success)
                    throw new Exception("Why did you propose the fix " + args.ProposedFix + " when there is was no problem (dont specify a proposedFix if you are passing in CheckResult.Success)");

                //there is a suggested fix, see if the user has subscribed to the fix handler (i.e. the fix handler tells the class whether the user wants to apply this specific fix, like maybe a messagebox or something gets shown and it returns true to apply the fix)
                bool applyFix = MakeChangePopup.ShowYesNoMessageBoxToApplyFix(yesNoYesToAllDialog,args.Message, args.ProposedFix);//user wants to apply fix (or doesnt)

                //user wants to apply fix so don't raise any more events
                if (applyFix)
                    return true;
            }
            
            //do not apply fix
            return false;
        }

        

        private void AddToListbox(CheckEventArgs args)
        {
            if (InvokeRequired && !IsDisposed)
            {
                BeginInvoke(new MethodInvoker(() => AddToListbox(args)));
                return;
            }

            olvChecks.AddObject(args);
        }


        
        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            olvChecks.ModelFilter = new TextMatchFilter(olvChecks,tbFilter.Text);
        }

        public void Clear()
        {
            olvChecks.Items.Clear();
            yesNoYesToAllDialog = new YesNoYesToAllDialog();
        }

        void olvChecks_ItemActivate(object sender, EventArgs e)
        {
            var args = olvChecks.SelectedObject as CheckEventArgs;
            if (args != null)
                if (args.Ex != null)
                    ExceptionViewer.Show(args.Message+ Environment.NewLine + ExceptionHelper.ExceptionToListOfInnerMessages(args.Ex), args.Ex);
                else
                    WideMessageBox.Show(args.Message, environmentDotStackTrace: args.StackTrace);
        }

        public void TerminateWithExtremePrejudice()
        {
            if (_checkingThread != null && _checkingThread.IsAlive)
                _checkingThread.Abort();

            btnAbortChecking.Enabled = false;
        }

        private void btnAbortChecking_Click(object sender, EventArgs e)
        {
            TerminateWithExtremePrejudice();
        }
    }
}
