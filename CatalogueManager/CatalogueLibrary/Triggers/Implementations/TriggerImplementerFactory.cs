using System;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Triggers.Implementations
{
    /// <summary>
    /// Handles the creation of the appropriate <see cref="ITriggerImplementer"/> for any given <see cref="DatabaseType"/>
    /// </summary>
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
                    return new MySqlTriggerImplementer(table, createDataLoadRunIDAlso);
                default:
                    throw new ArgumentOutOfRangeException("databaseType");
            }
        }
    }
}