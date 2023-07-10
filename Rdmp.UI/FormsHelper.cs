// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Windows.Forms;

namespace Rdmp.UI;

/// <summary>
/// Helper Extension Methods for Control
/// </summary>
public static class FormsHelper
{
    /// <summary>
    /// Returns the visible portion of the control in client coordinates of c.  For example if you
    /// have a control in a scrollable container then this method will return the client rectangle
    /// that is visible with the current scroll viewport.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static Rectangle GetVisibleArea(this Control c)
    {
        var originalControl = c;
        var rect = c.RectangleToScreen(c.ClientRectangle);
        while (c != null)
        {
            rect = Rectangle.Intersect(rect, c.RectangleToScreen(c.ClientRectangle));
            c = c.Parent;
        }

        rect = originalControl.RectangleToClient(rect);
        return rect;
    }


}