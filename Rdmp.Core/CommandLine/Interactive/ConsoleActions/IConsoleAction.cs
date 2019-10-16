// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    interface IConsoleAction
    {
        void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo);
    }
}