// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SynthEHR;
using SynthEHR.Datasets;

namespace Rdmp.UI.SimpleDialogs.Reports;

/// <summary>
/// Part of GenerateTestDataUI  (See GenerateTestDataUI).  This control lets you decide how many records in the dataset to create.  This data is fictional although it is designed to look
/// semi real and exhibit peculiarities common to medical records.  The slider is exponential so if you drag it all the way to the top expect to wait for a weekend for it to generate
/// all the data.
/// </summary>
public partial class DataGeneratorUI : UserControl
{
    public DataGeneratorUI()
    {
        InitializeComponent();
        trackBar1.LargeChange = 1;
        cbGenerate.Checked = true;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IDataGenerator Generator
    {
        get => _generator;
        set
        {
            _generator = value;

            if (value != null)
                value.RowsGenerated += ValueOnRowsGenerated;

            cbGenerate.Text = value != null ? value.GetType().Name : "";
        }
    }

    public int GetSize() => 10 * (int)Math.Pow(10, trackBar1.Value);

    private int sizeAtBeginGeneration = -1;
    public Thread Thread;
    private IDataGenerator _generator;
    public event Action TrackBarMouseUp;
    public event Action Completed;

    public void BeginGeneration(IPersonCollection cohort, DirectoryInfo target)
    {
        //already running
        if (sizeAtBeginGeneration != -1)
            return;

        sizeAtBeginGeneration = GetSize();

        var fi = new FileInfo(Path.Combine(target.FullName, $"{Generator.GetType().Name}.csv"));

        Thread = new Thread(() => Generator.GenerateTestDataFile(cohort, fi, sizeAtBeginGeneration));
        Thread.Start();
    }

    private void ValueOnRowsGenerated(object sender, RowsGeneratedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(new MethodInvoker(() => ValueOnRowsGenerated(sender, e)));
            return;
        }

        var percentProgress = e.RowsWritten / (double)sizeAtBeginGeneration * 100.0;
        progressBar1.Value = (int)percentProgress;

        if (e.IsFinished) Completed?.Invoke();
    }

    private void trackBar1_MouseUp(object sender, MouseEventArgs e)
    {
        TrackBarMouseUp?.Invoke();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Generate
    {
        get => cbGenerate.Checked;
        set => cbGenerate.Checked = value;
    }

    private void CbGenerate_CheckedChanged(object sender, EventArgs e)
    {
        trackBar1.Enabled = Generate;
        progressBar1.Enabled = Generate;
    }
}