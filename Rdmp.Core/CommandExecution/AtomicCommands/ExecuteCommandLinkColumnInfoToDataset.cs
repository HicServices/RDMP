


using Rdmp.Core.Curation.Data;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandLinkColumnInfoToDataset : BasicCommandExecution
{
    private ColumnInfo _columnInfo;
    private Curation.Data.Dataset _dataset;
    private bool _linkAll;
    public ExecuteCommandLinkColumnInfoToDataset(IBasicActivateItems activator, [DemandsInitialization("The column to link")] ColumnInfo columnInfo, [DemandsInitialization("The dataset to link to")] Curation.Data.Dataset dataset, bool linkAllOtherColumns = true) : base(activator)
    {
        _columnInfo = columnInfo;
        _dataset = dataset;
        _linkAll = linkAllOtherColumns;
    }


    public override void Execute()
    {
        base.Execute();
        _columnInfo.Dataset_ID = _dataset.ID;
        _columnInfo.SaveToDatabase();
        if (_linkAll)
        {
            var databaseName = _columnInfo.Name[.._columnInfo.Name.LastIndexOf('.')];
            var catalogueItems = _columnInfo.CatalogueRepository.GetAllObjects<ColumnInfo>().Where(ci => ci.Name[..ci.Name.LastIndexOf(".")] == databaseName);
            foreach (var ci in catalogueItems)
            {
                ci.Dataset_ID = _dataset.ID;
                ci.SaveToDatabase();
            }
        }
    }
}