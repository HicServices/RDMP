// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandShowTooltip : BasicUICommandExecution
{
    private readonly string _title;
    private readonly string _body;
    private readonly bool _isBad;

    public ExecuteCommandShowTooltip(IActivateItems activator, object o) : base(activator)
    {
        Weight = 100.5f;

        var hasOne = RDMPCollectionCommonFunctionality.GetToolTip(activator, o, out _title, out _body, out _isBad);

        if (!hasOne)
            SetImpossible($"{o} does not have a tooltip/problem");
    }

    public override string GetCommandName() => _isBad ? "Show Problem" : "Show Tooltip";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        (Image<Rgba32>)(_isBad ? Image.Load(FamFamFamIcons.flag_red) : iconProvider.GetImage(RDMPConcept.Help));


    public override void Execute()
    {
        base.Execute();

        if (_isBad)
            BasicActivator.ShowException(_title, new Exception(_body));
        else
            BasicActivator.Show(_title, _body);
    }
}