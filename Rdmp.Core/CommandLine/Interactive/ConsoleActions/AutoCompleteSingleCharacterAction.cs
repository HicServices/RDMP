// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

#pragma warning disable 1591

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    class AutoCompleteSingleCharacterAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (!console.PreviousLineBuffer.HasLines ||
                        console.CurrentLine.Length >= console.PreviousLineBuffer.LastLine.Length)
                return;
            if (console.CursorPosition == console.CurrentLine.Length)
            {
                console.CurrentLine = console.CurrentLine + console.PreviousLineBuffer.LastLine[console.CurrentLine.Length];
            }
            console.CursorPosition++;
        }
    }
}
