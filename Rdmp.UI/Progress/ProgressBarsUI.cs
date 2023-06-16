// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.UI.Progress;

/// <summary>
/// Cut down version of ProgressUI which shows progress events as bars.  If a progress event has a known target number the bar will indicate progress otherwise
/// it will be a Marquee bar (one with a moving unknown progress animation).  All Notify events are displayed under the smiley face (or frowning if the process
/// has crashed)
/// </summary>
public partial class ProgressBarsUI : UserControl,IDataLoadEventListener
{
    private Dictionary<string,ProgressBar> progressBars = new();
    private ToolTip tt = new();

    public float EmSize = 9f;

    public ProgressBarsUI()
    {
        InitializeComponent();
    }
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
        if (InvokeRequired)
        {
            Invoke(new MethodInvoker(() => OnProgress(sender, e)));
            return;
        }

        if (progressBars.TryGetValue(e.TaskDescription, out var bar))
            UpdateProgressBar(bar, e);
        else
        {
            var y = GetRowYForNewProgressBar();

            var lbl = new Label
            {
                Text = e.TaskDescription,
                Font = new Font(Font.FontFamily, EmSize),
                Location = new Point(0, y)
            };
            Controls.Add(lbl);

            var pb = new ProgressBar
            {
                Location = new Point(lbl.Right, y),
                Size = new Size(ragSmiley1.Left - lbl.Right, lbl.Height - 2),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
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
        var text = $"{progressEventArgs.Progress.Value} {progressEventArgs.Progress.UnitOfMeasurement}";

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
        if(ParentForm is { IsHandleCreated: true })
            ParentForm.Close();
    }

    public void Clear()
    {
        //remove existing progress bars
        foreach (var pb in Controls.OfType<ProgressBar>().ToArray()) 
            Controls.Remove(pb);

        //clear our record of them
        progressBars.Clear();

        //reset the smiley
        ragSmiley1.Reset();
    }
}