using System;

namespace LoadModules.Generic.DataFlowOperations.Aliases.Exceptions
{
    /// <summary>
    /// Exception thrown by AliasHandler when it is unable to reach the Alias fact table (See AliasHandler class and AliasHandler.docx).
    /// </summary>
    public class AliasTableFetchException : Exception
    {
        public AliasTableFetchException(string msg) : base(msg)
        {
            
        }
    }
}
