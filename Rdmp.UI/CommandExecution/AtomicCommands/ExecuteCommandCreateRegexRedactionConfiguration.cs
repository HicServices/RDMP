using Rdmp.Core.CommandExecution;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateRegexRedactionConfiguration: BasicCommandExecution
    {
        public ExecuteCommandCreateRegexRedactionConfiguration(IActivateItems activator) : base(activator)
        {
        }
    }
}
