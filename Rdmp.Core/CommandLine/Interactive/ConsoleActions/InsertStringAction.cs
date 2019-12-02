// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

#pragma warning disable 1591

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    class InsertStringAction : IConsoleAction
    {
        private readonly string _string;

        public InsertStringAction(string str)
        {
            _string = str;
        }

        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (string.IsNullOrEmpty(_string))
                return;
            if (console.CurrentLine.Length >= byte.MaxValue - _string.Length)
                return;
            console.CurrentLine = console.CurrentLine.Insert(console.CursorPosition, _string);
            console.CursorPosition += _string.Length;
        }
    }
}
