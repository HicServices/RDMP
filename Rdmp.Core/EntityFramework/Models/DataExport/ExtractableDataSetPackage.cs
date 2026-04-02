using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models.DataExport
{
    [Table("ExtractableDataSetPackage")]
    public class ExtractableDataSetPackage: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }
        public string Name { get; set; }
        public string Creator { get; set; }
        public DateTime CreationDate { get; set; }

        public override string ToString() => Name;

    }
}
