// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Checks a given <see cref="ICheckable"/> object reporting integrity to an <see cref="ICheckNotifier"/>
/// </summary>
public sealed class ExecuteCommandCheck : BasicCommandExecution
{
    private readonly ICheckable _checkable;
    private readonly ICheckNotifier _notifier;

    public ExecuteCommandCheck(IBasicActivateItems activator, ICheckable checkable, ICheckNotifier notifier) :
        base(activator)
    {
        _checkable = checkable;
        _notifier = notifier;
    }

    public override void Execute()
    {
        base.Execute();
        _checkable.Check(_notifier);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => Image.Load<Rgba32>(CatalogueIcons.TinyYellow);
}