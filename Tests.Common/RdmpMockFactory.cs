// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Moq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.ReusableLibraryCode.DataAccess;


namespace Tests.Common
{
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
        public static INameDatabasesAndTablesDuringLoads Mock_INameDatabasesAndTablesDuringLoads(string databaseNameToReturn,string tableNameToReturn )
        {
            return  Mock.Of<INameDatabasesAndTablesDuringLoads>(x=>
                x.GetDatabaseName(It.IsAny<string>(), It.IsAny<LoadBubble>())==databaseNameToReturn &&
                x.GetName(It.IsAny<string>(), It.IsAny<LoadBubble>())==tableNameToReturn);

        }

        /// <inheritdoc cref="Mock_INameDatabasesAndTablesDuringLoads(string, string)"/>
        public static INameDatabasesAndTablesDuringLoads Mock_INameDatabasesAndTablesDuringLoads(DiscoveredDatabase databaseNameToReturn, string tableNameToReturn)
        {
            return Mock_INameDatabasesAndTablesDuringLoads(databaseNameToReturn.GetRuntimeName(), tableNameToReturn);
        }

        /// <summary>
        /// Creates a mock implementation of <see cref="ILoadMetadata"/> which loads the supplied <paramref name="table"/>
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static ILoadMetadata Mock_LoadMetadataLoadingTable(DiscoveredTable table)
        {
            return Mock_LoadMetadataLoadingTable(Mock_TableInfo(table));
        }

        /// <summary>
        /// Creates a mock implementation of <see cref="ILoadMetadata"/> which loads the supplied <paramref name="tableInfo"/>
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        public static ILoadMetadata Mock_LoadMetadataLoadingTable(ITableInfo tableInfo)
        {
            var lmd = new Mock<ILoadMetadata>();
            var cata = new Mock<ICatalogue>();

            lmd.Setup(m => m.GetDistinctLiveDatabaseServer()).Returns(tableInfo.Discover(DataAccessContext.DataLoad).Database.Server);
            lmd.Setup(m => m.GetAllCatalogues()).Returns(new[] { cata.Object });
            lmd.Setup(p => p.GetDistinctLoggingTask()).Returns(TestLoggingTask);
            
            cata.Setup(m => m.GetTableInfoList(It.IsAny<bool>())).Returns(new[] { tableInfo });
            cata.Setup(m => m.LoggingDataTask).Returns(TestLoggingTask);
            return lmd.Object;
        }

        /// <summary>
        /// Creates a mock implementation of <see cref="ITableInfo"/> that points to the live database table <paramref name="table"/>
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static ITableInfo Mock_TableInfo(DiscoveredTable table)
        {
            return Mock.Of<ITableInfo>(p=>
                p.Name == table.GetFullyQualifiedName() &&
                p.Database==table.Database.GetRuntimeName() && 
                p.DatabaseType==table.Database.Server.DatabaseType &&
                p.IsTableValuedFunction == (table.TableType == TableType.TableValuedFunction) &&
                p.Discover(It.IsAny<DataAccessContext>())==table);
        }
    }
}
