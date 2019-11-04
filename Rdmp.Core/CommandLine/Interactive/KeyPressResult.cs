// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

namespace Rdmp.Core.CommandLine.Interactive
{
    /// <summary>
    /// Documents the state of a key pressed in the console
    /// </summary>
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
