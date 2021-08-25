// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Autocomplete;
using ScintillaNET;

namespace Rdmp.UI.AutoComplete
{
    public class AutoCompleteProviderWin : AutoCompleteProvider
    {
        public AutoCompleteProviderWin()
        {
        }
        public AutoCompleteProviderWin(IQuerySyntaxHelper helper) : base(helper)
        {
        }

        public void RegisterForEvents(Scintilla queryEditor)
        {
            queryEditor.CharAdded += scintilla_CharAdded;
            queryEditor.AutoCIgnoreCase = true;
        }


        private void scintilla_CharAdded(object sender, CharAddedEventArgs e)
        {
            var scintilla = (Scintilla)sender;

            // Find the word start
            var currentPos = scintilla.CurrentPosition;
            var wordStartPos = scintilla.WordStartPosition(currentPos, false);

            var list = Items.SelectMany(GetBits).OrderBy(a => a).Where(s=>!string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            // Display the autocompletion list
            var lenEntered = currentPos - wordStartPos;
            if (lenEntered > 0)
            {
                    scintilla.AutoCShow(lenEntered, string.Join(' ', list));                    
            }
        }
    }
}
