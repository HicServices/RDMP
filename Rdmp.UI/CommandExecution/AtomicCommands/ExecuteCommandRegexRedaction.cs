using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandRegexRedaction: BasicUICommandExecution, IAtomicCommand
{
    private readonly Catalogue _catalogue;
    private readonly IActivateItems _activator;

    public ExecuteCommandRegexRedaction(IActivateItems activator, Catalogue catalogue) : base(activator)
    {
        _activator = activator;
        _catalogue = catalogue;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => iconProvider.GetImage(RDMPConcept.StandardRegex);

    public override void Execute()
    {
        base.Execute();

        _activator.Activate<RedactCatalogueUI, Catalogue>(_catalogue);
    }
}
