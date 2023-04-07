// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using ScintillaNET;

namespace Rdmp.UI.ScintillaHelper;

/// <summary>
/// Helper for highlighting specific lines of a <see cref="Scintilla"/> text editor.
/// </summary>
public class ScintillaLineHighlightingHelper
{
    public void HighlightLine(Scintilla editor, int i, Color color)
    {
        Marker marker = editor.Markers[0];
        marker.Symbol = MarkerSymbol.Background;
        marker.SetBackColor(color);
        editor.Lines[i].MarkerAdd(0);
    }

    public void ClearAll(Scintilla editor)
    {
        editor.MarkerDeleteAll(-1);
    }
}