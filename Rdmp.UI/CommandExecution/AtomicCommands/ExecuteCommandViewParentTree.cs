﻿// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Providers;
using Rdmp.UI.SimpleDialogs;
using System.Linq;

namespace Rdmp.UI.CommandExecution.AtomicCommands;
public class ExecuteCommandViewParentTree : BasicCommandExecution, IAtomicCommand
{

    private readonly IBasicActivateItems _activator;
    private readonly object _databaseObject;
    private DescendancyList _tree;

    public ExecuteCommandViewParentTree(IBasicActivateItems activator, object databaseObject) : base(activator)
    {
        _activator = activator;
        _databaseObject = databaseObject;
    }

    private void BuildTree()
    {
        _tree = _activator.CoreChildProvider.GetDescendancyListIfAnyFor(_databaseObject);
    }

    public override void Execute()
    {
        base.Execute();
        BuildTree();
        if (_activator.IsInteractive)
        {
            var tree = _tree.GetUsefulParents().ToList();
            tree.Add(_databaseObject);
            var dialog = new ViewParentTreeDialog(_activator, tree);
            dialog.ShowDialog();
        }
    }
}
