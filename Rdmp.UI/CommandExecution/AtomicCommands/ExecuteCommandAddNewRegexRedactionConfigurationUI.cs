using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddNewRegexRedactionConfigurationUI : ExecuteCommandCreateRegexRedactionConfiguration
    {
        private readonly IActivateItems _activator;

        public ExecuteCommandAddNewRegexRedactionConfigurationUI(IActivateItems activator) : base(activator)
        {
            _activator = activator;
        }

        //public override void Execute()
        //{
        //    //var ui = new 
        //}
    }

}
