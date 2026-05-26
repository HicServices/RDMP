using FAnsi;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ConnectionStringKeyword")]
    public class ConnectionStringKeyword: DatabaseObject, ICheckable
    {
        private DatabaseType microsoftSQLServer;
        private string v;

        public ConnectionStringKeyword() { }
        public ConnectionStringKeyword(RDMPDbContext catalogueDbContext, DatabaseType microsoftSQLServer, string name, string v)
        {
            CatalogueDbContext = catalogueDbContext;
            this.microsoftSQLServer = microsoftSQLServer;
            Name = name;
            this.v = v;
        }

        [Key]
        public override int ID { get; set; }
        public string DatabaseType { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => Name;
    }
}
