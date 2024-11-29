// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.Collections;
using Rdmp.UI.SimpleDialogs;
using Timer = System.Windows.Forms.Timer;

namespace Rdmp.UI.ChecksUI;

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
public partial class ChecksUI : UserControl, ICheckNotifier
{
    private Bitmap _tick;
    private Bitmap _warning;
    private Bitmap _warningEx;
    private Bitmap _fail;
    private Bitmap _failEx;

    private ConcurrentBag<CheckEventArgs> _results = new();
    private bool outOfDate;

            public ChecksUI()
    {
        InitializeComponent();
        olvChecks.ItemActivate += olvChecks_ItemActivate;
        olvResult.ImageGetter += ImageGetter;
        olvChecks.RowHeight = 19;

        _tick = ChecksAndProgressIcons.Tick.ImageToBitmap();
        _warning = ChecksAndProgressIcons.Warning.ImageToBitmap();
        _warningEx = ChecksAndProgressIcons.WarningEx.ImageToBitmap();
        _fail = ChecksAndProgressIcons.Fail.ImageToBitmap();
        _failEx = ChecksAndProgressIcons.FailEx.ImageToBitmap();

        olvChecks.PrimarySortOrder = SortOrder.Descending;

        olvChecks.UseFiltering = true;
        AllowsYesNoToAll = true;

        _timer = new Timer
        {
            Interval = 500
        };
        _timer.Tick += _timer_Tick;
        _timer.Start();

        if (!UserSettings.AutoResizeColumns)
        {
            RDMPCollectionCommonFunctionality.SetupColumnTracking(olvChecks, olvMessage,
                new Guid("5d62580d-2bee-420b-ab43-f40317769514"));
            RDMPCollectionCommonFunctionality.SetupColumnTracking(olvChecks, olvResult,
                new Guid("18b26ae1-c35d-4e73-9dc5-88f15910c1f9"));
            RDMPCollectionCommonFunctionality.SetupColumnTracking(olvChecks, olvEventDate,
                new Guid("28c13822-b4c0-4fa5-b20d-af612b076716"));
        }
    }

    private void _timer_Tick(object sender, EventArgs e)
    {
        if (IsDisposed)
        {
            _timer.Stop();
            _timer.Dispose();
            return;
        }

        if (outOfDate)
        {
            olvChecks.ClearObjects();
            olvChecks.AddObjects(_results);
            outOfDate = false;

            AutoResizeColumns();
        }
    }

    private void AutoResizeColumns()
    {
        if (UserSettings.AutoResizeColumns)
        {
            olvResult.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            olvEventDate.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            olvMessage.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
        }
    }

    private Bitmap ImageGetter(object rowObject)
    {
        return rowObject is not CheckEventArgs e
            ? null
            : e.Result switch
            {
                CheckResult.Success => _tick,
                CheckResult.Warning => e.Ex == null ? _warning : _warningEx,
                CheckResult.Fail => e.Ex == null ? _fail : _failEx,
                _ => throw new ArgumentOutOfRangeException()
            };
    }

    public bool CheckingInProgress { get; private set; }
    public bool AllowsYesNoToAll { get; set; }

    private Timer _timer;

    public event EventHandler<AllChecksCompleteHandlerArgs> AllChecksComplete;

    private Thread _checkingThread;
    private YesNoYesToAllDialog yesNoYesToAllDialog;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        yesNoYesToAllDialog = new YesNoYesToAllDialog();
    }

    public void StartChecking(ICheckable rootCheckable, bool bClearUI = true)
    {
        if (bClearUI) yesNoYesToAllDialog = new YesNoYesToAllDialog();

        if (CheckingInProgress)
        {
            MessageBox.Show(
                "Checking already in progress, please wait for current checks to complete before requesting more");
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
                listener.OnCheckPerformed(new CheckEventArgs("Entire checking process crashed", CheckResult.Fail, e));
                CheckingInProgress = false;

                AllChecksComplete?.Invoke(this, new AllChecksCompleteHandlerArgs(listener));
            }
        });
        _checkingThread.Start();
    }

    private void checker_AllChecksFinished(ToMemoryCheckNotifier listener)
    {
        _results.Add(new CheckEventArgs("All Checks Complete", CheckResult.Success));
        outOfDate = true;

        CheckingInProgress = false;

        AllChecksComplete?.Invoke(this, new AllChecksCompleteHandlerArgs(listener));
    }


    public bool OnCheckPerformed(CheckEventArgs args)
    {
        var shouldApplyFix = DoesUserWantToApplyFix(args);

        AddToListbox(shouldApplyFix
            ? new CheckEventArgs($"Fix will be applied for message:{args.Message}", CheckResult.Warning, args.Ex)
            : args);

        return shouldApplyFix;
    }

    private object olockYesNoToAll = new();

    private bool DoesUserWantToApplyFix(CheckEventArgs args)
    {
        if (InvokeRequired)
            lock (olockYesNoToAll)
            {
                return Invoke(() => DoesUserWantToApplyFix(args));
            }

        //if there is a fix and a request handler for whether or not to apply the fix
        if (args.ProposedFix == null) return false;
        if (args.Result == CheckResult.Success)
            throw new Exception(
                $"Why did you propose the fix {args.ProposedFix} when there is was no problem (don't specify a proposedFix if you are passing in CheckResult.Success)");

        //there is a suggested fix, see if the user has subscribed to the fix handler (i.e. the fix handler tells the class whether the user wants to apply this specific fix, like maybe a messagebox or something gets shown and it returns true to apply the fix)
        return MakeChangePopup.ShowYesNoMessageBoxToApplyFix(AllowsYesNoToAll ? yesNoYesToAllDialog : null,
            args.Message, args.ProposedFix);
    }

    private void AddToListbox(CheckEventArgs args)
    {
        _results.Add(args);
        outOfDate = true;
    }

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        olvChecks.ModelFilter = new TextMatchFilter(olvChecks, tbFilter.Text);
    }

    public void Clear()
    {
        _results.Clear();
        olvChecks.ClearObjects();
        yesNoYesToAllDialog = new YesNoYesToAllDialog();
    }

    private void olvChecks_ItemActivate(object sender, EventArgs e)
    {
        if (olvChecks.SelectedObject is CheckEventArgs args)
            if (args.Ex != null)
                ExceptionViewer.Show(args.Message, args.Ex);
            else
                WideMessageBox.Show(args, false);
    }

    public void TerminateWithExtremePrejudice()
    {
        if (_checkingThread is { IsAlive: true })
#pragma warning disable SYSLIB0006 // Type or member is obsolete
            _checkingThread.Abort();
#pragma warning restore SYSLIB0006 // Type or member is obsolete

        btnAbortChecking.Enabled = false;
    }

    private void btnAbortChecking_Click(object sender, EventArgs e)
    {
        TerminateWithExtremePrejudice();
    }
}