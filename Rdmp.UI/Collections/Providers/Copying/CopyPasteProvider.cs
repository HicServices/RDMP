// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.UI.Copying;

namespace Rdmp.UI.Collections.Providers.Copying;

/// <summary>
/// Enables Ctrl+C support in <see cref="TreeListView"/>
/// </summary>
public class CopyPasteProvider
{
    private TreeListView _tree;

    public void RegisterEvents(TreeListView tree)
    {
        _tree = tree;
        _tree.KeyUp += TreeOnKeyUp;
    }

    private void TreeOnKeyUp(object sender, KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.KeyCode == Keys.C && keyEventArgs.Control)
        {
            var commandFactory = new RDMPCombineableFactory();

            var command = commandFactory.Create(_tree.SelectedObject);

            if (command != null)
                Clipboard.SetDataObject(command);
        }
    }
}