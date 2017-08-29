using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace ReusableUIComponents.ChecksUI
{
    /// <summary>
    /// There are two main event systems in play in the RDMP.  There is Checking and Progress.  Checking activities are tasks that should be supervised and can block asking the user
    /// whether or not a proposed fix to a problem should be applied (See ChecksUI).  Progress activities are messages only and can also include numerical update messages indicating 
    /// that progress is made towards a fixed number e.g. you could get 1000 messages over the course of an hour reporting how close towards a goal of 1,000,000 records a given task is.
    /// 
    /// This control covers the checking event system. For information about the progress system see ProgressUI.
    /// 
    /// Used throughout the RDMP software to inform the user about the progress or checking of an activity.  Messages will appear along with a result (Success,Fail,Warning) and optionally
    /// an Exception if one was generated.  Double clicking a message lets you view a StackTrace and even view the source code (See ViewSourceCodeDialog) where the message was generated 
    /// (even if it wasn't an Exception).
    /// 
    /// You can copy and paste values out of the listbox using Ctrl+C and Ctrl+V to paste.
    /// 
    /// Typing into the Filter lets you filter by message text.
    /// </summary>
    public partial  class ChecksUI : UserControl, ICheckNotifier
    {
        //list view items currently being hidden from the user due to filters
        private readonly List<ListViewItem> _hiddenItems = new List<ListViewItem>();
        
        //original order that the events were received from the source
        Dictionary<ListViewItem, int> orderDictionary = new Dictionary<ListViewItem,int>();

        public ChecksUI()
        {
            InitializeComponent();
        }

        public bool CheckingInProgress { get; private set; }

        public event AllChecksCompleteHandler AllChecksComplete;
        
        Thread _checkingThread;
        private YesNoYesToAllDialog yesNoYesToAllDialog = new YesNoYesToAllDialog();

        public void StartChecking(ICheckable rootCheckable, bool bClearUI =true)
        {
            if(bClearUI)
            {
                _hiddenItems.Clear();
                yesNoYesToAllDialog = new YesNoYesToAllDialog();
            }

            if (CheckingInProgress)
            {
                MessageBox.Show("Checking already in progress, please wait for current checks to complete before requesting more");
                return;
            }

            if (bClearUI)
                listView1.Items.Clear();

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

            ddFilterSeverity.DataSource = Enum.GetValues(typeof (CheckResult));
            ddFilterSeverity.SelectedItem = CheckResult.Success;
        }
        
        void checker_AllChecksFinished(ToMemoryCheckNotifier listener)
        {

            if (InvokeRequired && !IsDisposed)
            {
                Invoke(new MethodInvoker(() => checker_AllChecksFinished(listener)));
                return;
            }

            //let user know it's all done
            ListViewItem i = new ListViewItem("","Pass");
            i.SubItems.Add("All Checks Complete");
            listView1.Items.Add(i);
            orderDictionary.Add(i,int.MaxValue);

            //scroll to bottom of list view
            listView1.EnsureVisible(listView1.Items.Count - 1);

            CheckingInProgress = false;

            if(AllChecksComplete!= null)
                AllChecksComplete(this,new AllChecksCompleteHandlerArgs(listener));
        }


        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Control)
            {
                string text = "";

                foreach (ListViewItem item in listView1.SelectedItems)
                    text += GetTextFromItem(item) + Environment.NewLine;


                Clipboard.SetText(text);
            }
        }

        private string GetTextFromItem(ListViewItem item)
        {
            string toReturn = "";
            foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                toReturn += subitem.Text + Environment.NewLine;
            
            return toReturn;
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

            ListViewItem i;
            switch (args.Result)
            {
                case CheckResult.Success:
                    i = new ListViewItem("", "Pass");
                    break;
                case CheckResult.Fail:
                    i = new ListViewItem("", args.Ex == null ? "Fail" : "FailedWithException");
                    break;
                case CheckResult.Warning:
                    i = new ListViewItem("", args.Ex == null ? "Warning" : "WarningWithException");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("result");
            }
            i.Tag = args;

            i.SubItems.Add(args.Message);

            orderDictionary.Add(i, orderDictionary.Count);

            if (args.Ex != null)
            {
                //add the exception into the Tag of the subitem
                var item = i.SubItems.Add(args.Ex.ToString());
                item.Tag = args.Ex;
            }
            else
                i.SubItems.Add("none");

            listView1.Items.Add(i);

            foreach (ColumnHeader column in listView1.Columns)
                column.Width = -2;//magical (apparently it resizes to max width of content or header)

            //scroll to bottom of list view
            listView1.EnsureVisible(listView1.Items.Count - 1);
        }


        
        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            listView1.Visible = false;
            
            ApplyFilter();

            listView1.Visible = true;
        }

        private void ApplyFilter()
        {
            //make anything that was previously hidden visible again and clear the hidden list (reset)
            listView1.Items.AddRange(_hiddenItems.ToArray());
            _hiddenItems.Clear();
            
            //for each thing in the list
            foreach (var item in listView1.Items.Cast<ListViewItem>().ToArray())//to array prevents editing collection while in use error
            {
                //if it does not match the severity threshold, hide it
                if (item.Tag != null && ((CheckEventArgs)item.Tag).Result < _threshold)
                    HideItem(item);
                else
                //if there are search text criteria
                if (!string.IsNullOrWhiteSpace(tbFilter.Text))
                    //see if any text from the node matches the search criteria
                    if (!GetTextFromItem(item).ToUpper().Contains(tbFilter.Text.ToUpper()))
                        HideItem(item);
            }

            var ordered = listView1.Items.Cast<ListViewItem>().OrderBy(i => orderDictionary[i]).ToArray();
            listView1.Items.Clear();
            listView1.Items.AddRange(ordered);
        }

        private void HideItem(ListViewItem item)
        {
            _hiddenItems.Add(item);
            listView1.Items.Remove(item);
        }

        public void Clear()
        {
            _hiddenItems.Clear();
            listView1.Items.Clear();
            yesNoYesToAllDialog = new YesNoYesToAllDialog();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo listViewHitTestInfo = listView1.HitTest(e.Location);
            if (listViewHitTestInfo.Item != null)
            {
                var args = (CheckEventArgs)listViewHitTestInfo.Item.Tag;

                //user maybe double clicked on 'All Checks Complete'?
                if(args == null)
                    return;
                
                if (args.Ex != null)
                    ExceptionViewer.Show(listViewHitTestInfo.Item.SubItems[1].Text + Environment.NewLine + ExceptionHelper.ExceptionToListOfInnerMessages(args.Ex), args.Ex);
                else
                    WideMessageBox.Show(listViewHitTestInfo.Item.SubItems[1].Text,environmentDotStackTrace: args.StackTrace);
            }
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

        CheckResult _threshold = CheckResult.Success;
        private void ddFilterSeverity_SelectedIndexChanged(object sender, EventArgs e)
        {
            _threshold = (CheckResult) ddFilterSeverity.SelectedItem;
            ApplyFilter();
        }
    }
}
