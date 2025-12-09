using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandDeleteDatasetUI : BasicUICommandExecution
{
    private readonly IActivateItems _activateItems;
    private readonly Dataset _dataset;

    public ExecuteCommandDeleteDatasetUI(IActivateItems activator, Dataset dataset) : base(activator)
    {
        _dataset = dataset;
        _activateItems = activator;
    }

    public override string GetCommandHelp() =>
       "Delete this dataset and remove all links to it within RDMP";

    public override void Execute()
    {
        base.Execute();
        var confirmDelete = YesNo( $"Are you sure you want to delete the dataset \"{_dataset.Name}\"?", $"Delete Dataset: {_dataset.Name}");
        if (!confirmDelete) return;

        var cmd = new Core.CommandExecution.AtomicCommands.ExecuteCommandDeleteDataset(_activateItems, _dataset);
        cmd.Execute();
        _activateItems.RefreshBus.Publish(this, new Refreshing.RefreshObjectEventArgs(_dataset));
    }


    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Dataset, OverlayKind.Delete);
}
