// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

#pragma warning disable 1591

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    interface IConsole
    {
        PreviousLineBuffer PreviousLineBuffer { get; }
        string CurrentLine { get; set; }
        int CursorPosition { get; set; }
    }
}