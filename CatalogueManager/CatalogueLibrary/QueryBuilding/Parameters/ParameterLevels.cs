using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.QueryBuilding.Parameters
{
    /// <summary>
    /// Describes the hierarchical level at which an ISqlParameter was found at by a ParameterManager.
    /// 
    /// Do not reorder these!
    /// </summary>
    public enum ParameterLevel
    {
        /// <summary>
        /// lowest, these are table valued function default values
        /// </summary>
        TableInfo,

        /// <summary>
        /// higher these are explicitly declared properties at the query level e.g. filters, aggregation level (e.g. in the WHERE statements of an AggregateConfiguration on extraction query )
        /// </summary>
        QueryLevel,

        /// <summary>
        /// These are done when joining multiple queries together in an super query (usually separated with set operations such as UNION, EXCEPT etc). See CohortQueryBuilder
        /// </summary>
        CompositeQueryLevel,

        /// <summary>
        /// highest, these are added to the QueryBuilder by the code and should always be preserved, e.g. CohortID is explicitly added by the data export manager.
        /// </summary>
        Global,
    }
}
