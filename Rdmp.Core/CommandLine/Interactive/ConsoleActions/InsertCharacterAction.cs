
using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    public class InsertCharacterAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (consoleKeyInfo.KeyChar == 0 || console.CurrentLine.Length >= byte.MaxValue - 1)
                return;
            console.CurrentLine = console.CurrentLine.Insert(console.CursorPosition, consoleKeyInfo.KeyChar.ToString());
            console.CursorPosition++;
        }
    }
}
