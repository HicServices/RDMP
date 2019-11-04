// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    class DeleteAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (console.CursorPosition < console.CurrentLine.Length)
                console.CurrentLine = console.CurrentLine.Remove(console.CursorPosition, 1);
        }
    }
}
