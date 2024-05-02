// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     <see cref="ICommandExecution" /> with an Image designed for use with MenuItems and HomeUI.  Represents a single
///     atomic
///     action which the user can undertake.  The command may be <see cref="ICommandExecution.IsImpossible" />.
/// </summary>
public interface IAtomicCommand : ICommandExecution
{
    Image<Rgba32> GetImage(IIconProvider iconProvider);


    /// <summary>
    ///     When presenting the command in a hierarchical presentation should it be under a subheading
    ///     (e.g. in a context menu).  Null if not
    /// </summary>
    string SuggestedCategory { get; set; }

    /// <summary>
    ///     Key which should result in this command being fired e.g. "F2"
    /// </summary>
    string SuggestedShortcut { get; set; }

    /// <summary>
    ///     True to require Ctrl key to be pressed when <see cref="SuggestedShortcut" /> is entered
    /// </summary>
    bool Ctrl { get; set; }

    /// <summary>
    ///     The relative heaviness of the control in menus.  Higher means that it sinks down the menu
    ///     Lower floats to the top.  Also determines where Separators appear (if supported by the UI
    ///     framework).
    /// </summary>
    float Weight { get; set; }
}