using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    class ExecuteCommandSetProjectsForCatalogueUI : BasicUICommandExecution, IAtomicCommand
    {
        private readonly Catalogue _catalogue;
        private readonly IActivateItems _activator;

        public ExecuteCommandSetProjectsForCatalogueUI(IActivateItems activator, Catalogue catalogue): base(activator)
        {
            _activator = activator;
            _catalogue = catalogue;
        }

        public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
       iconProvider.GetImage(RDMPConcept.ProjectCatalogue, OverlayKind.Edit);

        public override void Execute()
        {
            var ui = new SetProjectsForCatalogueUI(_activator, _catalogue);

            ui.Show();
        }
    }
}
