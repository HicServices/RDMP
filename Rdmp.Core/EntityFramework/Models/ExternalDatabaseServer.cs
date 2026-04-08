using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.Logging.PastEvents;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ExternalDatabaseServer")]
    public class ExternalDatabaseServer : DatabaseObject, IDataAccessPoint, ICheckable, IExternalDatabaseServer
    {
        [Key]
        public override int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        [Required]
        public string Server { get; set; }

        public string Database { get; set; }
        public string DatabaseType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string CreatedByAssembly { get; set; }
        public string MappedDataPath { get; set; }
        public override string ToString() => Name;


        public string GetDecryptedPassword()
        {
            return "";//todo
        }

        [NotMapped]
        DatabaseType IDataAccessPoint.DatabaseType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public bool DiscoverExistence(DataAccessContext context, out string reason)
        {
            throw new System.NotImplementedException();
        }

        public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
        {
            throw new System.NotImplementedException();
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            throw new System.NotImplementedException();
        }
        public bool WasCreatedBy(IPatcher patcher) => !string.IsNullOrWhiteSpace(CreatedByAssembly) &&
                                              (patcher.Name == CreatedByAssembly ||
                                               patcher.LegacyName == CreatedByAssembly);

        public void Check(ICheckNotifier notifier)
        {
            throw new System.NotImplementedException();
        }

        public DiscoveredDatabase Discover(DataAccessContext context)
        {
            throw new System.NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new System.NotImplementedException();
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new System.NotImplementedException();
        }

        public bool Exists()
        {
            throw new System.NotImplementedException();
        }

        public void SaveToDatabase()
        {
            throw new System.NotImplementedException();
        }

        public DiscoveredServer GetDistinctLoggingDatabase()
        {
            throw new System.NotImplementedException();
        }

        public DiscoveredServer GetDistinctLoggingDatabase(out IExternalDatabaseServer serverChosen)
        {
            throw new System.NotImplementedException();
        }

        public string GetDistinctLoggingTask()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ArchivalDataLoadInfo> FilterRuns(IEnumerable<ArchivalDataLoadInfo> runs)
        {
            throw new System.NotImplementedException();
        }
    }

}
