using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandReorderFilter: BasicUICommandExecution
    {
        public ExecuteCommandReorderFilter(IActivateItems activator, ConcreteFilter source, ConcreteFilter destination, InsertOption insertOption):base(activator)
        {

        }

        public override void Execute()
        {

        }
    }

}
