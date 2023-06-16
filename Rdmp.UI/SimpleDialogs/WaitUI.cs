// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Rdmp.UI.SimpleDialogs;

public sealed partial class WaitUI : Form
{
    private readonly string _caption;
    private readonly Task _task;
    private readonly CancellationTokenSource _cancellationTokenSource;
    Timer timer = new();

    public WaitUI(string caption, Task task, CancellationTokenSource cancellationTokenSource)
    {
        _caption = caption;
        _task = task;
        _cancellationTokenSource = cancellationTokenSource;
        InitializeComponent();

        label1.Text = caption;
        SetClientSizeCore(pictureBox1.Width + label1.PreferredWidth + button1.Width + 10, button1.Height);

        button1.Left = label1.Right;
        timer.Interval = 500;
        timer.Start();
        timer.Tick += TimerTick;
    }

    private void TimerTick(object sender, EventArgs e)
    {
        if (_task.IsCompleted)
        {
            timer.Stop();
            Close();
        }
    }
        
    private void button1_Click(object sender, EventArgs e)
    {
        _cancellationTokenSource.Cancel();
        button1.Enabled = false;
    }
}