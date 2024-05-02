// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core;

public abstract class PluginUserInterface : IPluginUserInterface
{
    protected readonly IBasicActivateItems BasicActivator;

    public Image<Rgba32> ImageUnknown => BasicActivator.CoreIconProvider.ImageUnknown;

    /// <summary>
    ///     Creates a new instance of your plugin UI.  See notes on <paramref name="itemActivator" />
    /// </summary>
    /// <param name="itemActivator">
    ///     The UI layer of the client.  May be a console UI activator or a winforms activator.  Use GetType to
    ///     determine if the currently running UI layer is one you support in your plugin.
    /// </param>
    protected PluginUserInterface(IBasicActivateItems itemActivator)
    {
        BasicActivator = itemActivator;
    }

    public virtual object[] GetChildren(object model)
    {
        return null;
    }

    /// <summary>
    ///     Override to return a custom set of commands for some objects
    /// </summary>
    /// <param name="o">
    ///     An object that was right clicked or a member of the enum <see cref="RDMPCollection" /> if a right
    ///     click occurs in whitespace
    /// </param>
    /// <returns></returns>
    public virtual IEnumerable<IAtomicCommand> GetAdditionalRightClickMenuItems(object o)
    {
        yield break;
    }

    public virtual Image<Rgba32> GetImage(object concept, OverlayKind kind = OverlayKind.None)
    {
        return null;
    }

    /// <inheritdoc />
    public virtual bool CustomActivate(IMapsDirectlyToDatabaseTable o)
    {
        return false;
    }
}