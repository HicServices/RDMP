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
