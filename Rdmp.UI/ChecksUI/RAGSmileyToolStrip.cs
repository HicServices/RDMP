// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.ChecksUI;

/// <inheritdoc cref="IRAGSmiley" />
public partial class RAGSmileyToolStrip : ToolStripButton,  IRAGSmiley
{
    private readonly Control _host;
    private CheckResult _worst;
    private Exception _exception;
    private YesNoYesToAllDialog dialog;

    public RAGSmileyToolStrip(Control host)
    {
        _host = host;
        _worst = CheckResult.Success;

        //until first check is run
        Enabled = false;
        Text = "Checks";
        Image = _green;

        timer = new Timer();
        timer.Interval = 500;
        timer.Tick += T_Tick;
        timer.Start();
    }

    private void T_Tick(object sender, EventArgs e)
    {
        if(IsDisposed)
        {
            timer.Stop();
            timer.Dispose();
        }
        else
        {
            switch (_worst)
            {
                case CheckResult.Success:

                    Image = _green;
                    Tag = null;
                    break;

                case CheckResult.Warning:

                    Image = _yellow;
                    Tag = _exception;
                    Enabled = true;
                    break;

                case CheckResult.Fail:

                    Image = _red;
                    Tag = _exception;
                    Enabled = true;
                    break;
            }
        }
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

    private Bitmap _green = Images.TinyGreen.ImageToBitmap();
    private Bitmap _yellow = Images.TinyYellow.ImageToBitmap();
    private Bitmap _red = Images.TinyRed.ImageToBitmap();

    private ToMemoryCheckNotifier memoryCheckNotifier = new();
    private Task _checkTask;
    private object oTaskLock = new();
    private Timer timer;

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);

        var tag = Tag as Exception;

        if (PopupMessagesIfAny(tag))
            return;

        if (tag != null)
            ExceptionViewer.Show(tag);
    }

    public void Warning(Exception ex)
    {            
        if (IsFatal())
            return;

        _worst = CheckResult.Warning;
        _exception = ex;
    }


    public void Fatal(Exception ex)
    {
        _worst = CheckResult.Fail;
        _exception = ex;
    }

    public void Reset()
    {            
        //reset the checks too so as not to leave old check results kicking about
        memoryCheckNotifier = new ToMemoryCheckNotifier();
        _worst = CheckResult.Success;
        _exception = null;
    }

    public bool OnCheckPerformed(CheckEventArgs args)
    {            
        //record in memory
        memoryCheckNotifier.OnCheckPerformed(args);
            

        if (dialog != null)
        {
            if(!string.IsNullOrWhiteSpace(args.ProposedFix))
                if (dialog.ShowDialog($"Problem:{args.Message}\r\n\r\nFix:{args.ProposedFix}", "Apply Fix?") == DialogResult.Yes)
                {
                    ElevateState(CheckResult.Warning);
                    memoryCheckNotifier.OnCheckPerformed(new CheckEventArgs("Fix will be applied",CheckResult.Warning));
                    return true;
                }
        }

        ElevateState(args.Result);

        if (args.Ex != null)
            _exception = args.Ex;

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

            //if we have a tagged Exception that isn't included in the ToMemoryCheckNotifier we should show the user that one too
            if (tag != null && memoryCheckNotifier.Messages.All(m=>m.Ex != tag))
                popup.OnCheckPerformed(new CheckEventArgs(tag.Message, CheckResult.Fail, tag));

            return true;
        }

        return false;
    }

    public void StartChecking(ICheckable checkable)
    {
        lock (oTaskLock)
        {

            //if there is already a Task and it has not completed
            if (_checkTask is { IsCompleted: false })
                return;
                
            dialog = new YesNoYesToAllDialog();

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