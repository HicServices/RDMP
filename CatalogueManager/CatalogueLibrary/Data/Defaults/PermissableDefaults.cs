using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Fields that can be set or fetched from the ServerDefaults table in the Catalogue Database
    /// </summary>
    public enum PermissableDefaults
    {
        /// <summary>
        /// Null value/representation
        /// </summary>
        None = 0,

        /// <summary>
        /// Relational logging database to store logs in while loading, running DQE, extracting etc
        /// </summary>
        LiveLoggingServer_ID,

        /// <summary>
        /// Deprecated, use LiveLoggingServer_ID instead.
        /// </summary>
        TestLoggingServer_ID,

        /// <summary>
        /// Server to split sensitive identifiers off to during load (e.g. IdentifierDumper)
        /// </summary>
        IdentifierDumpServer_ID,

        /// <summary>
        /// Server to store the results of running the DQE on datasets over time
        /// </summary>
        DQE,

        /// <summary>
        /// Server to store cached results of <see cref="AggregateConfiguration"/> which are not sensitive and could be shown on a website etc
        /// </summary>
        WebServiceQueryCachingServer_ID,

        /// <summary>
        /// The RAW bubble server in data loads
        /// </summary>
        RAWDataLoadServer,

        /// <summary>
        /// Server to store substituted ANO/Identifiable mappings for sensitive fields during data load e.g. GPCode, CHI, etc.
        /// </summary>
        ANOStore,

        /// <summary>
        /// Server to store cached identifier lists of <see cref="AggregateConfiguration"/>  which are part of a <see cref="CohortIdentificationConfiguration"/> in order
        /// to speed up performance of UNION/INTERSECT/EXCEPT section of the cohort building query.
        /// </summary>
        CohortIdentificationQueryCachingServer_ID
    }
}