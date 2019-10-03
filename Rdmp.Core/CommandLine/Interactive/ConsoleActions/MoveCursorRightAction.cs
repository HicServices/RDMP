
using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    public class MoveCursorRightAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            console.CursorPosition = Math.Min(console.CurrentLine.Length, console.CursorPosition + 1);
        }
    }
}
