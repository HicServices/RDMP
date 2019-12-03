// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

#pragma warning disable 1591

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    class RemoveSucceedingAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (console.CursorPosition < console.CurrentLine.Length)
                console.CurrentLine = console.CurrentLine.Remove(console.CursorPosition);
        }
    }
}
