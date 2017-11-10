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
using ReusableLibraryCode.Progress;

namespace ReusableUIComponents.Progress
{
    public partial class ProgressBarsUI : UserControl,IDataLoadEventListener
    {
        Dictionary<string,ProgressBar> progressBars = new Dictionary<string, ProgressBar>();
        ToolTip tt = new ToolTip();

        public float EmSize = 9f;

        public ProgressBarsUI(string caption,bool showClose = false)
        {
            InitializeComponent();
            btnClose.Visible = showClose;
            lblTask.Text = caption;
        }

        public void Done()
        {
            btnClose.Enabled = true;

            foreach (var pb in progressBars.Values)
            {
                if (pb.Style == ProgressBarStyle.Marquee)
                {
                    pb.Style = ProgressBarStyle.Continuous;
                    pb.Maximum = 1;
                    pb.Value = 1;
                }
            }
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            ragSmiley1.OnCheckPerformed(e.ToCheckEventArgs());
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            if (progressBars.ContainsKey(e.TaskDescription))
                UpdateProgressBar(progressBars[e.TaskDescription], e);
            else
            {
                var y = GetRowYForNewProgressBar();

                Label lbl = new Label();
                lbl.Text = e.TaskDescription;
                lbl.Font = new Font(Font.FontFamily,EmSize);
                lbl.Location = new Point(0,y);
                Controls.Add(lbl);

                ProgressBar pb = new ProgressBar();
                pb.Location = new Point(lbl.Right,y);
                pb.Size = new Size(ragSmiley1.Left - lbl.Right,lbl.Height-2);
                pb.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                Controls.Add(pb);

                UpdateProgressBar(pb,e);

                progressBars.Add(e.TaskDescription,pb);
            }
        }

        private int GetRowYForNewProgressBar()
        {
            if (!progressBars.Any())
                return ragSmiley1.Bottom;

            return progressBars.Max(kvp => kvp.Value.Bottom);
        }

        private void UpdateProgressBar(ProgressBar progressBar, ProgressEventArgs progressEventArgs)
        {
            string text = progressEventArgs.Progress.Value + " " + progressEventArgs.Progress.UnitOfMeasurement;

            tt.SetToolTip(progressBar,text);

            if (progressEventArgs.Progress.KnownTargetValue != 0)
            {
                progressBar.Maximum = progressEventArgs.Progress.KnownTargetValue;
                progressBar.Value = Math.Min(progressBar.Maximum,progressEventArgs.Progress.Value);
                progressBar.Style = ProgressBarStyle.Continuous;
            }
            else
                progressBar.Style = ProgressBarStyle.Marquee;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if(ParentForm != null && ParentForm.IsHandleCreated)
                ParentForm.Close();
        }
    }
}
