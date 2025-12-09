using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.Datasets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    class ExecuteCommandAddNewDatasetUI : ExecuteCommandCreateDataset
    {
        private readonly IActivateItems _activator;
        private readonly Type _providerType;

        public ExecuteCommandAddNewDatasetUI(IActivateItems activator, Type providerType) : base(
           activator, "New Dataset")
        {
            _activator = activator;
            _providerType = providerType; ;
        }

        public override void Execute()
        {
            var ui = new ImportExistingDatasetUI(_activator, _providerType);
            ui.ShowDialog();
        }
    }
}
