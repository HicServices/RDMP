// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

#pragma warning disable 1591

namespace Rdmp.Core.CommandLine.Interactive
{
    /// <summary>
    /// Documents the state of a key pressed in the console
    /// </summary>
    public class KeyPressResult
    {
        /// <summary>
        /// A key pressed on the console with the cached state before and after keypress
        /// </summary>
        /// <param name="consoleKeyInfo"></param>
        /// <param name="lineBeforeKeyPress"></param>
        /// <param name="lineAfterKeyPress"></param>
        public KeyPressResult(ConsoleKeyInfo consoleKeyInfo, LineState lineBeforeKeyPress, LineState lineAfterKeyPress)
        {
            ConsoleKeyInfo = consoleKeyInfo;
            LineBeforeKeyPress = lineBeforeKeyPress;
            LineAfterKeyPress = lineAfterKeyPress;
        }

        /// <summary>
        /// The key that was pressed
        /// </summary>
        public ConsoleKeyInfo ConsoleKeyInfo { get; private set; }

        /// <summary>
        /// Returns <see cref="System.ConsoleKeyInfo.Key"/>
        /// </summary>
        public ConsoleKey Key { get { return ConsoleKeyInfo.Key; } }

        /// <summary>
        /// Returns <see cref="System.ConsoleKeyInfo.KeyChar"/>
        /// </summary>
        public char KeyChar { get { return ConsoleKeyInfo.KeyChar; } }
        
        /// <summary>
        /// Returns <see cref="System.ConsoleKeyInfo.Modifiers"/>
        /// </summary>
        public ConsoleModifiers Modifiers { get { return ConsoleKeyInfo.Modifiers; } }

        /// <summary>
        /// The state of the console current line before the key was pressed
        /// </summary>
        public LineState LineBeforeKeyPress { get; private set; }

        /// <summary>
        /// The state of the console current line after the key was pressed
        /// </summary>
        public LineState LineAfterKeyPress { get; private set; }
    }
}
