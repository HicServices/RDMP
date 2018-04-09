using System;
using System.Data.SqlClient;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace RDMPStartup.Events
{
    /// <summary>
    /// Event Args for when the .Database assembly (e.g. CatalogueLibrary.Database) managed database is located during Startup.cs
    /// 
    /// <para>Includes the evaluated status of the database (does it need patching etc) and the Assemblies responsible for managing the database
    /// (The DatabaseAssembly and the HostAssembly - which contains the object definitions).</para>
    /// 
    /// <para>It is important that all platform Databases exactly match the runtime libraries for managing saving/loading objects therefore if the Status is 
    /// RequiresPatching it is imperative that you patch the database and restart the application (happens automatically with StartupUI).</para>
    /// </summary>
    public class PlatformDatabaseFoundEventArgs
    {
        public ITableRepository Repository { get; set; }
        public Assembly HostAssembly { get; set; }
        public Assembly DatabaseAssembly { get; set; }

        public int Tier { get; set; }
        public RDMPPlatformType DatabaseType { get; set; }

        public RDMPPlatformDatabaseStatus Status { get; set; }
        public Exception Exception { get; set; }

        public PlatformDatabaseFoundEventArgs(ITableRepository repository, Assembly hostAssembly, Assembly databaseAssembly, int tier, RDMPPlatformDatabaseStatus status, RDMPPlatformType databaseType,Exception exception=null)
        {
            Repository = repository;
            HostAssembly = hostAssembly;
            DatabaseAssembly = databaseAssembly;
            Tier = tier;
            Status = status;
            Exception = exception;
            DatabaseType = databaseType;
        }

        public string SummariseAsString()
        {
            return "RDMPPlatformDatabaseStatus is " + Status + " for tier " + Tier + " database of type " +
                   DatabaseType + " with connection string " +
                   (Repository == null ? "Unknown" : Repository.ConnectionString) + Environment.NewLine +
                   (Exception == null
                       ? "No exception"
                       : ExceptionHelper.ExceptionToListOfInnerMessages(Exception));
        }
    }
}
