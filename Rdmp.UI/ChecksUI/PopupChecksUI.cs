// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.UI.ChecksUI;

/// <summary>
/// Popup dialog version of ChecksUI, See ChecksUI for description of functionality.
/// </summary>
public partial class PopupChecksUI : Form, ICheckNotifier
{
    public event EventHandler<AllChecksCompleteHandlerArgs> AllChecksComplete;

    public PopupChecksUI(string task, bool showOnlyWhenError)
    {
        InitializeComponent();
        Text = task;

        if (!showOnlyWhenError)
        {
            Show();
            haveDemandedVisibility = true;
        }
        else
            CreateHandle(); //let windows get a handle on the situation ;)

        KeyPreview = true;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.W))
        {
            Close();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private bool haveDemandedVisibility = false;
    private CheckResult _worstSeen = CheckResult.Success;

    public bool OnCheckPerformed(CheckEventArgs args)
    {
        if (_worstSeen < args.Result)
            _worstSeen = args.Result;

        if (args.Result == CheckResult.Fail || args.Result == CheckResult.Warning)
            if (!haveDemandedVisibility)
            {
                haveDemandedVisibility = true;
                Invoke(new MethodInvoker(Show));
            }

        return checksUI1.OnCheckPerformed(args);
    }

    protected override void OnClosed(EventArgs e)
    {
        if (haveDemandedVisibility)
            checksUI1.TerminateWithExtremePrejudice();

        base.OnClosed(e);
    }

    public void Check(ICheckable checkable)
    {
        try
        {
            checkable.Check(this);
        }
        catch (Exception ex)
        {
            checksUI1.OnCheckPerformed(new CheckEventArgs("Entire checking process failed", CheckResult.Fail, ex));
        }
    }

    public void StartChecking(ICheckable checkable)
    {
        Show();
        checksUI1.AllChecksComplete += AllChecksComplete;
        checksUI1.StartChecking(checkable);
    }

    public CheckResult GetWorst() => _worstSeen;
}