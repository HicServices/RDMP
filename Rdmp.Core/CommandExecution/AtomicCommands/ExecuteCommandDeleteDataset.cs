using System.Linq;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandDeleteDataset : BasicCommandExecution
{
    private readonly Curation.Data.Dataset _dataset;
    private readonly IBasicActivateItems _activator;

    public ExecuteCommandDeleteDataset(IBasicActivateItems activator,
        [DemandsInitialization("The Dataset to delete")] Curation.Data.Dataset dataset)
    {
        _dataset = dataset;
        _activator = activator;
    }

    public override void Execute()
    {
        base.Execute();
        var columnItemsLinkedToDataset = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ColumnInfo>()
            .Where(cif => cif.Dataset_ID == _dataset.ID);
        foreach (var col in columnItemsLinkedToDataset)
        {
            col.Dataset_ID = null;
            col.SaveToDatabase();
        }

        _dataset.DeleteInDatabase();
    }
}