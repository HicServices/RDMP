using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadModules.Generic.DataFlowOperations.Aliases
{
    public enum AliasResolutionStrategy
    {
        CrashIfAliasesFound,
        MultiplyInputDataRowsByAliases
    }
}
