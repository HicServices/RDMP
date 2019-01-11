using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Aggregation
{
    /// <summary>
    /// Describes the requirement to include a given TableInfo in an AggregateConfiguration query even though the TableInfo is not the owner of any of the Columns in the
    /// query (the usual way of deciding which TableInfos to join).  This is needed if you want a count(*) for example in which both header and result records tables are
    /// joined together. 
    /// </summary>
    public class AggregateForcedJoin
    {
        private readonly CatalogueRepository _repository;

        /// <summary>
        /// Creates a new instance targetting the catalogue database referenced by the repository.  The instance can be used to populate / edit the AggregateForcedJoin in 
        /// the database.  Access via <see cref="CatalogueRepository.AggregateForcedJoiner"/>
        /// </summary>
        /// <param name="repository"></param>
        internal AggregateForcedJoin(CatalogueRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Returns all the TableInfos that the provided <see cref="AggregateConfiguration"/> has been explicitly requested (by the user) to join to in it's FROM section (See 
        /// <see cref="CatalogueLibrary.QueryBuilding.AggregateBuilder"/>. 
        /// 
        /// <para>This set will be combined with those that would already be joined against because of the <see cref="AggregateDimension"/> configured.  Note that your query results
        /// in multiple TableInfos being needed then you will still need to have defined a way for the TableInfos to be joined (See <see cref="JoinInfo"/>.</para>
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public TableInfo[] GetAllForcedJoinsFor(AggregateConfiguration configuration)
        {
            return
                _repository.SelectAllWhere<TableInfo>(
                    "Select TableInfo_ID from AggregateForcedJoin where AggregateConfiguration_ID = " + configuration.ID,
                    "TableInfo_ID").ToArray();
        }

        /// <summary>
        /// Deletes the mandate that the provided AggregateConfiguration should always join with the specified TableInfo regardless of what <see cref="AggregateDimension"/> are
        /// configured.  This will have no effect if there was no forced join declared in the first place.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="tableInfo"></param>
        public void BreakLinkBetween(AggregateConfiguration configuration, TableInfo tableInfo)
        {
            _repository.Delete(string.Format("DELETE FROM AggregateForcedJoin WHERE AggregateConfiguration_ID = {0} AND TableInfo_ID = {1}", configuration.ID, tableInfo.ID));
        }

        /// <summary>
        /// Creates the mandate that the provided AggregateConfiguration should always join with the specified TableInfo regardless of what <see cref="AggregateDimension"/> are
        /// configured (See <see cref="CatalogueLibrary.QueryBuilding.AggregateBuilder"/>. 
        /// 
        /// <para>Note that your query results in multiple TableInfos being needed then you will still need to have defined a way for the TableInfos to be joined (See <see cref="JoinInfo"/>.</para>
        /// </summary>
        /// <seealso cref="GetAllForcedJoinsFor"/>
        /// <param name="configuration"></param>
        /// <param name="tableInfo"></param>
        public void CreateLinkBetween(AggregateConfiguration configuration, TableInfo tableInfo)
        {
            using (var con = _repository.GetConnection())
                DatabaseCommandHelper.GetCommand(
                    string.Format(
                        "INSERT INTO AggregateForcedJoin (AggregateConfiguration_ID,TableInfo_ID) VALUES ({0},{1})",
                        configuration.ID, tableInfo.ID), con.Connection,con.Transaction).ExecuteNonQuery();
        }
    }
}
