using System.Data;
using CatalogueLibrary.DataFlowPipeline;

namespace DataLoadEngine.DataFlowPipeline.Sources
{
    /// <summary>
    /// Reads records in Batches (of size BatchSize) from the remote database (DbConnectionStringBuilder builder) by executing the specified _sql.
    /// </summary>
    public interface IDbDataCommandDataFlowSource:IDataFlowSource<DataTable>
    {
        /// <summary>
        /// Reads and returns a single row.  GetChunk must have been called at least once to function
        /// </summary>
        /// <returns></returns>
        DataRow ReadOneRow();
    }
}