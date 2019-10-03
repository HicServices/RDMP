
using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    public class MoveCursorToEndAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            console.CursorPosition = console.CurrentLine.Length;
        }
    }
}
