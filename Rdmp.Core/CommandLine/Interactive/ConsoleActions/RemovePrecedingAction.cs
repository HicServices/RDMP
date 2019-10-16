// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    class RemovePrecedingAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (console.CursorPosition > 0)
            {
                console.CurrentLine = console.CurrentLine.Remove(0, console.CursorPosition);
                console.CursorPosition = 0;
            }
        }
    }
}
