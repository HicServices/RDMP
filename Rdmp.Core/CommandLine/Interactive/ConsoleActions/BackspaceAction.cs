// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

#pragma warning disable 1591

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    class BackspaceAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (console.CursorPosition > 0)
            {
                console.CurrentLine = console.CurrentLine.Remove(console.CursorPosition - 1, 1);
                console.CursorPosition--;
            }
        }
    }
}
