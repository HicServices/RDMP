// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    class IgnoreAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            // Do nothing. "Ignore" the command
        }
    }
}
