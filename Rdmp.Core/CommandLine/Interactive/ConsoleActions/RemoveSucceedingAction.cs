
using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    public class RemoveSucceedingAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (console.CursorPosition < console.CurrentLine.Length)
                console.CurrentLine = console.CurrentLine.Remove(console.CursorPosition);
        }
    }
}
