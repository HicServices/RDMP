using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution;
using Rdmp.UI.ItemActivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdmp.UI.SimpleDialogs.Datasets.Jira;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewJiraConfigurationUI : BasicCommandExecution, IAtomicCommand
    {
        private readonly IActivateItems _activator;

        public ExecuteCommandCreateNewJiraConfigurationUI(IActivateItems activator) : base(activator)
        {
            _activator = activator;
        }

        public override void Execute()
        {
            base.Execute();
            var ui = new CreateNewJiraConfigurationUI(_activator);
            ui.ShowDialog();
        }
    }
}
