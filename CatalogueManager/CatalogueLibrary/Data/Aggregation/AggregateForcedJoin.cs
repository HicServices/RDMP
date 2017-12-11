using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers;

namespace CatalogueLibrary.Data.Aggregation
{
    /// <summary>
    /// Describes the requirement to include a given TableInfo in an AggregateConfiguration query even though the TableInfo is not the owner of any of the Columns in the
    /// query (the usual way of deciding which TablesInfos to join).  This is needed if you want a count(*) for example in which both header and result records tables are
    /// joined together. 
    /// </summary>
    public class AggregateForcedJoin
    {
        private readonly CatalogueRepository _repository;

        public AggregateForcedJoin(CatalogueRepository repository)
        {
            _repository = repository;
        }

        public TableInfo[] GetAllForcedJoinsFor(AggregateConfiguration configuration)
        {
            return
                _repository.SelectAllWhere<TableInfo>(
                    "Select TableInfo_ID from AggregateForcedJoin where AggregateConfiguration_ID = " + configuration.ID,
                    "TableInfo_ID").ToArray();
        }

        public void BreakLinkBetween(AggregateConfiguration configuration, TableInfo tableInfo)
        {
            _repository.Delete(string.Format("DELETE FROM AggregateForcedJoin WHERE AggregateConfiguration_ID = {0} AND TableInfo_ID = {1}", configuration.ID, tableInfo.ID));
        }

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
