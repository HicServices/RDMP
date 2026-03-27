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
    [Table("Setting")]
    public class Setting: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
