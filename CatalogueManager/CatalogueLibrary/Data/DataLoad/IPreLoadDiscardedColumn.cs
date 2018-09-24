using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <inheritdoc cref="PreLoadDiscardedColumn"/>
    public interface IPreLoadDiscardedColumn : IResolveDuplication,IMapsDirectlyToDatabaseTable
    {
        /// <summary>
        /// Where the RAW column values will end up during a load.  Either dropped completely, diluted into LIVE or routed to an identifier dump
        /// </summary>
        DiscardedColumnDestination Destination { get; set; }

        /// <summary>
        /// The name of the virtual column (that will exist only in RAW).
        /// </summary>
        string RuntimeColumnName { get; set; }

        /// <summary>
        /// The type of the virtual column when creating it in RAW during a data load
        /// </summary>
        string SqlDataType { get; set; }

        #region Relationships

        /// <summary>
        /// The table the virtual column is associated with.  When creating RAW during a DLE execution, all <see cref="IPreLoadDiscardedColumn"/> will be created in addition
        /// to the normal LIVE columns in the <see cref="TableInfo"/> live schema.
        /// </summary>
        [NoMappingToDatabase]
        ITableInfo TableInfo { get; }
        #endregion
    }

    
}