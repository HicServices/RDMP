// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using NSubstitute;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.ReusableLibraryCode.DataAccess;


namespace Tests.Common;

public class RdmpMockFactory
{
    private const string TestLoggingTask = "TestLoggingTask";

    /// <summary>
    /// Generates an implementation of <see cref="INameDatabasesAndTablesDuringLoads"/> which always returns the provided table regardless of the
    /// DLE load stage.
    /// </summary>
    /// <param name="databaseNameToReturn">Database name to return regardless of what <see cref="LoadBubble"/> is asked for by the DLE</param>
    /// <param name="tableNameToReturn">Table name to return regardless of what <see cref="LoadBubble"/> is asked for by the DLE</param>
    /// <returns></returns>
    public static INameDatabasesAndTablesDuringLoads Mock_INameDatabasesAndTablesDuringLoads(
        string databaseNameToReturn, string tableNameToReturn)
    {
        var mock = Substitute.For<INameDatabasesAndTablesDuringLoads>();

        mock.GetDatabaseName(Arg.Any<string>(), Arg.Any<LoadBubble>()).Returns(databaseNameToReturn);
        mock.GetName(Arg.Any<string>(), Arg.Any<LoadBubble>()).Returns(tableNameToReturn);
        return mock;
    }

    /// <inheritdoc cref="Mock_INameDatabasesAndTablesDuringLoads(string, string)"/>
    public static INameDatabasesAndTablesDuringLoads
        Mock_INameDatabasesAndTablesDuringLoads(DiscoveredDatabase databaseNameToReturn, string tableNameToReturn) =>
        Mock_INameDatabasesAndTablesDuringLoads(databaseNameToReturn.GetRuntimeName(), tableNameToReturn);

    /// <summary>
    /// Creates a mock implementation of <see cref="ILoadMetadata"/> which loads the supplied <paramref name="table"/>
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public static ILoadMetadata Mock_LoadMetadataLoadingTable(DiscoveredTable table) =>
        Mock_LoadMetadataLoadingTable(Mock_TableInfo(table));

    /// <summary>
    /// Creates a mock implementation of <see cref="ILoadMetadata"/> which loads the supplied <paramref name="tableInfo"/>
    /// </summary>
    /// <param name="tableInfo"></param>
    /// <returns></returns>
    public static ILoadMetadata Mock_LoadMetadataLoadingTable(ITableInfo tableInfo)
    {
        var lmd = Substitute.For<ILoadMetadata>();
        var cata = Substitute.For<ICatalogue>();
        var server = tableInfo.Discover(DataAccessContext.DataLoad).Database.Server;
        lmd.GetDistinctLiveDatabaseServer().Returns(server);
        lmd.GetAllCatalogues().Returns(new[] { cata });
        lmd.GetDistinctLoggingTask().Returns(TestLoggingTask);

        cata.GetTableInfoList(Arg.Any<bool>()).Returns(new[] { tableInfo });
        cata.LoggingDataTask.Returns(TestLoggingTask);
        return lmd;
    }

    /// <summary>
    /// Creates a mock implementation of <see cref="ITableInfo"/> that points to the live database table <paramref name="table"/>
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public static ITableInfo Mock_TableInfo(DiscoveredTable table)
    {
        var mock = Substitute.For<ITableInfo>();
        mock.Name.Returns(table.GetFullyQualifiedName());
        mock.Database.Returns(table.Database.GetRuntimeName());
        mock.DatabaseType.Returns(table.Database.Server.DatabaseType);
        mock.IsTableValuedFunction.Returns(table.TableType == TableType.TableValuedFunction);
        mock.Discover(Arg.Any<DataAccessContext>()).Returns(table);
        return mock;

    }
}