// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.ChecksUI;

/// <inheritdoc cref="IRAGSmiley"/>
public partial class RAGSmiley : UserControl, IRAGSmiley
{
    private bool _alwaysShowHandCursor;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AlwaysShowHandCursor
    {
        get => _alwaysShowHandCursor;
        set
        {
            _alwaysShowHandCursor = value;
            SetCorrectCursor();
        }
    }

    public bool IsGreen() => pbGreen.Visible;

    public bool IsFatal() => pbRed.Visible;

    private void SetCorrectCursor()
    {
        if (AlwaysShowHandCursor || memoryCheckNotifier.Messages.Any())
            Cursor = Cursors.Hand;
        else if (pbYellow.Tag != null || pbRed.Tag != null)
            Cursor = Cursors.Hand;
        else
            Cursor = Cursors.Arrow;
    }

    public RAGSmiley()
    {
        InitializeComponent();

        BackColor = Color.Transparent;

        pbGreen.Visible = true;
        pbYellow.Visible = false;
        pbRed.Visible = false;

        _timer = new Timer
        {
            Interval = 500
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (IsDisposed)
        {
            _timer.Stop();
            _timer.Dispose();
            return;
        }

        switch (_state)
        {
            case CheckResult.Success:
                pbGreen.Visible = true;
                pbYellow.Visible = false;
                pbYellow.Tag = null;
                pbRed.Visible = false;
                pbRed.Tag = null;
                SetCorrectCursor();
                break;

            case CheckResult.Warning:

                //only change for novel values to prevent flickering
                if (!pbYellow.Visible)
                {
                    pbGreen.Visible = false;
                    pbYellow.Visible = true;
                }

                pbYellow.Tag = _exception;
                SetCorrectCursor();
                break;

            case CheckResult.Fail:

                pbGreen.Visible = false;
                pbYellow.Visible = false;
                pbRed.Visible = true;
                pbRed.Tag = _exception;
                SetCorrectCursor();
                break;
        }
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);

        pbGrey.Visible = !Enabled;
    }

    public void ShowMessagesIfAny()
    {
        pb_Click(this, EventArgs.Empty);
    }

    private void pb_Click(object sender, EventArgs e)
    {
        var tag = ((Control)sender).Tag as Exception ?? _exception;

        if (PopupMessagesIfAny(tag))
            return;

        if (tag != null)
            ExceptionViewer.Show(tag);
    }

    public void Warning(Exception ex)
    {
        if (_state == CheckResult.Fail) return;

        _state = CheckResult.Warning;
        _exception = ex;
    }

    public void Fatal(Exception ex)
    {
        _state = CheckResult.Fail;
        _exception = ex;
    }

    public void Reset()
    {
        //reset the checks too so as not to leave old check results kicking about
        memoryCheckNotifier = new ToMemoryCheckNotifier();
        _state = CheckResult.Success;
        _exception = null;
    }

    private ToMemoryCheckNotifier memoryCheckNotifier = new();
    private Task _checkTask;
    private object oTaskLock = new();
    private Timer _timer;
    private CheckResult _state;
    private Exception _exception;

    public bool OnCheckPerformed(CheckEventArgs args)
    {
        //record in memory
        memoryCheckNotifier.OnCheckPerformed(args);

        ElevateState(args.Result);

        if (args.Ex != null) _exception = args.Ex;

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
        if (InvokeRequired)
        {
            Invoke(new MethodInvoker(() => SetVisible(visible)));
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
            if (_checkTask is { IsCompleted: false })
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