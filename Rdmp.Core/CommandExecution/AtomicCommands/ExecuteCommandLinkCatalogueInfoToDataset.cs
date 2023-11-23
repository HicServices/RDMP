


using Rdmp.Core.Curation.Data;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;


//There is not currently any way to run this via the CLI
public class ExecuteCommandLinkCatalogueInfoToDataset : BasicCommandExecution, IAtomicCommand
{
    private Catalogue _catalogue;
    private Curation.Data.Dataset _dataset;
    private bool _linkAll;
    public ExecuteCommandLinkCatalogueInfoToDataset(IBasicActivateItems activator, Catalogue catalogue, Curation.Data.Dataset dataset, bool linkAllOtherColumns = true) : base(activator)
    {
        _catalogue = catalogue;
        _dataset = dataset;
        _linkAll = linkAllOtherColumns;
    }


    public override void Execute()
    {
        base.Execute();
        var items = _catalogue.CatalogueItems.ToList();
        foreach (var item in items)
        {
            var ci = item.ColumnInfo;
            if (ci.Dataset_ID == _dataset.ID)
            {
                continue;
            }

            ci.Dataset_ID = _dataset.ID;
            ci.SaveToDatabase();
            if (_linkAll)
            {
                var databaseName = ci.Name[..ci.Name.LastIndexOf('.')];
                var catalogueItems = ci.CatalogueRepository.GetAllObjects<ColumnInfo>().Where(ci => ci.Name[..ci.Name.LastIndexOf(".")] == databaseName);
                foreach (var aci in catalogueItems)
                {
                    aci.Dataset_ID = _dataset.ID;
                    aci.SaveToDatabase();
                }
            }

        }

    }
}