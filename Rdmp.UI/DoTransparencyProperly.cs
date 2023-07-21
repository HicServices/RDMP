// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Windows.Forms;

namespace Rdmp.UI;

/// <summary>
///     Helper for positioning controls on top of one another (with transparent backgrounds)
/// </summary>
public class DoTransparencyProperly
{
    /// <summary>
    ///     Positions <paramref name="controlThatHovers" /> on top of <paramref name="whatItHoversOver" /> and
    ///     sets the background colour of the controlThatHovers to Transparent.
    /// </summary>
    /// <param name="controlThatHovers"></param>
    /// <param name="whatItHoversOver"></param>
    public static void ThisHoversOver(Control controlThatHovers, Control whatItHoversOver)
    {
        controlThatHovers.BackColor = Color.Transparent;

        var pos = controlThatHovers.Parent.PointToScreen(controlThatHovers.Location);
        controlThatHovers.Parent = whatItHoversOver;
        controlThatHovers.Location = whatItHoversOver.PointToClient(pos);
    }
}