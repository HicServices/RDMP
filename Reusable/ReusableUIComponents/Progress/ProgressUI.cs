using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace ReusableUIComponents.Progress
{
    /// <summary>
    /// There are two main event systems in play in the RDMP.  There is Checking and Progress.  Checking activities are tasks that should be supervised and can block asking the user
    /// whether or not a proposed fix to a problem should be applied (See ChecksUI).  Progress activities are messages only and can also include numerical update messages indicating 
    /// that progress is made towards a fixed number e.g. you could get 1000 messages over the course of an hour reporting how close towards a goal of 1,000,000 records a given task is.
    /// 
    /// This control handles progress messages.  For Checks event system see ChecksUI.
    /// 
    /// ProgressUI handles progress messages of numerical progress (in either records or kilobytes) by updating the datagrid.  Messages appear in the Notifications area and 
    /// function very similarly to ChecksUI (you can double click them to view the message/copy it / view stack traces etc).  Because classes can be quite enthusiastic about notifying 
    /// progress this control buffers all messages it receives and only updates the user interface once every 3s (this improves performance).  All date/times come from the buffered messages
    /// so there is no impact from the 3s refresh rate on those. 
    /// </summary>
    public partial class ProgressUI : UserControl, IDataLoadEventListener
    {

        DataTable progress = new DataTable();


        /// <summary>
        /// See HandleThrottlingForJob, basically if the message in a progress event changes over time we don;t want to spam the datagrid so instead we just note that there is a flood of distinct messages coming from a specific source
        /// </summary>
        private const int MaxNumberOfJobsAcceptableFromSenderBeforeThrottlingKicksIn = 5000;

        public ProgressUI()
        {
            InitializeComponent();
            dataGridView1.DataSource = progress;

            progress.Columns.Add("Job");
            progress.PrimaryKey = new []{progress.Columns[0]};

            progress.Columns.Add("Progress");
            progress.Columns.Add("Quantity");
            progress.Columns.Add("Processing Time");

            Timer t = new Timer();
            t.Interval = 3000;//every 3 seconds
            t.Tick += ProcessAndClearQueuedProgressMessages;
            t.Start();
        }
        

        public void Clear()
        {
            listView1.Items.Clear();
            progress.Rows.Clear();
        }

        private string FormatSender(object sender)
        {
            return sender != null ? sender.GetType().Name : "Unknown";
        }

        private string FormatTime(DateTime dt)
        {
            return dt.ToLongTimeString();
        }

        private void NotifyError(DateTime time, object sender, string message, Exception e, string stackTrace)
        {
            ListViewItem i = new ListViewItem("", "Fail");
            i.SubItems.Add(FormatTime(time));
            i.SubItems.Add(FormatSender(sender));
            i.SubItems.Add(message);
            i.Tag = stackTrace;

            ListViewItem.ListViewSubItem item = i.SubItems.Add(e != null ? e.ToString() : "none");

            if (e != null)
            {
                item.Tag = e;
                i.ImageKey = "FailedWithException";
            }
            else
                item.Tag = stackTrace;

            listView1.Items.Add(i);
            listView1.Items[listView1.Items.Count - 1].EnsureVisible();

            foreach (ColumnHeader column in listView1.Columns)
                column.Width = -2; //magical (apparently it resizes to max width of content or header)
        }

        private void NotifyInformation(DateTime time, object sender, string message, string stackTrace)
        {
            ListViewItem i = new ListViewItem("", "Information");
            i.SubItems.Add(FormatTime(time));
            i.SubItems.Add(FormatSender(sender));
            i.SubItems.Add(message);
            i.SubItems.Add("none");
            i.Tag = stackTrace;

            listView1.Items.Add(i);
            listView1.Items[listView1.Items.Count - 1].EnsureVisible();

            foreach (ColumnHeader column in listView1.Columns)
                column.Width = -2; //magical (apparently it resizes to max width of content or header)
        }

        private void NotifyWarning(DateTime time,object sender, string message, Exception e, string stackTrace)
        {
            ListViewItem i = new ListViewItem("", e == null?"Warning":"WarningWithException");
            i.SubItems.Add(FormatTime(time));
            i.SubItems.Add(FormatSender(sender));
            i.SubItems.Add(message);
            i.Tag = stackTrace;

            ListViewItem.ListViewSubItem item = i.SubItems.Add(e != null ? e.ToString() : "none");

            if (e != null)
                item.Tag = e;

            listView1.Items.Add(i);
            listView1.Items[listView1.Items.Count - 1].EnsureVisible();


            foreach (ColumnHeader column in listView1.Columns)
                column.Width = -2; //magical (apparently it resizes to max width of content or header)
        }


        Dictionary<object, HashSet<string>> JobsreceivedFromSender = new Dictionary<object, HashSet<string>>();
        
        object oProgressQueLock = new object();
        Dictionary<string, QueuedProgressMessage> ProgressQueue = new Dictionary<string, QueuedProgressMessage>();


        object oNotifyQueLock = new object();
        List<QueuedNotifyMessage> NotificationQueue = new List<QueuedNotifyMessage>();
        private List<object> blackListedSenders = new List<object>();

        public void Progress(object sender,ProgressEventArgs args)
        {
            lock (oProgressQueLock)
            {
                //we have received an update to this message 
                if (ProgressQueue.ContainsKey(args.TaskDescription))
                {
                    ProgressQueue[args.TaskDescription].DateTime = DateTime.Now;
                    ProgressQueue[args.TaskDescription].ProgressEventArgs = args;
                    ProgressQueue[args.TaskDescription].Sender = sender;
                }
                else
                    ProgressQueue.Add(args.TaskDescription,new QueuedProgressMessage(DateTime.Now, args, sender));
            }
        }
        private void Notify(object sender, NotifyEventArgs notifyEventArgs)
        {

            lock (oNotifyQueLock)
            {
                NotificationQueue.Add(new QueuedNotifyMessage(DateTime.Now,notifyEventArgs,sender));
            }

            
        }
        void ProcessAndClearQueuedProgressMessages(object sender, EventArgs e)
        {
            lock (oProgressQueLock)
            {
                while(ProgressQueue.Any())
                {
                    KeyValuePair<string, QueuedProgressMessage> message = ProgressQueue.First();
                    var args = message.Value.ProgressEventArgs;
                    
                    string label = "";
                    switch (args.Progress.UnitOfMeasurement)
                    {
                        case ProgressType.Records:
                            label = "records";
                            break;
                        case ProgressType.Kilobytes:
                            label = "KB";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("type");
                    }

                    bool handledByFlood = HandleFloodOfMessagesFromJob(message.Value.Sender, args.TaskDescription, args.Progress.Value, label);
                        
                    if(!handledByFlood)
                        if (!progress.Rows.Contains(args.TaskDescription))
                        {
                            progress.Rows.Add(new object[] { args.TaskDescription, args.Progress.Value, label });
                        }
                        else
                        {
                            progress.Rows.Find(args.TaskDescription)["Progress"] = args.Progress.Value;
                            progress.Rows.Find(args.TaskDescription)["Processing Time"] = args.TimeSpentProcessingSoFar;
                        }

                    ProgressQueue.Remove(message.Key);
                }
            }

            lock (oNotifyQueLock)
            {
                foreach (QueuedNotifyMessage args in NotificationQueue)
                {
                    switch (args.NotifyEventArgs.ProgressEventType)
                    {
                        case ProgressEventType.Information:
                            NotifyInformation(args.DateTime, args.Sender, args.NotifyEventArgs.Message, args.NotifyEventArgs.StackTrace);
                            break;
                        case ProgressEventType.Warning:
                            NotifyWarning(args.DateTime, args.Sender, args.NotifyEventArgs.Message, args.NotifyEventArgs.Exception, args.NotifyEventArgs.StackTrace);
                            break;
                        case ProgressEventType.Error:
                            NotifyError(args.DateTime, args.Sender, args.NotifyEventArgs.Message, args.NotifyEventArgs.Exception, args.NotifyEventArgs.StackTrace);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                foreach (IGrouping<object, QueuedNotifyMessage> spammers in NotificationQueue.GroupBy(f => f.Sender).Where(g => g.Count() > 500))
                {
                    NotifyError(DateTime.Now, spammers.Key, "Sender blacklisted for sending more than 300 messages in under 3 seconds!", null, null);
                    blackListedSenders.Add(spammers.Key);
                }

                NotificationQueue.Clear();
            }
        }
        private bool HandleFloodOfMessagesFromJob(object sender, string job,int progressAmount,string label)
        {
            
            //ensure we have a records of the sender
            if (!JobsreceivedFromSender.ContainsKey(sender))
                JobsreceivedFromSender.Add(sender, new HashSet<string>());

            //new job we have not seen before
            if (!JobsreceivedFromSender[sender].Contains(job))
                JobsreceivedFromSender[sender].Add(job); //add it - even if sender has a bad rep for sending lots of jobs

            //see if sender has been sending loads of unique job messages
            if (JobsreceivedFromSender[sender].Count > MaxNumberOfJobsAcceptableFromSenderBeforeThrottlingKicksIn) //it has told us about more than 5 jobs so far
            {
                MergeAllJobsForSenderIntoSingleRowAndCumulateResult(sender, job, progressAmount, label);
                return true;
            }

            return false;
        }

        private void MergeAllJobsForSenderIntoSingleRowAndCumulateResult(object sender  , string jobToAdd,int progressAmountToAdd, string label)
        {
            int startAtProgressAmount = 0;

            foreach (var jobsAlreadySeen in JobsreceivedFromSender[sender])
                if (progress.Rows.Contains(jobsAlreadySeen))
                {
                    startAtProgressAmount += Convert.ToInt32(progress.Rows.Find(jobsAlreadySeen)["Progress"]);
                    progress.Rows.Remove(progress.Rows.Find(jobsAlreadySeen)); //discard the flood of messages that might be in data table 
                    
                }

            int i = 1;
            try
            {
                for (; i < 500; i++)
                {
                    string startsWith = JobsreceivedFromSender[sender].First().Substring(0, i);

                    if (!JobsreceivedFromSender[sender].All(job => job.Substring(0,i).StartsWith(startsWith)))
                        break;
                }
            }
            catch (ArgumentException)
            {
                i = 1;//one of them is a dodgy length or empty length or otherwise they are sending us some dodgy messages
            }

            string floodJob;
            
            //no shared prefix
            if(i ==1)
                floodJob = sender + " FloodOfMessages";
            else
                floodJob = JobsreceivedFromSender[sender].First().Substring(0,i-1) + "... FloodOfMessages";
            
           //add a new row (or edit existing) for the flood of messages from sender
           if (progress.Rows.Contains(floodJob))
           {
               //update with progress
               progress.Rows.Find(floodJob)["Progress"] = Convert.ToInt32(progress.Rows.Find(floodJob)["Progress"]) + progressAmountToAdd;

           }
           else
            {
                progress.Rows.Add(new object[] { floodJob, startAtProgressAmount + progressAmountToAdd, label });
            }
           
        }

        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            //copy entire row to clipboard
            if (e.KeyCode == Keys.C && e.Control)
            {
                string text = "";
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                        text += subitem.Text + Environment.NewLine;
                    
                    text += Environment.NewLine;
                }


                Clipboard.SetText(text);
            }
        
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo listViewHitTestInfo = listView1.HitTest(e.Location);
            if (listViewHitTestInfo.Item != null)
            {
                Exception exception = listViewHitTestInfo.Item.SubItems[4].Tag as Exception;

                string stackTrace = listViewHitTestInfo.Item.Tag as string;

                if (exception != null)
                    ExceptionViewer.Show(listViewHitTestInfo.Item.SubItems[3].Text, exception,false);
                else
                    WideMessageBox.Show(listViewHitTestInfo.Item.SubItems[3].Text, environmentDotStackTrace: stackTrace,isModalDialog: false);
            }
        }


        public void OnProgress(object sender, ProgressEventArgs e)
        {
            Progress(sender, e);
        }
        public void OnNotify(object sender, NotifyEventArgs e)
        {
            if (blackListedSenders.Contains(sender))//sender was blacklisted for spamming messages
                return;
            
            Notify(sender,e);
        }
    }

    internal class QueuedNotifyMessage
    {
        public DateTime DateTime { get; set; }
        public NotifyEventArgs NotifyEventArgs { get; set; }
        public object Sender { get; set; }

        public QueuedNotifyMessage(DateTime dateTime, NotifyEventArgs notifyEventArgs, object sender)
        {
            DateTime = dateTime;
            NotifyEventArgs = notifyEventArgs;
            Sender = sender;
        }
    }

    internal class QueuedProgressMessage
    {
        public QueuedProgressMessage(DateTime dateTime, ProgressEventArgs progressEventArgs, object sender)
        {
            DateTime = dateTime;
            ProgressEventArgs = progressEventArgs;
            Sender = sender;
        }

        public DateTime DateTime { get; set; }
        public ProgressEventArgs ProgressEventArgs { get; set; }
        public object Sender { get; set; }
        
    }
}
