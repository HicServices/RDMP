// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using FAnsi.Discovery;
using Moq;
using ReusableLibraryCode.DataAccess;


namespace Tests.Common
{
    public class RdmpMockFactory
    {
        public static INameDatabasesAndTablesDuringLoads Mock_INameDatabasesAndTablesDuringLoads(string databaseNameToReturn,string tableNameToReturn )
        {
            return  Mock.Of<INameDatabasesAndTablesDuringLoads>(x=>
                x.GetDatabaseName(It.IsAny<string>(), It.IsAny<LoadBubble>())==databaseNameToReturn &&
                x.GetName(It.IsAny<string>(), It.IsAny<LoadBubble>())==tableNameToReturn);

        }

        public static INameDatabasesAndTablesDuringLoads Mock_INameDatabasesAndTablesDuringLoads(DiscoveredDatabase databaseNameToReturn, string tableNameToReturn)
        {
            return Mock_INameDatabasesAndTablesDuringLoads(databaseNameToReturn.GetRuntimeName(), tableNameToReturn);
        }

        public static ILoadMetadata Mock_LoadMetadataLoadingTable(DiscoveredTable table)
        {
            return Mock_LoadMetadataLoadingTable(Mock_TableInfo(table));
        }

        public static ILoadMetadata Mock_LoadMetadataLoadingTable(ITableInfo tableInfo)
        {
            var lmd = new Mock<ILoadMetadata>();
            var cata = new Mock<ICatalogue>();

            lmd.Setup(m => m.GetDistinctLiveDatabaseServer()).Returns(tableInfo.Discover(DataAccessContext.DataLoad).Database.Server);
            lmd.Setup(m => m.GetAllCatalogues()).Returns(new[] { cata.Object });

            cata.Setup(m => m.GetTableInfoList(It.IsAny<bool>())).Returns(new[] { tableInfo });

            return lmd.Object;
        }

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
