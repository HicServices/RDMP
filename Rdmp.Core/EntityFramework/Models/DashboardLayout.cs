using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("DashboardLayout")]
    public class DashboardLayout: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }

        public string Name { get; set; }
        public string Username { get; set; }
        public DateTime Created { get; set; }
        public override string ToString() => Name;

        public virtual List<DashboardControl> Controls { get; set; }
    }
}
