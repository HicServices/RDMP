using System;
using System.Data.SqlClient;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace RDMPStartup.Events
{
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