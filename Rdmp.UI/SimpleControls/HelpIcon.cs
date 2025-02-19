// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TransparentHelpSystem;

namespace Rdmp.UI.SimpleControls;

/// <summary>
/// Hovering over this control displays helpful information that relates to a nearby control.
/// </summary>
public partial class HelpIcon : UserControl
{
    public const int MaxHoverTextLength = 150;

    /// <summary>
    /// Returns the text that will be displayed when the user hovers over the control (this may be truncated if the text provided to <see cref="SetHelpText"/> was very long)
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string HoverText { get; private set; }

    private string _title;
    private HelpWorkflow _workFlow;
    private string _originalHoverText;
    private ToolTip _tt;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool SuppressClick { get; set; }

    public HelpIcon()
    {
        InitializeComponent();
    }

    public void SetHelpText(string title, string hoverText, HelpWorkflow workflow = null)
    {
        _workFlow = workflow;
        _title = title;
        HoverText = hoverText;
        _originalHoverText = hoverText;
        Visible = !string.IsNullOrWhiteSpace(HoverText);

        HoverText = GetShortText(HoverText);

        //If TT is null create new tooltip
        _tt ??= new ToolTip
        {
            AutoPopDelay = 15000, // Warning! MSDN states this is Int32, but anything over 32767 will fail.
            ShowAlways = true,
            ToolTipTitle = _title,
            InitialDelay = 200,
            ReshowDelay = 200,
            UseAnimation = true
        };
        _tt.SetToolTip(this, HoverText);
        Cursor = Cursors.Hand;
    }

    public void ClearHelpText()
    {
        SetHelpText(null, null);
    }

    private string GetShortText(string hoverText)
    {
        if (string.IsNullOrWhiteSpace(HoverText))
            return null;

        if (hoverText.Length <= MaxHoverTextLength)
            return hoverText;

        //enforce a maximum of 150 characters
        return $"{hoverText[..(MaxHoverTextLength - 3)]}...";
    }

    private void HelpIcon_MouseClick(object sender, MouseEventArgs e)
    {
        if (!SuppressClick)
            if (_workFlow != null)
                _workFlow.Start(true);
            else if (_title != null && _originalHoverText != null)
                WideMessageBox.Show(_title, _originalHoverText, WideMessageBoxTheme.Help);
    }
}