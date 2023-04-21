// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.SimpleDialogs;
using Cursors = System.Windows.Forms.Cursors;

namespace Rdmp.UI.LinkLabels;

/// <summary>
/// Label showing a file system path which opens the containing directory in explorer when clicked.
/// </summary>
public class PathLinkLabel : Label
{
    protected override void OnMouseHover(EventArgs e)
    {
        base.OnMouseHover(e);
        this.Cursor = Cursors.Hand;
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);

        if(!string.IsNullOrWhiteSpace(Text))
            try
            {
                UsefulStuff.GetInstance().ShowPathInWindowsExplorer(new DirectoryInfo(Text));
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
    }
        
    protected override void OnPaint(PaintEventArgs e)
    {
        //paint background
        using (SolidBrush b = new SolidBrush(BackColor))
            e.Graphics.FillRectangle(b, Bounds);
            
        //paint text
        using(Font f = new Font(Font, FontStyle.Underline))
            TextRenderer.DrawText(e.Graphics, Text, f, ClientRectangle, Color.Blue, TextFormatFlags.PathEllipsis);
    }
}