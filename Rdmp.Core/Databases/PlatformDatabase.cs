using MapsDirectlyToDatabaseTable.Versioning;

namespace Rdmp.Core.Databases
{
    public sealed class DataQualityEnginePatcher:Patcher
    {
        public DataQualityEnginePatcher() : base(2, "Databases.DataQualityEngineDatabase")
        {
            LegacyName = "DataQualityEngine.Database";
        }
    }
    public sealed class LoggingDatabasePatcher:Patcher
    {
        public LoggingDatabasePatcher():base(2,"Databases.LoggingDatabase")
        {
            LegacyName = "HIC.Logging.Database";
        }
    }

    public sealed class ANOStorePatcher:Patcher
    {
        public ANOStorePatcher():base(2,"Databases.ANOStoreDatabase")
        {
            LegacyName = "ANOStore.Database";
        }
    }

    public sealed class IdentifierDumpDatabasePatcher:Patcher
    {
        public IdentifierDumpDatabasePatcher():base(2,"Databases.IdentifierDumpDatabase")
        {
            LegacyName = "IdentifierDump.Database";
        }
    }

    public sealed class QueryCachingPatcher:Patcher
    {
        public QueryCachingPatcher():base(2,"Databases.QueryCachingDatabase")
        {
            LegacyName = "QueryCaching.Database";
        }
    }

    public sealed class DataExportPatcher:Patcher
    {
        public DataExportPatcher():base(1,"Databases.DataExportDatabase")
        {
            
        }
    }

    public sealed class CataloguePatcher:Patcher
    {
        public CataloguePatcher():base(1,"Databases.CatalogueDatabase")
        {
        }
    }
}
