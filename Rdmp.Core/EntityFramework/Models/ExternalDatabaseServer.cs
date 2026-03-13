using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ExternalDatabaseServer")]
    public class ExternalDatabaseServer : DatabaseObject, IDataAccessPoint, ICheckable
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
    }

}
