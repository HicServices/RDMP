using System;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data.Defaults
{
    public static class PermissableDefaultsExtensions
    {
        /// <summary>
        /// Translates the given <see cref="PermissableDefaults"/> (a default that can be set) to a <see cref="Tier2DatabaseType"/> (identifies what type of database it is).
        /// </summary>
        /// <param name="permissableDefault"></param>
        /// <returns></returns>
        public static Tier2DatabaseType? ToTier2DatabaseType(this PermissableDefaults permissableDefault)
        {
            switch (permissableDefault)
            {
                case PermissableDefaults.LiveLoggingServer_ID:
                    return Tier2DatabaseType.Logging;
                case PermissableDefaults.TestLoggingServer_ID:
                    return Tier2DatabaseType.Logging;
                case PermissableDefaults.IdentifierDumpServer_ID:
                    return Tier2DatabaseType.IdentifierDump;
                case PermissableDefaults.DQE:
                    return Tier2DatabaseType.DataQuality;
                case PermissableDefaults.WebServiceQueryCachingServer_ID:
                    return Tier2DatabaseType.QueryCaching;
                case PermissableDefaults.CohortIdentificationQueryCachingServer_ID:
                    return Tier2DatabaseType.QueryCaching;
                case PermissableDefaults.RAWDataLoadServer:
                    return null;
                case PermissableDefaults.ANOStore:
                    return Tier2DatabaseType.ANOStore;
                default:
                    throw new ArgumentOutOfRangeException("permissableDefault");
            }
        }
    }
}