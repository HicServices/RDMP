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
    class ExecuteCommandAddNewDatasetProviderUI: BasicUICommandExecution, IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly Type _provider;

        public ExecuteCommandAddNewDatasetProviderUI(IActivateItems activator, Type provider) : base(activator)
        {
            _activator = activator;
            _provider = provider;
        }

        public override void Execute()
        {
            base.Execute();
            var ui = new CreateNewDatasetProviderConfigurationUI(_activator, _provider);
            ui.ShowDialog();
        }
    }
}
