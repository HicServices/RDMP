// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.Curation.Data.Aggregation;

internal class AggregateForcedJoin : IAggregateForcedJoinManager
{
    private readonly CatalogueRepository _repository;

    /// <summary>
    ///     Creates a new instance targetting the catalogue database referenced by the repository.  The instance can be used to
    ///     populate / edit the AggregateForcedJoin in
    ///     the database.  Access via <see cref="CatalogueRepository.AggregateForcedJoinManager" />
    /// </summary>
    /// <param name="repository"></param>
    internal AggregateForcedJoin(CatalogueRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public ITableInfo[] GetAllForcedJoinsFor(AggregateConfiguration configuration)
    {
        var everyone = Enumerable.Empty<ITableInfo>();

        // join with everyone? .... what do you mean everyone? EVERYONE!!!!
        if (UserSettings.AlwaysJoinEverything)
            everyone = configuration.Catalogue.GetTableInfosIdeallyJustFromMainTables();

        return
            _repository.SelectAllWhere<TableInfo>(
                $"Select TableInfo_ID from AggregateForcedJoin where AggregateConfiguration_ID = {configuration.ID}",
                "TableInfo_ID").Union(everyone).ToArray();
    }

    /// <inheritdoc />
    public void BreakLinkBetween(AggregateConfiguration configuration, ITableInfo tableInfo)
    {
        _repository.Delete(
            $"DELETE FROM AggregateForcedJoin WHERE AggregateConfiguration_ID = {configuration.ID} AND TableInfo_ID = {tableInfo.ID}");
    }

    /// <inheritdoc />
    public void CreateLinkBetween(AggregateConfiguration configuration, ITableInfo tableInfo)
    {
        using var con = _repository.GetConnection();
        using var cmd = DatabaseCommandHelper.GetCommand(
            $"INSERT INTO AggregateForcedJoin (AggregateConfiguration_ID,TableInfo_ID) VALUES ({configuration.ID},{tableInfo.ID})",
            con.Connection, con.Transaction);
        cmd.ExecuteNonQuery();
    }
}