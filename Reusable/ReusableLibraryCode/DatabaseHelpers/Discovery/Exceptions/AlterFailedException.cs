using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Exceptions
{
    public class AlterFailedException : Exception
    {
        public AlterFailedException(string message, Exception inner)
            : base(message, inner)
        {
            
        }
    }
}