using System;
using ReusableLibraryCode.CommandExecution;

namespace ReusableLibraryCode.Exceptions
{
    /// <summary>
    /// Thrown when the API tries to Execute a command marked IsImpossible
    /// </summary>
    public class ImpossibleException : Exception
    {
        public ICommandExecution Command { get; private set; }
        public string ReasonCommandImpossible { get; private set; }

        public ImpossibleException(ICommandExecution command, string reasonCommandImpossible)
            : base(string.Format("Command is marked as IsImpossible and should not be Executed.  Reason is '{0}'", reasonCommandImpossible))
        {
            Command = command;
            ReasonCommandImpossible = reasonCommandImpossible;
        }
    }
}