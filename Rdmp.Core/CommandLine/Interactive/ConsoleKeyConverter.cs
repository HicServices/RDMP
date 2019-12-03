// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;

#pragma warning disable 1591

namespace Rdmp.Core.CommandLine.Interactive
{
    static class ConsoleKeyConverter
    {
        public static bool TryParseChar(char keyChar, out ConsoleKey consoleKey)
        {
            if (!Enum.TryParse(keyChar.ToString().ToUpper(), out consoleKey))
                return false;
            return true;
        }

        public static char ConvertConsoleKey(ConsoleKey consoleKey)
        {
            return (char) consoleKey;
        }
    }
}
