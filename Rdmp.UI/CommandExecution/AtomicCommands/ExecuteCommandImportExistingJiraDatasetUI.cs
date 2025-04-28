using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.Datasets.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandImportExistingJiraDatasetUI : ExecuteCommandCreateDataset
    {
        private readonly IActivateItems _activator;

        public ExecuteCommandImportExistingJiraDatasetUI(IActivateItems activator) : base(
           activator, "New Dataset")
        {
            _activator = activator;
        }

        public override void Execute()
        {
            var ui = new ImportExistingJiraDatasetUI(_activator, this);
            ui.ShowDialog();
        }
    }
}
