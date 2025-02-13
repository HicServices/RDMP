// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Asks you if you want to carry out a particular activity with the option to say Yes to this activity or 'Yes to All' (activities that are similar to this one).
/// </summary>
[TechnicalUI]
public class YesNoYesToAllDialog : WideMessageBox
{
    private bool _yesToAllClicked;
    private bool _noToAllClicked;
    private readonly Lock _lockShowDialog = new();


    private FlowLayoutPanel p = new();
    private readonly Button _btnYes = new() { Text = "Yes" };
    private readonly Button _btnYesToAll = new() { Text = "Yes To All" };
    private readonly Button _btnNo = new() { Text = "No" };
    private readonly Button _btnNoToAll = new() { Text = "No To All" };

    /// <summary>
    /// The number of pixels to allow outside of the text width when auto sizing buttons
    /// </summary>
    private const int ButtonXPadding = 10;

    public YesNoYesToAllDialog() : this(new WideMessageBoxArgs("YesNo", "Unknown", Environment.StackTrace, null,
        WideMessageBoxTheme.Help))
    {
    }

    private YesNoYesToAllDialog(WideMessageBoxArgs wideMessageBoxArgs) : base(wideMessageBoxArgs)
    {
        AddButton(_btnYes);
        AddButton(_btnYesToAll);
        AddButton(_btnNo);
        AddButton(_btnNoToAll);

        p.Dock = DockStyle.Fill;

        ButtonsPanel.Controls.Clear();
        ButtonsPanel.Controls.Add(p);

        MinimumSize = new Size(600, 300);

        //start at no in case they close with X button
        DialogResult = DialogResult.No;
    }

    private void AddButton(Button button)
    {
        button.Click += btn_Click;
        button.Width = TextRenderer.MeasureText(button.Text, button.Font).Width + ButtonXPadding;
        p.Controls.Add(button);
    }

    private new DialogResult ShowDialog()
    {
        if (_yesToAllClicked)
            return DialogResult.Yes;
        if (_noToAllClicked)
            return DialogResult.No;

        return InvokeRequired ? Invoke(ShowDialog) : base.ShowDialog();
    }

    public DialogResult ShowDialog(string message, string caption)
    {
        if (InvokeRequired)
            return (DialogResult)Invoke(() => ShowDialog(message, caption));

        Args.Title = caption;
        Args.Message = message;
        Setup(Args);

        lock (_lockShowDialog)
        {
            return ShowDialog();
        }
    }

    private void btn_Click(object sender, EventArgs e)
    {
        if (sender == _btnYes)
        {
            DialogResult = DialogResult.Yes;
        }
        else if (sender == _btnNo)
        {
            DialogResult = DialogResult.No;
        }
        else if (sender == _btnYesToAll)
        {
            _yesToAllClicked = true;
            DialogResult = DialogResult.Yes;
        }
        else if (sender == _btnNoToAll)
        {
            _noToAllClicked = true;
            DialogResult = DialogResult.No;
        }

        Close();
    }
}