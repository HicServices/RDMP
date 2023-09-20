// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Windows.Forms;

namespace Rdmp.UI.SingleControlForms;

/// <summary>
/// TECHNICAL: Helper class that turns a Control into a Form by mounting it.  Also wires up IConsultableBeforeClosing to the Form Closing event if the hosted Control implements it.
/// </summary>
[TechnicalUI]
[System.ComponentModel.DesignerCategory("")]
public class SingleControlForm : Form
{
    public SingleControlForm(Control control, bool showOkButton = false)
    {
        SetClientSizeCore(control.Width, control.Height);
        Text = !string.IsNullOrWhiteSpace(control.Text) ? control.Text : control.Name;

        Controls.Add(control);
        control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        if (control is IConsultableBeforeClosing consult)
            FormClosing += consult.ConsultAboutClosing;

        if (showOkButton)
        {
            var okButton = new Button
            {
                Text = "Ok"
            };
            okButton.Click += (s, e) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnHeight = okButton.PreferredSize.Height;
            var btnWidth = okButton.PreferredSize.Width;

            Height += btnHeight;
            control.Height -= btnHeight;
            okButton.Location = new Point(ClientSize.Width / 2 - btnWidth / 2, ClientSize.Height - btnHeight);
            okButton.Anchor = AnchorStyles.Bottom;

            Controls.Add(okButton);
            okButton.BringToFront();
        }
    }

    public static DialogResult ShowDialog(Control control, bool showOkButton = false) =>
        new SingleControlForm(control, showOkButton).ShowDialog();
}