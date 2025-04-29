using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.Datasets.HDR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandImportExistingHDRDatasetUI : ExecuteCommandCreateDataset
    {
        private readonly IActivateItems _activator;

        public ExecuteCommandImportExistingHDRDatasetUI(IActivateItems activator) : base(
           activator, "New Dataset")
        {
            _activator = activator;
        }

        public override void Execute()
        {
            var ui = new ImportExistingHDRDatasetUI(_activator, this);
            ui.ShowDialog();
        }
    }
}
