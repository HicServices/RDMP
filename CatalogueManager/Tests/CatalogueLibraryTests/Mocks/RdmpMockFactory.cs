// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms.VisualStyles;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using FAnsi.Discovery;
using ReusableLibraryCode.DataAccess;
using Rhino.Mocks;

namespace CatalogueLibraryTests.Mocks
{
    public class RdmpMockFactory
    {
        public static INameDatabasesAndTablesDuringLoads Mock_INameDatabasesAndTablesDuringLoads(string databaseNameToReturn,string tableNameToReturn )
        {
            var mock = MockRepository.GenerateMock<INameDatabasesAndTablesDuringLoads>();

            mock.Stub(x => x.GetDatabaseName(null, LoadBubble.Live)).IgnoreArguments().Return(databaseNameToReturn);
            mock.Stub(x => x.GetName(null, LoadBubble.Live)).IgnoreArguments().Return(tableNameToReturn);
            return mock;
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
            var lmd = MockRepository.GenerateMock<ILoadMetadata>();
            var cata = MockRepository.GenerateMock<ICatalogue>();

            lmd.Stub(m => m.GetDistinctLiveDatabaseServer()).Return(tableInfo.Discover(DataAccessContext.DataLoad).Database.Server);
            lmd.Stub(m => m.GetAllCatalogues()).Return(new[] { cata });

            cata.Stub(m => m.GetTableInfoList(true)).IgnoreArguments().Return(new[] { tableInfo });

            return lmd;
        }

        public static ITableInfo Mock_TableInfo(DiscoveredTable table)
        {
            var ti = MockRepository.GenerateMock<ITableInfo>();

            ti.Stub(p => p.Name).Return(table.GetFullyQualifiedName());
            ti.Stub(p => p.Database).Return(table.Database.GetRuntimeName());
            ti.Stub(p => p.DatabaseType).Return(table.Database.Server.DatabaseType);
            ti.Stub(p => p.IsTableValuedFunction).Return(table.TableType == TableType.TableValuedFunction);

            ti.Stub(m => m.Discover(DataAccessContext.Any)).IgnoreArguments().Return(table);
            
            return ti;
        }
    }
}
