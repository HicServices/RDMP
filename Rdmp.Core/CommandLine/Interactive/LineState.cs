using System;

namespace Rdmp.Core.CommandLine.Interactive
{
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
