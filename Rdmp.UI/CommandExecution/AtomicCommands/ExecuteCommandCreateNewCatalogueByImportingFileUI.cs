using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.UI.ItemActivation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdmp.UI.SimpleDialogs.SimpleFileImporting;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewCatalogueByImportingFileUI : ExecuteCommandCreateNewCatalogueByImportingFile
    {
        private readonly IActivateItems _activator;

        public ExecuteCommandCreateNewCatalogueByImportingFileUI(IActivateItems activator,FileInfo file = null) :base(activator,file)
        {
            this._activator = activator;
        }
        public override void Execute()
        {
            if(IsImpossible)
                throw new ImpossibleCommandException(this, ReasonCommandImpossible);

            var ui = new CreateNewCatalogueByImportingFileUI(_activator,this);
            ui.ShowDialog();
        }
    }
}
