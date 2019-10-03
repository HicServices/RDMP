
using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    public class MoveCursorToBeginAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            console.CursorPosition = 0;
        }
    }
}
