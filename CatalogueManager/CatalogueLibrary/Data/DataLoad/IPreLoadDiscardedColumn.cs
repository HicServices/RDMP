using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// A column which is discarded or diluted during data load.  See PreLoadDiscardedColumn.
    /// </summary>
    public interface IPreLoadDiscardedColumn : IResolveDuplication,IMapsDirectlyToDatabaseTable
    {
        DiscardedColumnDestination Destination { get; set; }
        string RuntimeColumnName { get; set; }
        string SqlDataType { get; set; }

        #region Relationships
        [NoMappingToDatabase]
        ITableInfo TableInfo { get; }
        #endregion
    }

    
}