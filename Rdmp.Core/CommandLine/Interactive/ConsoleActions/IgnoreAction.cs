
using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    public class IgnoreAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            // Do nothing. "Ignore" the command
        }
    }
}
