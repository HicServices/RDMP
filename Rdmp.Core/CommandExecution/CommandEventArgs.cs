using System;
using Rdmp.Core.CommandExecution.AtomicCommands;

namespace Rdmp.Core.CommandExecution
{
    public class CommandEventArgs:EventArgs
    {
        public IAtomicCommand Command { get; }

        public CommandEventArgs(IAtomicCommand command)
        {
            Command = command;
        }
    }
}