using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("RemoteRDMP")]
    public class RemoteRDMP: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }
        public string Name { get; set; }
        public string URL   { get; set; }
        public string UserName { get; set; }
        public string Username { get; internal set; }
        public string Password { get; set; }

        internal string GetDecryptedPassword()
        {
            throw new NotImplementedException();
        }

        internal string GetUrlFor<T>() where T : IMapsDirectlyToDatabaseTable
        {
            throw new NotImplementedException();
        }
    }
}
