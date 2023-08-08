// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rdmp.UI.TransparentHelpSystem;

/// <summary>
/// Describes why a given region is highlighted in TransparentHelpForm.  Describes what the user should do next and optionally can give the user alternate options to change the 
/// help path being taken.
/// </summary>
public partial class HelpBox : UserControl
{
    private readonly HelpWorkflow _workFlow;
    public const int FixedFormWidth = 400;

    public event Action OptionTaken;

    public HelpBox(HelpWorkflow workFlow, string text, string optionIfAny)
    {
        _workFlow = workFlow;
        InitializeComponent();

        if (workFlow == null && text == null && optionIfAny == null)
        {
            text = "Some useful text which will help guide the user to perform an activity";
            optionIfAny = "Some alternate option button the user can click";
        }

        lblHelp.Text = text;

        btnOption1.Text = optionIfAny;
        btnOption1.Visible = !string.IsNullOrWhiteSpace(optionIfAny);

        if (string.IsNullOrWhiteSpace(optionIfAny))
            //make label fill whole form
            lblHelp.Height = panel1.Height;

        btnOption1.Click += (s, e) => { OptionTaken?.Invoke(); };

        Size = GetSizeOfHelpBoxFor(text, !string.IsNullOrWhiteSpace(optionIfAny));
    }

    private Size GetSizeOfHelpBoxFor(string text, bool hasOptionButtons)
    {
        var basicSize = TextRenderer.MeasureText(text, SystemFonts.DefaultFont, new Size(FixedFormWidth, 0),
            TextFormatFlags.WordBreak);

        //if there's an option button and very little text then widen the box
        if (btnOption1.Visible)
            basicSize.Width = Math.Max(basicSize.Width, btnOption1.Width + 3);

        if (hasOptionButtons)
            basicSize.Height += 23;

        //this controls padding
        basicSize.Height += 15;
        basicSize.Width += 30; //padding + width of close button

        return basicSize;
    }

    private void btnCloseHelp_Click(object sender, EventArgs e)
    {
        _workFlow.Abandon();
    }
}