// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;

namespace ReusableUIComponents
{
    /// <summary>
    /// Asks you if you want to carry out a particular activity with the option to say Yes to this activity or 'Yes to All' (activities that are similar to this one).
    /// </summary>
    [TechnicalUI]
    public partial class YesNoYesToAllDialog : Form
    {
        private bool YesToAllClicked = false;
        private bool NoToAllClicked = false;

        public YesNoYesToAllDialog()
        {
            InitializeComponent(); 
        }

        new private DialogResult ShowDialog()
        {
            if(YesToAllClicked)
                return DialogResult.Yes;

            if(NoToAllClicked)
                return DialogResult.No;

            return base.ShowDialog();
        }

        public DialogResult ShowDialog(string message, string caption)
        {
            label1.Text = message;
            this.Text = caption;

            return ShowDialog();
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void btnYesToAll_Click(object sender, EventArgs e)
        {
            YesToAllClicked = true;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void btnNoToAll_Click(object sender, EventArgs e)
        {
            NoToAllClicked = true;
            this.DialogResult = DialogResult.No;
            this.Close();
        }
    } 
}
