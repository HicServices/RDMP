using System;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Triggers.Implementations
{
    public class TriggerImplementerFactory
    {
        private readonly DatabaseType _databaseType;

        public TriggerImplementerFactory(DatabaseType databaseType)
        {
            _databaseType = databaseType;
        }

        public ITriggerImplementer Create(DiscoveredTable table, bool createDataLoadRunIDAlso = true)
        {
            switch (_databaseType)
            {
                case DatabaseType.MicrosoftSQLServer:
                    return new MicrosoftSQLTriggerImplementer(table, createDataLoadRunIDAlso);
                case DatabaseType.MYSQLServer:
                    return new MySqlTriggerImplementer(table, createDataLoadRunIDAlso);
                case DatabaseType.Oracle:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException("databaseType");
            }
        }
    }
}