// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandExpandAllNodes : BasicUICommandExecution
{
    private readonly RDMPCollectionCommonFunctionality _commonFunctionality;
    private readonly object _rootToExpandFrom;

    public ExecuteCommandExpandAllNodes(IActivateItems activator, RDMPCollectionCommonFunctionality commonFunctionality,
        object toExpand) : base(activator)
    {
        _commonFunctionality = commonFunctionality;
        _rootToExpandFrom = toExpand;

        // if we are expanding everything in the tree that is ok
        if (_rootToExpandFrom is RDMPCollection) return;

        if (!commonFunctionality.Tree.CanExpand(toExpand))
            SetImpossible("Node cannot be expanded");

        Weight = 100.4f;
    }

    public override string GetCommandName() =>
        _rootToExpandFrom is RDMPCollection && string.IsNullOrWhiteSpace(OverrideCommandName)
            ? "Expand All"
            : base.GetCommandName();

    public override void Execute()
    {
        base.Execute();

        _commonFunctionality.Tree.Visible = false;
        try
        {
            if (_rootToExpandFrom is RDMPCollection)
            {
                _commonFunctionality.Tree.ExpandAll();
                return;
            }

            _commonFunctionality.ExpandToDepth(int.MaxValue, _rootToExpandFrom);

            var index = _commonFunctionality.Tree.IndexOf(_rootToExpandFrom);
            if (index != -1)
                _commonFunctionality.Tree.EnsureVisible(index);
        }
        finally
        {
            _commonFunctionality.Tree.Visible = true;
        }
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        Image.Load<Rgba32>(CatalogueIcons.ExpandAllNodes);
}