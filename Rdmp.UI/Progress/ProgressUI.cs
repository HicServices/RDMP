// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.Collections;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.Theme;
using ReusableLibraryCode.Progress;
using ReusableLibraryCode.Settings;

namespace Rdmp.UI.Progress
{
    /// <summary>
    /// There are two main event systems in play in the RDMP.  There is Checking and Progress.  Checking activities are tasks that should be supervised and can block asking the user
    /// whether or not a proposed fix to a problem should be applied (See ChecksUI).  Progress activities are messages only and can also include numerical update messages indicating 
    /// that progress is made towards a fixed number e.g. you could get 1000 messages over the course of an hour reporting how close towards a goal of 1,000,000 records a given task is.
    /// 
    /// <para>This control handles progress messages.  For Checks event system see ChecksUI.</para>
    /// 
    /// <para>ProgressUI handles progress messages of numerical progress (in either records or kilobytes) by updating the datagrid.  Messages appear in the Notifications area and 
    /// function very similarly to ChecksUI (you can double click them to view the message/copy it / view stack traces etc).  Because classes can be quite enthusiastic about notifying 
    /// progress this control buffers all messages it receives and only updates the user interface once every 3s (this improves performance).  All date/times come from the buffered messages
    /// so there is no impact from the 3s refresh rate on those. </para>
    /// </summary>
    public partial class ProgressUI : UserControl, IDataLoadEventListener
    {
        DataTable progress = new DataTable();

        /// <summary>
        /// Sender for all global errors that should never be filtered out of the <see cref="ProgressUI"/>
        /// </summary>
        public const string GlobalRunError = "Run Error";

        /// <summary>
        /// See HandleFloodOfMessagesFromJob, basically if the message in a progress event changes over time we don't want to spam the datagrid so instead we just note that there is a
        /// flood of distinct messages coming from a specific source component
        /// </summary>
        private const int MaxNumberOfJobsAcceptableFromSenderBeforeThrottlingKicksIn = 5000;

        private int _processingTimeColIndex;

        private Bitmap _information;
        private Bitmap _warning;
        private Bitmap _warningEx;
        private Bitmap _fail;
        private Bitmap _failEx;

        public ProgressUI()
        {
            InitializeComponent();
            dataGridView1.DataSource = progress;

            progress.Columns.Add("Job");
            progress.PrimaryKey = new []{progress.Columns[0]};

            progress.Columns.Add("Count",typeof(int));
            progress.Columns.Add("Unit");
            progress.Columns.Add("Processing Time",typeof(TimeSpan));

            Timer t = new Timer();
            t.Interval = 3000;//every 3 seconds
            t.Tick += ProcessAndClearQueuedProgressMessages;
            t.Start();

            DataGridViewCellStyle style = new DataGridViewCellStyle();
            style.Format = "N0";
            dataGridView1.Columns["Count"].DefaultCellStyle = style;

            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            _processingTimeColIndex = dataGridView1.Columns["Processing Time"].Index;

            _information = ChecksAndProgressIcons.Information;
            _warning = ChecksAndProgressIcons.Warning;
            _warningEx = ChecksAndProgressIcons.WarningEx;
            _fail = ChecksAndProgressIcons.Fail;
            _failEx = ChecksAndProgressIcons.FailEx;

            olvMessage.ImageGetter += ImageGetter;
            olvProgressEvents.ItemActivate += olvProgressEvents_ItemActivate;
            olvProgressEvents.UseFiltering = true;
            olvProgressEvents.Sorting = SortOrder.Descending;

            ddGroupBy.Items.Add("None");
            ddGroupBy.Items.Add(olvSender.Text);

            if (!UserSettings.AutoResizeColumns)
            {
                RDMPCollectionCommonFunctionality.SetupColumnTracking(olvProgressEvents, olvSender, new Guid("2731d3cb-703c-4743-96d9-f16abff1dbbf"));
                RDMPCollectionCommonFunctionality.SetupColumnTracking(olvProgressEvents, olvEventDate, new Guid("f3580392-e5b5-41d0-a1da-2751172d5517"));
                RDMPCollectionCommonFunctionality.SetupColumnTracking(olvProgressEvents, olvMessage, new Guid("d698faf6-2ff1-4f71-96e2-9a889c2e3f13"));
            }
        }

        public void ApplyTheme(ITheme theme)
        {
            theme.ApplyTo(toolStrip1);
        }

        private object ImageGetter(object rowObject)
        {
            var o = rowObject as ProgressUIEntry;

            if(o != null)
                switch (o.ProgressEventType)
                {
                    // TODO: draw a couple of new icons if required
                    case ProgressEventType.Debug:
                        return _information;
                    case ProgressEventType.Trace:
                        return _information;
                    case ProgressEventType.Information:
                        return _information;
                    case ProgressEventType.Warning:
                        return o.Exception == null ? _warning : _warningEx;
                    case ProgressEventType.Error:
                        return o.Exception == null ? _fail : _failEx;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return null;
        }

        void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if(e.ColumnIndex == _processingTimeColIndex)
                if (e.Value != null && e.Value != DBNull.Value)
                    e.Value = ((TimeSpan)e.Value).Hours.ToString("00") + ":" +
                           ((TimeSpan)e.Value).Minutes.ToString("00") + ":" +
                           ((TimeSpan)e.Value).Seconds.ToString("00");
        }


        public void Clear()
        {
            olvProgressEvents.ClearObjects();
            progress.Rows.Clear();
            
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Value = 0;

            lblCrashed.Visible = false;
            lblSuccess.Visible = false;
        }
        
