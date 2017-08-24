using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.DataLoad
{
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