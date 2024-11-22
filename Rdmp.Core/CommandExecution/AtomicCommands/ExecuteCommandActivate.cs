// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Command for double clicking objects.
/// </summary>
public class ExecuteCommandActivate : BasicCommandExecution
{
    private readonly object _o;

    /// <summary>
    /// Set to true to also emphasise the object being activated
    /// </summary>
    public bool AlsoShow { get; set; }

    public ExecuteCommandActivate(IBasicActivateItems activator, object o) : base(activator)
    {
        _o = o;

        //if we have a masquerader and we cannot activate the masquerader, maybe we can activate what it is masquerading as?
        if (_o is IMasqueradeAs masquerader && !BasicActivator.CanActivate(masquerader))
            _o = masquerader.MasqueradingAs();

        if (!BasicActivator.CanActivate(_o))
            SetImpossible(GlobalStrings.ObjectCannotBeActivated);

        Weight = -99.99999f;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        _o == null ? null : iconProvider.GetImage(_o, OverlayKind.Edit);

    public override string GetCommandName() => OverrideCommandName ?? GlobalStrings.Activate;

    public override void Execute()
    {
        base.Execute();

        BasicActivator.Activate(_o);

        if (_o is DatabaseEntity d && AlsoShow)
            Emphasise(d);
    }
}