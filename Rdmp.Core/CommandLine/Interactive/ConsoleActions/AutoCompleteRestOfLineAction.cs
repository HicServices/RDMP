// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

#pragma warning disable 1591

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    /// <summary>
    /// Replaces the current console text with 
    /// </summary>
    class AutoCompleteRestOfLineAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (!console.PreviousLineBuffer.HasLines)
                return;
            var previous = console.PreviousLineBuffer.LineAtIndex;
            if (previous.Length <= console.CursorPosition)
                return;
            var partToUse = previous.Remove(0, console.CursorPosition);
            var newLine = console.CurrentLine.Remove(console.CursorPosition,
                Math.Min(console.CurrentLine.Length - console.CursorPosition, partToUse.Length));
            console.CurrentLine = newLine.Insert(console.CursorPosition, partToUse);
            console.CursorPosition = console.CursorPosition + partToUse.Length;
        }
    }
}
