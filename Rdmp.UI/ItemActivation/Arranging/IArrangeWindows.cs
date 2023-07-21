// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.UI.ItemActivation.Arranging;

/// <summary>
///     Facilitates opening/closing lots of windows at once to achieve a specific goal (e.g. running a data load).
///     Basically sets up the tabs for a user friendly
///     consistent experience for the called user task.
/// </summary>
public interface IArrangeWindows
{
    //basic case where you only want to Emphasise and Activate it (after closing all other windows)
    void SetupEditAnything(object sender, IMapsDirectlyToDatabaseTable o);
    void Setup(WindowLayout target);
}