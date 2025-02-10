using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.DataQualityUIs;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdmp.UI.CatalogueAnalysisUIs;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommanndAnalyseCatalogue : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;

        [UseWithObjectConstructor]
        public ExecuteCommanndAnalyseCatalogue(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
            _catalogue = catalogue;
        }
        public ExecuteCommanndAnalyseCatalogue(IActivateItems activator) : base(activator)
        {
        }

        public override string GetCommandHelp() =>
            "...TODO";

        public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
            iconProvider.GetImage(RDMPConcept.DQE, OverlayKind.Execute);

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _catalogue = (Catalogue)target;
            return this;
        }

        public override void Execute()
        {
            base.Execute();

            var c = _catalogue ?? SelectOne<Catalogue>(BasicActivator.RepositoryLocator.CatalogueRepository);

            if (c == null)
                return;

            Activator.Activate<CatalogueAnalysisExecutionControlUI, Catalogue>(c);
        }

        public override string GetCommandName() => "Catalogue Analysis";
    }
}
