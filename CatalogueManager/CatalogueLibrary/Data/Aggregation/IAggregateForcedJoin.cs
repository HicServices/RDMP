namespace CatalogueLibrary.Data.Aggregation
{
    /// <summary>
    /// Describes the requirement to include a given TableInfo in an AggregateConfiguration query even though the TableInfo is not the owner of any of the Columns in the
    /// query (the usual way of deciding which TableInfos to join).  This is needed if you want a count(*) for example in which both header and result records tables are
    /// joined together. 
    /// </summary>
    public interface IAggregateForcedJoin
    {
        /// <summary>
        /// Returns all the TableInfos that the provided <see cref="AggregateConfiguration"/> has been explicitly requested (by the user) to join to in it's FROM section (See 
        /// <see cref="CatalogueLibrary.QueryBuilding.AggregateBuilder"/>. 
        /// 
        /// <para>This set will be combined with those that would already be joined against because of the <see cref="AggregateDimension"/> configured.  Note that your query results
        /// in multiple TableInfos being needed then you will still need to have defined a way for the TableInfos to be joined (See <see cref="JoinInfo"/>.</para>
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        TableInfo[] GetAllForcedJoinsFor(AggregateConfiguration configuration);

        /// <summary>
        /// Deletes the mandate that the provided AggregateConfiguration should always join with the specified TableInfo regardless of what <see cref="AggregateDimension"/> are
        /// configured.  This will have no effect if there was no forced join declared in the first place.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="tableInfo"></param>
        void BreakLinkBetween(AggregateConfiguration configuration, TableInfo tableInfo);

        /// <summary>
        /// Creates the mandate that the provided AggregateConfiguration should always join with the specified TableInfo regardless of what <see cref="AggregateDimension"/> are
        /// configured (See <see cref="CatalogueLibrary.QueryBuilding.AggregateBuilder"/>. 
        /// 
        /// <para>Note that your query results in multiple TableInfos being needed then you will still need to have defined a way for the TableInfos to be joined (See <see cref="JoinInfo"/>.</para>
        /// </summary>
        /// <seealso cref="AggregateForcedJoin.GetAllForcedJoinsFor"/>
        /// <param name="configuration"></param>
        /// <param name="tableInfo"></param>
        void CreateLinkBetween(AggregateConfiguration configuration, TableInfo tableInfo);
    }
}