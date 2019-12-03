// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;
using Rdmp.Core.CommandLine.Interactive.ConsoleActions;

#pragma warning disable 1591

namespace Rdmp.Core.CommandLine.Interactive
{
    internal class ConsoleExtInstance : IConsole
    {
        public PreviousLineBuffer PreviousLineBuffer { get { return ConsoleExt.PreviousLineBuffer; } }
        public string CurrentLine { get { return ConsoleExt.CurrentLine; } set { ConsoleExt.CurrentLine = value; } }
        public int CursorPosition { get { return Console.CursorLeft; } set { Console.CursorLeft = value; } }
    }
}
