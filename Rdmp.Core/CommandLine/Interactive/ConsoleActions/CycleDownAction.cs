// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    class CycleDownAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (!console.PreviousLineBuffer.CycleDown())
                return;
            console.CurrentLine = console.PreviousLineBuffer.LineAtIndex;
            console.CursorPosition = console.CurrentLine.Length;
        }
    }
}
