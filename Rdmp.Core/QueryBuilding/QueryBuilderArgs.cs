using System;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort.Joinables;

namespace Rdmp.Core.QueryBuilding
{
    public class QueryBuilderArgs: QueryBuilderCustomArgs
    {
        public JoinableCohortAggregateConfigurationUse JoinIfAny { get; }
        public AggregateConfiguration JoinedTo { get; }
        public string JoinSql { get; }

        /// <summary>
        /// Creates basic arguments for an <see cref="AggregateConfiguration"/> that does not have a join to a patient index table
        /// </summary>
        public QueryBuilderArgs(QueryBuilderCustomArgs customisations)
        {
            customisations?.Populate(this);
        }

        /// <summary>
        /// Creates arguments for an <see cref="AggregateConfiguration"/> which has a JOIN to a patient index table.  All arguments must be provided
        /// </summary>
        /// <param name="join">The join usage relationship object (includes join direction etc)</param>
        /// <param name="joinedTo">The patient index to which the join is made to (e.g. <see cref="JoinableCohortAggregateConfiguration.AggregateConfiguration"/>)</param>
        /// <param name="joinSql">The full SQL of the join</param>
        public QueryBuilderArgs(JoinableCohortAggregateConfigurationUse join,AggregateConfiguration joinedTo,string joinSql, QueryBuilderCustomArgs customisations):this(customisations)
        {
            JoinIfAny = join;
            JoinedTo = joinedTo;
            JoinSql = joinSql;

            if(JoinIfAny == null !=  (JoinedTo == null )||  JoinIfAny == null != (JoinSql == null))
                throw new Exception("You must provide all arguments or no arguments");

            if(JoinedTo != null)
                if(!JoinedTo.IsCohortIdentificationAggregate || !JoinedTo.IsJoinablePatientIndexTable())
                    throw new ArgumentException($"JoinedTo ({JoinedTo}) was not a patient index table",nameof(joinedTo));
        }
    }
}