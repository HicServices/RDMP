using System;

namespace Rdmp.Core.CommandLine.Interactive
{
    /// <summary>
    /// Exception for when user input is required during an operation but disallowed for some reason (e.g. command running from CLI
    /// in unattended mode).  
    /// </summary>
    public class InputDisallowedException : Exception
    {
        public InputDisallowedException(string message):base(message)
        {
            
        }
    }
}