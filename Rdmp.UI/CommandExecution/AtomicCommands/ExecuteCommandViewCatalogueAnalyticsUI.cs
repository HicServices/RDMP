using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.Analytics;
using Rdmp.UI.ExtractionUIs;
using Rdmp.UI.ItemActivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    class ExecuteCommandViewCatalogueAnalyticsUI: BasicUICommandExecution, IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;

        [UseWithObjectConstructor]
        public ExecuteCommandViewCatalogueAnalyticsUI(IActivateItems activator, Catalogue catalogue) : this(activator)
        {
            _catalogue = catalogue;
        }

        public ExecuteCommandViewCatalogueAnalyticsUI(IActivateItems activator) : base(activator)
        {
        }
        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _catalogue = (Catalogue)target;
            return this;
        }

        public override void Execute()
        {
            Activator.Activate<ViewCatalogueAnalyticsUI, Catalogue>(_catalogue);
        }
    }
}
