namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources
{
    /// <summary>
    /// Range of strategies to eliminate identical record duplication when extracting data sets from RDMP
    /// </summary>
    public enum DistinctStrategy
    {
        /// <summary>
        /// Do not distinct the records extracted
        /// </summary>
        None = 0,

        /// <summary>
        /// Apply a DISTINCT keyword to the SELECT statement
        /// </summary>
        SqlDistinct,

        /// <summary>
        /// Apply an ORDER BY release id and apply the DISTINCT in memory as batches are read
        /// </summary>
        OrderByAndDistinctInMemory,
        
    }
}