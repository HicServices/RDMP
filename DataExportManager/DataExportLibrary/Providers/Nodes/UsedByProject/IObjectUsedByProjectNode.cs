using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes.UsedByProject
{
    public interface IObjectUsedByProjectNode:IMasqueradeAs
    {
        Project Project { get; }
        object ObjectBeingUsed { get; }
    }
}
