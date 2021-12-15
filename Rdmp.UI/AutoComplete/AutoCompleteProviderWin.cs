// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Autocomplete;
using ScintillaNET;

namespace Rdmp.UI.AutoComplete
{
    /// <summary>
    /// Provides autocomplete handling and event hooking for <see cref="Scintilla"/> control
    /// </summary>
    public class AutoCompleteProviderWin : AutoCompleteProvider
    {
        public AutoCompleteProviderWin(IQuerySyntaxHelper helper) : base(helper)
        {
        }

        private char Separator = ';';

        public char[] ExtraWordChars = new[] { '.','[', ']','_' };

        public void RegisterForEvents(Scintilla queryEditor)
        {
            queryEditor.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Space && e.Control)
                {
                    e.SuppressKeyPress = true;
                    ShowAutocomplete(queryEditor, true);
                }
            };

            queryEditor.AutoCSeparator = Separator;
            queryEditor.CharAdded += scintilla_CharAdded;
            queryEditor.AutoCIgnoreCase = true;
            queryEditor.AutoCOrder = Order.Custom;
            queryEditor.AutoCAutoHide = false;
        }

        private void scintilla_CharAdded(object sender, CharAddedEventArgs e)
        {
            if (sender is not Scintilla scintilla)
                return;
            
            ShowAutocomplete(scintilla,false);
        }

        private void ShowAutocomplete(Scintilla scintilla,bool all)
        {
            // Find the word start
            string word = scintilla.GetWordFromPosition(scintilla.CurrentPosition)?.Trim();
            
            if (string.IsNullOrWhiteSpace(word) && !all)
                    return;

            var list = Items.Distinct()
                .Where(s => !string.IsNullOrWhiteSpace(s) && s.Contains(word,StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(a => a);

            // Display the autocompletion list
            scintilla.AutoCShow(word.Length, string.Join(Separator, list));
        }
    }
}
