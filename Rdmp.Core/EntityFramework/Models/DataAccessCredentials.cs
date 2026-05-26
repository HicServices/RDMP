using Rdmp.Core.Curation.Data;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    public class DataAccessCredentials: DatabaseObject, IDataAccessCredentials
    {
        public DataAccessCredentials() { }
        public DataAccessCredentials(RDMPDbContext catalogueDbContext, string v)
        {
        }

        [Key]
        public override int ID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public override string ToString() => Name;

        internal Dictionary<DataAccessContext, List<ITableInfo>> GetAllTableInfosThatUseThis()
        {
            throw new NotImplementedException();
        }

        public string GetDecryptedPassword()
        {
            throw new NotImplementedException();
        }

        internal bool PasswordIs(string password)
        {
            throw new NotImplementedException();
        }

        string IEncryptedPasswordHost.GetDecryptedPassword()
        {
            return GetDecryptedPassword();
        }
    }
}
