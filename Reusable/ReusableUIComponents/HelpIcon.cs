// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ReusableUIComponents.Dialogs;
using ReusableUIComponents.TransparentHelpSystem;

namespace ReusableUIComponents
{
    /// <summary>
    /// Hovering over this control displays helpful information that relates to a nearby control.
    /// </summary>
    public partial class HelpIcon : UserControl
    {
        private string _hoverText;
        private string _title;
        private HelpWorkflow _workFlow;

        public HelpIcon()
        {
            InitializeComponent();
        }
        
        public void SetHelpText(string title, string hoverText, HelpWorkflow workflow = null)
        {
            _workFlow = workflow;
            _title = title;
            _hoverText = hoverText;
            Visible = !String.IsNullOrWhiteSpace(_hoverText);

            Regex rgx = new Regex("(.{150}\\s)");
            _hoverText = rgx.Replace(hoverText, "$1\n");

            Cursor = Cursors.Hand;
        }

        private void HelpIcon_MouseHover(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(_hoverText))
            {
                var tt = new ToolTip
                {
                    AutoPopDelay = 15000,  // Warning! MSDN states this is Int32, but anything over 32767 will fail.
                    ShowAlways = true,
                    ToolTipTitle = _title,
                    InitialDelay = 200,
                    ReshowDelay = 200,
                    UseAnimation = true
                };
                tt.SetToolTip(this, _hoverText);
            }

        }

        private void HelpIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (_workFlow != null)
                _workFlow.Start(true);
            else
                WideMessageBox.Show(_title, _hoverText, WideMessageBoxTheme.Help);
        }
    }
}
