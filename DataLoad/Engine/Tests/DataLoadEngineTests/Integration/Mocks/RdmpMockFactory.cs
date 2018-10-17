using CatalogueLibrary.Data;
using CatalogueLibrary.Data.EntityNaming;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Rhino.Mocks;

namespace DataLoadEngineTests.Integration.Mocks
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
    }
}
