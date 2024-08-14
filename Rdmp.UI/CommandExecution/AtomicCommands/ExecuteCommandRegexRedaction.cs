using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ExtractionUIs;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandRegexRedaction: BasicUICommandExecution, IAtomicCommand
{
    private readonly Catalogue _catalogue;

    public ExecuteCommandRegexRedaction(IActivateItems activator, Catalogue catalogue) : base(activator)
    {
        _catalogue = catalogue;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => iconProvider.GetImage(RDMPConcept.StandardRegex);

    public override void Execute()
    {
        base.Execute();

        //Activator.Activate<ReOrderCatalogueItemsUI, Catalogue>(_catalogue);
    }
}
