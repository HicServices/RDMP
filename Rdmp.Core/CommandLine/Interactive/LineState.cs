// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

namespace Rdmp.Core.CommandLine.Interactive
{
    /// <summary>
    /// Records the state of the currently typed line in the console
    /// </summary>
    public class LineState
    {
        /// <summary>
        /// Records the state of the Console either before or after a keypress
        /// </summary>
        /// <param name="line"></param>
        /// <param name="cursorPosition"></param>
        public LineState(string line, int cursorPosition)
        {
            Line = line;
            if (line == null)
                return;
            CursorPosition = Math.Min(line.Length, Math.Max(0, cursorPosition));
            LineBeforeCursor = line.Substring(0, CursorPosition);
            LineAfterCursor = line.Substring(CursorPosition);
        }

        /// <summary>
        /// The text that has been typed so far
        /// </summary>
        public string Line { get; private set; }

        /// <summary>
        /// The location of the cursor on the console.  If no text has been entered yet on the line then the <see cref="CursorPosition"/>
        /// is usually 0.  Once text has been entered the cursor is usually 1 index ahead of the characters typed (i.e. <see cref="Line"/>="a"
        /// <see cref="CursorPosition"/>=1.
        /// </summary>
        public int CursorPosition { get; private set; }

        /// <summary>
        /// The text of the line before the <see cref="CursorPosition"/> (exclusive of the cursor location)
        /// </summary>
        public string LineBeforeCursor { get; private set; }
        
        /// <summary>
        /// The text of the line after the <see cref="CursorPosition"/> (inclusive of the cursor location)
        /// </summary>
        public string LineAfterCursor { get; private set; }

    }
}
