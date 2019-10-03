using System;

namespace Rdmp.Core.CommandLine.Interactive.ConsoleActions
{
    public interface IConsoleAction
    {
        void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo);
    }
}