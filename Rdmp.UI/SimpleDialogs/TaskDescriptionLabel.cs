// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.UI.SimpleDialogs;

public partial class TaskDescriptionLabel : UserControl
{
    private Color _backColour;
    private Color _foreColour;

    public TaskDescriptionLabel()
    {
        InitializeComponent();
    }

    public void SetupFor(DialogArgs args)
    {
        var task = args.TaskDescription;
        var entryLabel = args.EntryLabel;


        tbTaskDescription.Visible = pnlTaskDescription.Visible = !string.IsNullOrWhiteSpace(task);
        tbTaskDescription.Text = task;

        tbEntryLabel.Visible = pnlEntryLabel.Visible = !string.IsNullOrWhiteSpace(entryLabel);

        if (entryLabel is { Length: > WideMessageBox.MAX_LENGTH_BODY })
            entryLabel = entryLabel[..WideMessageBox.MAX_LENGTH_BODY];

        // set prompt text. If there's a TaskDescription too then leave a bit of extra space
        tbEntryLabel.Text = entryLabel;

        Height = (!string.IsNullOrWhiteSpace(entryLabel) ? tbEntryLabel.Height : 0) +
                 (!string.IsNullOrWhiteSpace(task) ? tbTaskDescription.Height : 0);

        //Switch style based on args.DesciptionSeverity
        switch (args.DesciptionSeverity)
        {
            case ProgressEventType.Warning:
                _backColour = Color.FromArgb(253, 248, 228);
                _foreColour = Color.FromArgb(134, 105, 53);
                break;
            case ProgressEventType.Error:
                _backColour = Color.FromArgb(242, 222, 223);
                _foreColour = Color.FromArgb(143, 58, 75);
                break;
            //Default blue information colours
            default:
                _backColour = Color.FromArgb(217, 236, 242);
                _foreColour = Color.FromArgb(44, 108, 128);
                break;
        }

        pnlTaskDescriptionBorder.BackColor = _backColour;
        tbTaskDescription.BackColor = _backColour;
        tbTaskDescription.ForeColor = _foreColour;
    }

    /// <summary>
    /// Returns the width this control would ideally like to take up
    /// </summary>
    public int PreferredWidth => Math.Max(tbEntryLabel.Width, tbTaskDescription.Width);

    public int PreferredHeight => Height;

    private void textBox1_Resize(object sender, EventArgs e)
    {
        var MessageSize = tbTaskDescription.CreateGraphics()
            .MeasureString(tbTaskDescription.Text,
                tbTaskDescription.Font,
                tbTaskDescription.Width,
                new StringFormat(0));

        tbTaskDescription.Height = (int)MessageSize.Height + 3;
        pnlTaskDescriptionBorder.Height = tbTaskDescription.Height + 20;
        pnlTaskDescription.Height = pnlTopMargin.Height + pnlTaskDescriptionBorder.Height;
    }

    private void tbEntryLabel_Resize(object sender, EventArgs e)
    {
        var MessageSize = tbEntryLabel.CreateGraphics()
            .MeasureString(tbEntryLabel.Text,
                tbEntryLabel.Font,
                tbEntryLabel.Width,
                new StringFormat(0));

        tbEntryLabel.Height = (int)MessageSize.Height + 3;
        pnlEntryLabel.Height = pnlTopMargin.Height + tbEntryLabel.Height;
    }
}