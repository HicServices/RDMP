using System;

namespace Rdmp.Core.CommandLine.Interactive
{
    public class KeyPressResult
    {
        public KeyPressResult(ConsoleKeyInfo consoleKeyInfo, LineState lineBeforeKeyPress, LineState lineAfterKeyPress)
        {
            ConsoleKeyInfo = consoleKeyInfo;
            LineBeforeKeyPress = lineBeforeKeyPress;
            LineAfterKeyPress = lineAfterKeyPress;
        }

        public ConsoleKeyInfo ConsoleKeyInfo { get; private set; }
        public ConsoleKey Key { get { return ConsoleKeyInfo.Key; } }
        public char KeyChar { get { return ConsoleKeyInfo.KeyChar; } }
        public ConsoleModifiers Modifiers { get { return ConsoleKeyInfo.Modifiers; } }
        public LineState LineBeforeKeyPress { get; private set; }
        public LineState LineAfterKeyPress { get; private set; }
    }
}
