// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal sealed class ExecuteCommandCheckAsync : BasicUICommandExecution
{
    private readonly ICheckable _checkable;
    private readonly Action<ICheckable, CheckResult> _reportWorstTo;

    public ExecuteCommandCheckAsync(IActivateItems activator, DatabaseEntity checkable) : base(activator)
    {
        _checkable = checkable as ICheckable;

        if (_checkable == null)
            SetImpossible("Object is not checkable");

        Weight = 100.3f;
    }

    public ExecuteCommandCheckAsync(IActivateItems activator, DatabaseEntity checkable,
        Action<ICheckable, CheckResult> reportWorst) : this(activator, checkable)
    {
        _reportWorstTo = reportWorst;
    }

    public override string GetCommandName() => _checkable == null ? "Check" : $"Check '{_checkable}'";

    public override string GetCommandHelp() =>
        "Run validation checks for this item to ensure that easily checkable properties are valid";

    public override void Execute()
    {
        base.Execute();

        var popupChecksUI = new PopupChecksUI($"Checking {_checkable}", false);

        if (_reportWorstTo != null)
            popupChecksUI.AllChecksComplete += (s, a) => _reportWorstTo(_checkable, a.CheckResults.GetWorst());

        popupChecksUI.StartChecking(_checkable);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => Image.Load<Rgba32>(CatalogueIcons.TinyYellow);
}