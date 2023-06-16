// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ScintillaNET;

namespace Rdmp.UI.ScintillaHelper;

/// <summary>
/// Syntax highlighter for <see cref="Scintilla"/>
/// </summary>
public class CSharpLexer
{
    public const int StyleDefault = 0;
    public const int StyleKeyword = 1;
    public const int StyleIdentifier = 2;
    public const int StyleNumber = 3;
    public const int StyleString = 4;

    private const int STATE_UNKNOWN = 0;
    private const int STATE_IDENTIFIER = 1;
    private const int STATE_NUMBER = 2;
    private const int STATE_STRING = 3;

    private HashSet<string> keywords;

    public void Style(Scintilla scintilla, int startPos, int endPos)
    {
        // Back up to the line start
        var line = scintilla.LineFromPosition(startPos);
        startPos = scintilla.Lines[line].Position;

        var length = 0;
        var state = STATE_UNKNOWN;

        // Start styling
        scintilla.StartStyling(startPos);
        while (startPos < endPos)
        {
            var c = (char)scintilla.GetCharAt(startPos);

            REPROCESS:
            switch (state)
            {
                case STATE_UNKNOWN:
                    if (c == '"')
                    {
                        // Start of "string"
                        scintilla.SetStyling(1, StyleString);
                        state = STATE_STRING;
                    }
                    else if (char.IsDigit(c))
                    {
                        state = STATE_NUMBER;
                        goto REPROCESS;
                    }
                    else if (char.IsLetter(c))
                    {
                        state = STATE_IDENTIFIER;
                        goto REPROCESS;
                    }
                    else
                    {
                        // Everything else
                        scintilla.SetStyling(1, StyleDefault);
                    }
                    break;

                case STATE_STRING:
                    if (c == '"')
                    {
                        length++;
                        scintilla.SetStyling(length, StyleString);
                        length = 0;
                        state = STATE_UNKNOWN;
                    }
                    else
                    {
                        length++;
                    }
                    break;

                case STATE_NUMBER:
                    if (char.IsDigit(c) || c is >= 'a' and <= 'f' || c is >= 'A' and <= 'F' || c == 'x')
                    {
                        length++;
                    }
                    else
                    {
                        scintilla.SetStyling(length, StyleNumber);
                        length = 0;
                        state = STATE_UNKNOWN;
                        goto REPROCESS;
                    }
                    break;

                case STATE_IDENTIFIER:
                    if (char.IsLetterOrDigit(c))
                    {
                        length++;
                    }
                    else
                    {
                        var style = StyleIdentifier;
                        var identifier = scintilla.GetTextRange(startPos - length, length);
                        if (keywords.Contains(identifier))
                            style = StyleKeyword;

                        scintilla.SetStyling(length, style);
                        length = 0;
                        state = STATE_UNKNOWN;
                        goto REPROCESS;
                    }
                    break;
            }

            startPos++;
        }
    }

    public CSharpLexer(string keywords)
    {
        // Put keywords in a HashSet
        var list = Regex.Split(keywords ?? string.Empty, @"\s+").Where(l => !string.IsNullOrEmpty(l));
        this.keywords = new HashSet<string>(list);
    }
}