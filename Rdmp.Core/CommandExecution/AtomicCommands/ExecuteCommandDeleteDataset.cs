using Rdmp.Core.Curation.Data;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;
public sealed class ExecuteCommandDeleteDataset: BasicCommandExecution
{
    private readonly Curation.Data.Datasets.Dataset _dataset;
    private readonly IBasicActivateItems _activator;
public ExecuteCommandDeleteDataset(IBasicActivateItems activator, [DemandsInitialization("The Dataset to delete")]Curation.Data.Datasets.Dataset dataset)
    {
        _dataset = dataset;
        _activator = activator;
    }

    public override void Execute()
    {
        base.Execute();
        var columnItemsLinkedToDataset = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ColumnInfo>().Where(cif => cif.Dataset_ID == _dataset.ID);
        foreach (var col in columnItemsLinkedToDataset)
        {
            col.Dataset_ID = null;
            col.SaveToDatabase();
        }
        _dataset.DeleteInDatabase();
    }
}
