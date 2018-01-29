using System;

namespace LoadModules.Generic.DataFlowOperations.Aliases.Exceptions
{
    /// <summary>
    /// Exception thrown by pipeline component AliasHandler when the AliasResolutionStrategy is CrashIfAliasesFound and Aliases exist for one or more records
    /// </summary>
    public class AliasException:Exception
    {
        public AliasException(string msg) : base(msg)
        {
            
        }
    }
}
