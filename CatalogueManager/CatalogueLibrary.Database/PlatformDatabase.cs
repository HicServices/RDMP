using System;
using System.Collections.Generic;
using System.Text;
using MapsDirectlyToDatabaseTable.Versioning;

namespace CatalogueLibrary.Database
{
    public sealed class DataQualityEnginePatcher:Patcher
    {
        public DataQualityEnginePatcher() : base(2, "DataQualityEngineDatabase")
        {
            LegacyName = "DataQualityEngine.Database";
        }
    }
    public sealed class LoggingDatabasePatcher:Patcher
    {
        public LoggingDatabasePatcher():base(2,"LoggingDatabase")
        {
            LegacyName = "HIC.Logging.Database";
        }
    }

    public sealed class ANOStorePatcher:Patcher
    {
        public ANOStorePatcher():base(2,"ANOStoreDatabase")
        {
            LegacyName = "ANOStore.Database";
        }
    }

    public sealed class IdentifierDumpDatabasePatcher:Patcher
    {
        public IdentifierDumpDatabasePatcher():base(2,"IdentifierDumpDatabase")
        {
            LegacyName = "IdentifierDump.Database";
        }
    }

    public sealed class QueryCachingPatcher:Patcher
    {
        public QueryCachingPatcher():base(2,"QueryCachingDatabase")
        {
            LegacyName = "QueryCaching.Database";
        }
    }

    public sealed class DataExportPatcher:Patcher
    {
        public DataExportPatcher():base(1,"DataExportDatabase")
        {
            
        }
    }

    public sealed class CataloguePatcher:Patcher
    {
        public CataloguePatcher():base(1,"CatalogueDatabase")
        {
        }
    }
            
}
