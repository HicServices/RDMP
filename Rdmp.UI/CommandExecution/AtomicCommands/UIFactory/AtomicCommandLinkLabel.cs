// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;


namespace Rdmp.UI.CommandExecution.AtomicCommands.UIFactory
{
    /// <summary>
    /// Provides access to an IAtomicCommand including showing the command name, help text etc.  When the link label is clicked the command .Execute is run.
    /// </summary>
    [TechnicalUI]
    public partial class AtomicCommandLinkLabel : UserControl
    {
        private readonly IAtomicCommand _command;

        public AtomicCommandLinkLabel(IIconProvider iconProvider, IAtomicCommand command)
        {
            _command = command;
            InitializeComponent();

            pbCommandIcon.Image = command.GetImage(iconProvider).ImageToBitmap();
            var name = command.GetCommandName();
            lblName.Text = name;

            helpIcon1.SetHelpText(_command.GetCommandName(),command.GetCommandHelp());
        }
        
        private void label1_Click(object sender, EventArgs e)
        {
            _command.Execute();
        }

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            Cursor = DefaultCursor;
        }
    }
}
