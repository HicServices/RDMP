// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     Folder Node that can be added to TreeListViews.  You can only add one folder of each name because they inherit from
///     <see cref="SingletonNode" />.
/// </summary>
public class ArbitraryFolderNode : SingletonNode, IOrderable
{
    public int Order { get; set; }

    /// <summary>
    ///     Commands to be created when/if the node is right clicked.  Null if no commands are required
    /// </summary>
    public Func<IEnumerable<IAtomicCommand>> CommandGetter { get; set; }

    public ArbitraryFolderNode(string caption, int order) : base(caption)
    {
        Order = order;
    }
}