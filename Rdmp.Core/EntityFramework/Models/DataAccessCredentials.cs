using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    public class DataAccessCredentials: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public override string ToString() => Name;

    }
}
