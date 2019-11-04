// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    class InsertCharacterAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (consoleKeyInfo.KeyChar == 0 || console.CurrentLine.Length >= byte.MaxValue - 1)
                return;
            console.CurrentLine = console.CurrentLine.Insert(console.CursorPosition, consoleKeyInfo.KeyChar.ToString());
            console.CursorPosition++;
        }
    }
}