        Dictionary<object, HashSet<string>> JobsreceivedFromSender = new Dictionary<object, HashSet<string>>();

        object oProgressQueLock = new object();
        Dictionary<string, QueuedProgressMessage> ProgressQueue = new Dictionary<string, QueuedProgressMessage>();


        object oNotifyQueLock = new object();
        List<ProgressUIEntry> NotificationQueue = new List<ProgressUIEntry>();

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
                NotificationQueue.Add(new ProgressUIEntry(sender,DateTime.Now, notifyEventArgs));
            }
        }

        public void ShowRunning(bool isRunning)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new MethodInvoker(() => ShowRunning(isRunning)));
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show(e);
                }
                return;
            }

            if (isRunning)
                progressBar1.Style = ProgressBarStyle.Marquee;
            else
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 1;
                progressBar1.Value = 1;
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
                            progress.Rows.Find(args.TaskDescription)["Count"] = args.Progress.Value;
                            progress.Rows.Find(args.TaskDescription)["Processing Time"] = args.TimeSpentProcessingSoFar;
                        }

                    ProgressQueue.Remove(message.Key);

                    AutoReizeColumns();
                }
            }

            lock (oNotifyQueLock)
            {
                if (NotificationQueue.Any())
                {
                    olvProgressEvents.AddObjects(NotificationQueue);
                    NotificationQueue.Clear();

                    AutoReizeColumns();
                }
            }

            olvProgressEvents.Sort();
        }

        private void AutoReizeColumns()
        {
            if (UserSettings.AutoResizeColumns)
            {
                olvSender.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                olvMessage.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                olvMessage.Width = olvMessage.Width + 15; //add room for icon
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
                    startAtProgressAmount += Convert.ToInt32(progress.Rows.Find(jobsAlreadySeen)["Count"]);
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
               progress.Rows.Find(floodJob)["Count"] = Convert.ToInt32(progress.Rows.Find(floodJob)["Count"]) + progressAmountToAdd;

           }
           else
            {
                progress.Rows.Add(new object[] { floodJob, startAtProgressAmount + progressAmountToAdd, label });
            }
           
        }

        void olvProgressEvents_ItemActivate(object sender, EventArgs e)
        {
            var model = olvProgressEvents.SelectedObject as ProgressUIEntry;

            if (model != null)
            {
                if (model.Exception != null)
                    ExceptionViewer.Show(model.Message, model.Exception, false);
                else
                    WideMessageBox.Show("Progress Message",model.Message, model.Args.StackTrace, false,theme:model.GetTheme());
            }
        }


        public void OnProgress(object sender, ProgressEventArgs e)
        {
            Progress(sender, e);
        }
        public void OnNotify(object sender, NotifyEventArgs e)
        {
            Notify(sender,e);
        }

        private void tbTopX_TextChanged(object sender, EventArgs e)
        {
            SetTopX();
        }

        private void SetTopX()
        {

            try
            {
                var topX = int.Parse(tbTopX.Text);
                if(topX <=0)
                    throw new Exception("Must be positive");

                olvProgressEvents.ListFilter = new TailFilter(topX);

                tbTopX.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbTopX.ForeColor = Color.Red;
            }
        }

        private void tbTextFilter_TextChanged(object sender, EventArgs e)
        {
            SetFilterFromTextBox();
        }

        

        private void ddGroupBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            var dd = ddGroupBy.SelectedItem as string;

            var c = olvProgressEvents.Columns.OfType<OLVColumn>().SingleOrDefault(col => col.Text.Equals(dd));

            olvProgressEvents.AlwaysGroupByColumn = c;
            olvProgressEvents.ShowGroups = c != null;
            olvProgressEvents.BuildGroups();
        }

        public void GroupBySender(string filter = null)
        {
            ddGroupBy.SelectedItem = "Sender";
            tbTextFilter.Text = filter;

            //clear the renderers filter so that we don't see yellow text highlighting all over the Sender column etc.
            var renderer = olvProgressEvents.DefaultRenderer as HighlightTextRenderer;
            if (renderer != null)
                renderer.Filter = null;
        }

        private void SetFilterFromTextBox()
        {
            var alwaysShow = olvProgressEvents.Objects == null ? new ProgressUIEntry[0] :
                olvProgressEvents.Objects.OfType<ProgressUIEntry>().Where(p => p.Sender == GlobalRunError).ToArray();
            olvProgressEvents.ModelFilter = new TextMatchFilterWithAlwaysShowList(alwaysShow, olvProgressEvents, tbTextFilter.Text, StringComparison.CurrentCultureIgnoreCase);
        }
        public void SetFatal()
        {
            lblCrashed.Visible = true;
            lblCrashed.BringToFront();
        }

        internal void SetSuccess()
        {
            lblSuccess.Visible = true;
            lblSuccess.BringToFront();
        }

        /// <summary>
        /// Returns the worst message recorded in the UI
        /// </summary>
        /// <returns></returns>
        public NotifyEventArgs GetWorst()
        {
            
            var worstEntry = (olvProgressEvents.Objects ?? new object[0]).OfType<ProgressUIEntry>().Union(NotificationQueue).OrderByDescending(e=>e.ProgressEventType).FirstOrDefault();
            
            if(worstEntry == null)
                return null;

            return new NotifyEventArgs(worstEntry.ProgressEventType,worstEntry.Message,worstEntry.Exception);
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
