// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

namespace Rdmp.Core.CommandLine.Interactive
{
    /// <summary>
    /// Records the state of the currently typed line in the console
    /// </summary>
    public class LineState
    {
        public LineState(string line, int cursorPosition)
        {
            Line = line;
            if (line == null)
                return;
            CursorPosition = Math.Min(line.Length, Math.Max(0, cursorPosition));
            LineBeforeCursor = line.Substring(0, CursorPosition);
            LineAfterCursor = line.Substring(CursorPosition);
        }

        public string Line { get; private set; }
        public int CursorPosition { get; private set; }
        public string LineBeforeCursor { get; private set; }
        public string LineAfterCursor { get; private set; }
    }
}
