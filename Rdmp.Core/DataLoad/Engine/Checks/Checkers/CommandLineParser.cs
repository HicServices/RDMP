// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;

namespace Rdmp.Core.DataLoad.Engine.Checks.Checkers;

internal class CommandLineParser
{
    private char[] cmd; // source buffer
    private StringBuilder buf; // output buffer
    private int i; // current position within the source buffer

    public CommandLineParser()
    {
        cmd = null;
        buf = null;
        i = -1;
    }

    public IEnumerable<string> Parse(string commandLine)
    {
        cmd = commandLine.ToCharArray();
        buf = new StringBuilder();
        i = 0;

        while (i < cmd.Length)
        {
            var ch = cmd[i];

            if (char.IsWhiteSpace(ch)) throw new InvalidOperationException();

            switch (ch)
            {
                case '\\':
                    ParseEscapeSequence();
                    break;
                case '"':
                    ParseQuotedWord();
                    break;
                default:
                    ParseBareWord();
                    break;
            }

            if (i >= cmd.Length || char.IsWhiteSpace(cmd[i]))
            {
                var arg = buf.ToString();

                yield return arg;

                buf.Length = 0;
                ConsumeWhitespace();
            }
        }
    }

    /// <summary>
    /// Parse a quoted word
    /// </summary>
    private void ParseQuotedWord()
    {
        // scan over the lead-in quotation mark w/o adding it to the buffer
        ++i;

        // scan the contents of the quoted word into the buffer
        while (i < cmd.Length && cmd[i] != '"')
        {
            var ch = cmd[i];
            if (ch == '\\')
            {
                ParseEscapeSequence();
            }
            else
            {
                buf.Append(ch);
                ++i;
            }
        }

        // scan over the lead-out quotation mark w/o adding it to the buffer
        if (i < cmd.Length) ++i;
    }

    /// <summary>
    /// Parse a bareword
    /// </summary>
    private void ParseBareWord()
    {
        while (i < cmd.Length)
        {
            var ch = cmd[i];
            if (char.IsWhiteSpace(ch)) break; // whitespace terminates a bareword
            if (ch == '"') break; // lead-in quote starts a quoted word
            if (ch == '\\') break; // escape sequence terminates the bareword

            buf.Append(ch); // otherwise, keep reading this word

            ++i;
        }
    }

    /// <summary>
    /// Parse an escape sequence of one or more backslashes followed an an optional trailing quotation mark
    /// </summary>
    private void ParseEscapeSequence()
    {
        //---------------------------------------------------------------------------------------------------------
        // The rule is that:
        //
        // * An even number of backslashes followed by a quotation mark ('"') means that
        //   - the backslashes are escaped, so half that many get injected into the buffer, and
        //   - the quotation mark is a lead-in/lead-out quotation mark that marks the start of a quoted word
        //     which does not get added to the buffer.
        //
        // * An odd number of backslashes followed by a quotation mark ('"') means that
        //   - the backslashes are escaped, so half that many get injected into the buffer, and
        //   - the quotation mark is escaped. It's a literal quotation mark that also gets injected into the buffer
        //
        // * Any number of backslashes that aren't followed by a quotation mark ('"') have no special meaning:
        //   all of them get added to the buffer as-sis.
        //
        //---------------------------------------------------------------------------------------------------------

        //
        // scan in the backslashes
        //
        var p = i; // start of the escape sequence
        while (i < cmd.Length && cmd[i] == '\\')
        {
            buf.Append('\\');
            ++i;
        }

        //
        // if the backslash sequence is followed by a quotation mark, it's an escape sequence
        //
        if (i < cmd.Length && cmd[i] == '"')
        {
            var n = i - p; // find the number of backslashes seen
            var quotient = n >> 1; // n divide 2 ( 5 div 2 = 2 , 6 div 2 = 3 )
            var remainder = n & 1; // n modulo 2 ( 5 mod 2 = 1 , 6 mod 2 = 0 )

            buf.Length -= quotient + remainder; // remove the unwanted backslashes

            if (remainder != 0)
            {
                // the trailing quotation mark is an escaped, literal quotation mark
                // add it to the buffer and increment the pointer
                buf.Append('"');
                ++i;
            }
        }

        return;
    }

    /// <summary>
    /// Consume inter-argument whitespace
    /// </summary>
    private void ConsumeWhitespace()
    {
        while (i < cmd.Length && char.IsWhiteSpace(cmd[i])) ++i;
    }
}