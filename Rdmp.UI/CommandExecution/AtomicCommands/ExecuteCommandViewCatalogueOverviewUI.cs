using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Overview;
using Rdmp.UI.ExtractionUIs;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Overview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandViewCatalogueOverviewUI : BasicUICommandExecution
    {
        //private OverviewModel _overview;
        private Catalogue _catalogue;

        public ExecuteCommandViewCatalogueOverviewUI(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
            //_overview = new OverviewModel(activator,catalogue);
            _catalogue = catalogue;

        }

        public override void Execute()
        {
            Activator.Activate<ViewCatalogueOverviewUI, Catalogue>(_catalogue);
        }
    }
}

