// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Menus;
using Rdmp.UI.Menus.MenuItems;
using Rdmp.UI.SimpleDialogs;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands.UIFactory
{
    public class AtomicCommandUIFactory
    {
        private readonly IActivateItems _activator;
        private readonly IIconProvider _iconProvider;

        public AtomicCommandUIFactory(IActivateItems activator)
        {
            _activator = activator;
            _iconProvider = activator.CoreIconProvider;
        }

        public ToolStripMenuItem CreateMenuItem(IAtomicCommand command)
        {
            return new AtomicCommandMenuItem(command, _activator){Tag = command };
        }

        public AtomicCommandLinkLabel CreateLinkLabel(IAtomicCommand command)
        {
            return new AtomicCommandLinkLabel(_iconProvider,command){Tag = command };
        }

        public ToolStripItem CreateToolStripItem(IAtomicCommand command)
        {
            return new AtomicCommandToolStripItem(command, _activator);
        }

        public Button CreateButton(IAtomicCommand cmd)
        {
            var tt = new ToolTip();

            Button b = new Button
            {
                Width = 26,
                Height = 26,
                Image = cmd.GetImage(_activator.CoreIconProvider),
                Tag = cmd
            };

            b.Click += (s, e) =>
            {
                try
                {
                    cmd.Execute();
                }
                catch (Exception ex)
                {
                    ExceptionViewer.Show("Command Failed", ex);
                }
            };

            tt.SetToolTip(b, cmd.GetCommandHelp());

            return b;
        }
    }

}
