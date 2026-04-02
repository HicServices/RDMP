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
    [Table("ExtractionConfiguration")]
    public class ExtractionConfiguration: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }

        public int Project_ID { get; set; }

        public string Name { get; set; }

        public override string ToString() => Name;
    }
}
