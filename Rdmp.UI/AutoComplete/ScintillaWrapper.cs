// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using AutocompleteMenuNS;
using ScintillaNET;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Rdmp.UI.AutoComplete
{
    internal class ScintillaWrapper : ITextBoxWrapper
    {
        private Scintilla queryEditor;
        
        public event EventHandler LostFocus;
        
#pragma warning disable 0067
        public event ScrollEventHandler Scroll;
#pragma warning restore 0067
        public event KeyEventHandler KeyDown;
        public event MouseEventHandler MouseDown;

        public ScintillaWrapper(Scintilla queryEditor)
        {
            this.queryEditor = queryEditor;

            queryEditor.LostFocus += LostFocus;
            queryEditor.KeyDown += KeyDown;
            queryEditor.MouseDown += MouseDown;
            
        }

        public Control TargetControl => queryEditor;

        public string Text => queryEditor.Text;

        public string SelectedText { get => queryEditor.SelectedText; set => throw new NotImplementedException(); }
        public int SelectionLength { get => queryEditor.Selections.FirstOrDefault().Start; set => throw new NotImplementedException(); }
        public int SelectionStart { get => queryEditor.Selections.FirstOrDefault()?.Start ?? 0; set => throw new NotImplementedException(); }

        public bool Readonly => queryEditor.ReadOnly;


        public Point GetPositionFromCharIndex(int pos)
        {
            throw new NotImplementedException();
        }
    }
}