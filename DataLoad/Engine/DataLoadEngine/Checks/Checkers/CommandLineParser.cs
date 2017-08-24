using System;
using System.Collections.Generic;
using System.Text;

namespace DataLoadEngine.Checks.Checkers
{
    class CommandLineParser
    {
        char[] cmd; // source buffer
        StringBuilder buf; // output buffer
        int i; // current position within the source buffer

        public CommandLineParser()
        {
            cmd = null;
            buf = null;
            i = -1;
            return;
        }

        public IEnumerable<string> Parse(string commandLine)
        {
            cmd = commandLine.ToCharArray();
            buf = new StringBuilder();
            i = 0;

            while (i < cmd.Length)
            {
                char ch = cmd[i];

                if (char.IsWhiteSpace(ch)) { throw new InvalidOperationException(); }
                else if (ch == '\\') { ParseEscapeSequence(); }
                else if (ch == '"') { ParseQuotedWord(); }
                else { ParseBareWord(); }

                if (i >= cmd.Length || char.IsWhiteSpace(cmd[i]))
                {
                    string arg = buf.ToString();

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
                char ch = cmd[i];
                if (ch == '\\') { ParseEscapeSequence(); }
                else { buf.Append(ch); ++i; }
            }

            // scan over the lead-out quotation mark w/o adding it to the buffer
            if (i < cmd.Length)
            {
                ++i;
            }
            return;
        }

        /// <summary>
        /// Parse a bareword
        /// </summary>
        private void ParseBareWord()
        {
            while (i < cmd.Length)
            {
                char ch = cmd[i];
                if (char.IsWhiteSpace(ch)) break; // whitespace terminates a bareword
                else if (ch == '"') break; // lead-in quote starts a quoted word
                else if (ch == '\\') break; // escape sequence terminates the bareword

                buf.Append(ch); // otherwise, keep reading this word                

                ++i;
            }
            return;
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
            int p = i; // start of the escape sequence
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
                int n = (i - p); // find the number of backslashes seen
                int quotient = n >> 1; // n divide 2 ( 5 div 2 = 2 , 6 div 2 = 3 )
                int remainder = n & 1; // n modulo 2 ( 5 mod 2 = 1 , 6 mod 2 = 0 )

                buf.Length -= (quotient + remainder); // remove the unwanted backslashes

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
            while (i < cmd.Length && char.IsWhiteSpace(cmd[i]))
            {
                ++i;
            }
            return;
        }
    }
}